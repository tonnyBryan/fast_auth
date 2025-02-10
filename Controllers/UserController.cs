using fast_auth.annotation;
using fast_auth.model.dto;
using fast_auth.model.tiers;
using fast_auth.service;
using fast_authenticator.model;
using fast_authenticator.util;
using Microsoft.AspNetCore.Mvc;

using FirebaseAdmin.Auth;
using FirebaseAdmin;


namespace fast_authenticator.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppService _service;
        private readonly TokenService _tokenService;
        private static readonly string SECRET_KEY = "12345678901234567890123456789012";


        public UserController(AppService service)
        {
            _service = service;

            string secretKey = SECRET_KEY;
            string issuer = "myApp";
            string audience = "myAppUsers";

            _tokenService = new TokenService(secretKey, issuer, audience);
        }


        [HttpPost("register")]
        async public Task<IActionResult> Register([FromBody] UserRegistrationDTO registrationDTO)
        {
            if (_service.EmailExist(registrationDTO.Email))
            {
                Error err = new(500);
                // Unauthorized
                return Ok(new ApiResponse<string>(401, "User with this email already exist", err));
            }

            if (registrationDTO.Password.Length < 7 ||
                !registrationDTO.Password.Any(char.IsUpper) ||  
                !registrationDTO.Password.Any(char.IsDigit) ||  
                !registrationDTO.Password.Any(ch => !char.IsLetterOrDigit(ch))) 
            {
                Error err = new(500);
                return Ok(new ApiResponse<string>(401, "Password must contain at least 7 characters, an uppercase letter, a number and a special character.", err));
            }

            //var hashedPassword = AppUtil.Crypt(registrationDTO.Password);
            var hashedPassword = AppUtil.Encrypt(registrationDTO.Password, SECRET_KEY);
            var status = _service.FindStatus("attente");

            var newUser = new User
            {
                Username = registrationDTO.Username,
                Email = registrationDTO.Email,
                Password = hashedPassword, 
                NbTentative = 0,
                IdStatus = status.IdStatus,
                DateCreation = default
            };

            _service.AddUser(newUser);
            _service.PushData();

            string hashKey = AppUtil.GenerateSecretUniqueKey(newUser);
            UniqueKey uK = new UniqueKey(hashKey, newUser.IdUser);
            _service.AddUniqueKey(uK);

            string url = _service.GetConfirmUrl(uK, "localhost", "5000");
            string htmlBody = AppUtil.GenerateEmailHtmlConfirmation(hashKey, url);
            try
            {
                MailSender.SendEmail(newUser.Email, "Fast_Auth - Account Confirmation", htmlBody);
            } catch (Exception)
            {
                _service.RemoveUniqueKey(uK);
                _service.RemoveUser(newUser);
                _service.PushData();
                Error err = new(500);
                // internal server error
                return Ok(new ApiResponse<string>(500, "Error Connection. Make sure you are connected and try again!", err));
            }

            try
            {
                UserRecordArgs userArgs = new UserRecordArgs()
                {
                    Email = registrationDTO.Email,
                    Password = registrationDTO.Password,
                    EmailVerified = false, 
                    Disabled = false    
                };

                UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
            }
            catch (FirebaseAuthException ex)
            {
                _service.RemoveUniqueKey(uK);
                _service.RemoveUser(newUser);
                _service.PushData();

                Error err = new(500);
                // Unauthorized
                return Ok(new ApiResponse<string>(401, ex.Message, err));
            }

            _service.PushData();
            return Ok(new ApiResponse<string>(true, 200, "Done! Confirm your mail"));
        }

        [HttpPut("confirm")]
        async public Task<IActionResult> Confirm([FromQuery(Name = "key")] string key)
        {

            var uKey = _service.FindUniqueKey(key);
            if (uKey == null)
            {
                Error err = new(500);
                // Unauthorized
                return Ok(new ApiResponse<string>(401, "Invalid confirmation key", err));
            }

            var user = _service.FindUserById(uKey.IdUser);

            if (user == null)
            {
                Error err = new(500);
                // bad request
                return Ok(new ApiResponse<string>(405, "User not found", err));
            }

            _service.RemoveUniqueKey(uKey);

            var status = _service.FindStatus("attente");
            if (user.IdStatus != status.IdStatus)
            {
                return Ok(new ApiResponse<string>(200, "User already verified"));
            }

            var statusNormal = _service.FindStatus("normal");
            user.IdStatus = statusNormal.IdStatus;

            user.DateCreation = DateTime.UtcNow;

            try
            {
                UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(user.Email);
                UserRecordArgs updateArgs = new UserRecordArgs()
                {
                    Uid = userRecord.Uid,
                    EmailVerified = true 
                };

                await FirebaseAuth.DefaultInstance.UpdateUserAsync(updateArgs);
            }
            catch (FirebaseAuthException ex)
            {
                return Ok(new ApiResponse<string>(401, ex.Message, new Error(500)));
            }

            Token token = new()
            {
                Key = _tokenService.GenerateToken(user.Username),
                DateCreation = DateTime.UtcNow,
                DateExpiration = DateTime.UtcNow.AddHours(1),
                IdUser = user.IdUser
            };

            _service.AddToken(token);
            _service.PushData();

            SuccessAuth successAuth = new(user, token.Key);
            _service.HidePassword(user);
            return Ok(new ApiResponse<SuccessAuth>(200, successAuth, "confirmed"));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO loginDTO)
        {
            var user = _service.FindUserByEmail(loginDTO.Email);
            if (user == null)
            {
                Error err = new(500);
                return Ok(new ApiResponse<string>(401, "Specified Email does not exist!", err));
            }

            bool passwordMatches = _service.IsPasswordSame(loginDTO.Password, AppUtil.Decrypt(user.Password, SECRET_KEY));
            var statusBloque = _service.FindStatus("bloque");
            if (!passwordMatches)
            {
                if (user.NbTentative >= 3 - 1)
                {
                    user.IdStatus = statusBloque.IdStatus;
                    _service.PushData();

                    Error err = new(550);
                    // Unauthorized
                    return Ok(new ApiResponse<string>(401, "Too many wrong password, your account is now blocked. Request a key cleaner by login!", err));
                }

                user.NbTentative++;
                _service.PushData();

                Error err2 = new(500);
                // Unauthorized
                return Ok(new ApiResponse<string>(401, "Oups! Wrong password", err2));
            }

            if (user.IdStatus == statusBloque.IdStatus)
            {
                Error err = new(550);
                // Unauthorized
                return Ok(new ApiResponse<string>(401, "Sorry, this user has been blocked. Request a key cleaner by login!", err));
            }

            var statusAttente = _service.FindStatus("attente");
            if (user.IdStatus == statusAttente.IdStatus)
            {
                Error err = new(555);
                // Unauthorized
                return Ok(new ApiResponse<string>(401, "Email not verified. Check your email to get code", err));
            }

            string pin = AppUtil.GeneratePin(6);
            Authentification auth = new()
            {
                Pin = pin,
                IdUser = user.IdUser,
                User = user,
                Expiration = DateTime.UtcNow.AddSeconds(90),
                AKey = AppUtil.GenerateSecretUniqueKey(user)
            };

            try
            {
                MailSender.SendEmail(user.Email, "Fast_auth - Pin Code", AppUtil.GeneratePinDiv(pin));
            } catch (Exception)
            {
                Error err = new(500);
                // internal server error
                return Ok(new ApiResponse<string>(500, "Error Connection. Make sure you are connected and try again!", err));
            }

            _service.RemoveAuthentificationsOf(user);
            _service.AddAuthentification(auth);
            _service.PushData();

            return Ok(new ApiResponse<string>(200, auth.AKey, "Verify your email box to get pin code")); 
        }

        [HttpPut("auth")]
        public IActionResult Auth([FromBody] UserAuthDTO authDTO)
        {
            var authData = _service.FindAuthentificationByKey(authDTO.AKey);
            if (authData == null)
            {
                Error err = new(500);
                // 401
                return Ok(new ApiResponse<string>(401, "Authentification not found", err));
            }

            if (_service.IsExpired(authData.Expiration))
            {
                Error err = new(500);
                // 401
                return Ok(new ApiResponse<string>(401, "Authentification expired", err));
            }

            var user = _service.FindUserById(authData.IdUser);
            if (user == null) 
            {
                Error err = new(500);
                // 401
                return Ok(new ApiResponse<string>(401, "Authentification invalid", err));
            }

            var statusBloque = _service.FindStatus("bloque");
            if (user.IdStatus == statusBloque.IdStatus)
            {
                Error err = new(550);
                // 401
                return Ok(new ApiResponse<string>(401, "This user has been blocked", err));
            }

            if (authDTO.Pin != authData.Pin) 
            {
                user.NbTentative++;

                if (user.NbTentative == 3)
                {
                    user.IdStatus = statusBloque.IdStatus;
                    _service.PushData();

                    Error err = new(550);
                    // 401
                    return Ok(new ApiResponse<string>(401, "Too many wrong password, your account is blocked", err));
                }

                _service.PushData();

                Error err2 = new(500);
                // 401
                return Ok(new ApiResponse<string>(401, "Invalid pin code", err2));
            }

            if (user.NbTentative > 0)
            {
                user.NbTentative = 0;
            }

            _service.RemoveAuthentification(authData);

            Token token = new Token();
            token.Key = _tokenService.GenerateToken(user.Username);
            token.DateCreation = DateTime.UtcNow;
            token.DateExpiration = DateTime.UtcNow.AddHours(10);
            token.IdUser = user.IdUser;

            _service.AddToken(token);
            _service.PushData();

            SuccessAuth successAuth = new SuccessAuth(user, token.Key);
            _service.HidePassword(user);
            return Ok(new ApiResponse<SuccessAuth>(200, successAuth));
        }

        [HttpPost("sendReset")]
        public IActionResult Reset([FromBody] UserMailDTO mailDTO)
        {
            var user = _service.FindUserByEmail(mailDTO.Email);
            if (user == null)
            {
                Error err = new(500);
                // unauthorized
                return Ok(new ApiResponse<string>(401, "Email not found", err));
            }

            var statusBloque = _service.FindStatus("bloque");
            if (user.IdStatus == statusBloque.IdStatus)
            {
                _service.FindResetEmailRequestByEmailAndRemove(mailDTO.Email);  

                string hashKey = AppUtil.GenerateSecretUniqueKey(user);

                ResetEmailRequest req = new ResetEmailRequest(mailDTO.Email , hashKey);
                _service.AddResetEmailRequest(req);

                string url = _service.GetResetUrl(hashKey, "localhost", "5000");
                string htmlBody = AppUtil.GenerateEmailHtmlConfirmation(hashKey, url);
                try
                {
                    MailSender.SendEmail(user.Email, "Fast_auth - Key cleaner", htmlBody);
                } catch (Exception)
                {
                    Error err = new(500);
                    // internam server error
                    return Ok(new ApiResponse<string>(500, "Error Connection. Make sure you are connected and try again!", err));
                }
            } else
            {
                Error err = new(500);
                // 401
                return Ok(new ApiResponse<string>(401, "Blocked Account, not accepted", err));
            }

            _service.PushData();
            return Ok(new ApiResponse<string>(true, 200, "Email sended"));
        }


        [HttpPut("reset")]
        public IActionResult Reset([FromQuery(Name = "key")] string key)
        {
            var resetReq = _service.FindResetEmailRequest(key);
            if (resetReq == null)
            {
                Error err = new(500);
                // unauthorized
                return Ok(new ApiResponse<string>(401, "Key cleaner not found", err));
            }

            var statusBloque = _service.FindStatus("bloque");
            var statusNormal = _service.FindStatus("normal");

            var user = _service.FindUserByEmail(resetReq.Email);
            if (user == null)
            {
                Error err = new(500);
                // unauthorized
                return Ok(new ApiResponse<string>(401, "An error has occured, try again later!", err));
            }

            if (user.IdStatus != statusBloque.IdStatus)
            {
                Error err = new(500);
                // internal server error
                return Ok(new ApiResponse<string>(500, "Unauthorized key cleaner", err));
            }

            user.NbTentative = 0;
            user.IdStatus = statusNormal.IdStatus;

            _service.RemoveResetEmailRequest(resetReq);
            _service.PushData();

            return Ok(new ApiResponse<string>(true, 200, "Account clean. GG!"));
        }
    }
}
