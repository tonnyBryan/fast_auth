using fast_auth.annotation;
using fast_auth.model.dto;
using fast_auth.model.tiers;
using fast_auth.service;
using fast_authenticator.model;
using fast_authenticator.util;
using Microsoft.AspNetCore.Mvc;

namespace fast_authenticator.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppService _service;
        private readonly TokenService _tokenService;


        public UserController(AppService service)
        {
            _service = service;

            string secretKey = "123456789101112131415161718192021222324";
            string issuer = "myApp";
            string audience = "myAppUsers";

            _tokenService = new TokenService(secretKey, issuer, audience);
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            try
            {
                IEnumerable<User> usersList = _service.FindAllUser();
                return Ok(new ApiResponse<IEnumerable<User>>(200, usersList)); 
            }
            catch (Exception ex)
            {
                Error err = new(500);
                return StatusCode(500, new ApiResponse<string>(500, $"Erreur lors de la récupération des utilisateurs: {ex.Message}", err));
            }
        }

        [TokenRequired]
        [HttpPut]
        public IActionResult Update([FromBody] UserModificationDTO userDTO)
        {
            var user = _service.FindUserById(userDTO.IdUser);
            if (user == null)
            {
                Error err = new(500);
                return Unauthorized(new ApiResponse<string>(401, "Utilisateur introuvable", err));
            }

            bool modified = false;

            if (!string.IsNullOrEmpty(userDTO.Username))
            {
                modified = true;
                user.Username = userDTO.Username;
            }

            if (!string.IsNullOrEmpty(userDTO.Password))
            {
                modified = true;
                user.Password = AppUtil.Crypt(userDTO.Password);
            }

            if (modified)
            {
                _service.PushData();
            }

            return Ok(new ApiResponse<User>(200, user, "modifié"));
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegistrationDTO registrationDTO)
        {
            if (_service.EmailExist(registrationDTO.Email))
            {
                Error err = new(500);
                return Unauthorized(new ApiResponse<string>(401, "Un utilisateur avec cet email existe déjà.", err));
            }

            var hashedPassword = AppUtil.Crypt(registrationDTO.Password);
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
            string url = _service.GetConfirmUrl(uK, "localhost", "5106");

            try
            {
                MailSender.SendEmail(newUser.Email, "Confirmation Mail", url);
            } catch (Exception)
            {
                _service.RemoveUniqueKey(uK);
                _service.RemoveUser(newUser);
                _service.PushData();

                Error err = new(500);
                return StatusCode(500, new ApiResponse<string>(500, "Email Sending failed", err));
            }

            _service.PushData();
            return Ok(new ApiResponse<string>(true, 200, "Reussi! Confirmez votre mail"));
        }

        [HttpPost("confirm")]
        public IActionResult Auth([FromQuery(Name = "key")] string key)
        {
            var uKey = _service.FindUniqueKey(key);
            if (uKey == null)
            {
                Error err = new(500);
                return Unauthorized(new ApiResponse<string>(401, "Clé Invalide", err));
            }

            var user = _service.FindUserById(uKey.IdUser);

            if (user == null)
            {
                Error err = new(500);
                return BadRequest(new ApiResponse<string>(405, "Utilisateur inexistant", err));
            }

            _service.RemoveUniqueKey(uKey);

            var status = _service.FindStatus("attente");
            if (user.IdStatus != status.IdStatus)
            {
                return Ok(new ApiResponse<string>(200, "Déjà confirmé"));
            }

            var statusNormal = _service.FindStatus("normal");
            user.IdStatus = statusNormal.IdStatus;

            //_service.UpdateUser(user);

            Token token = new Token();
            token.Key = _tokenService.GenerateToken(user.Username);
            token.DateCreation = DateTime.UtcNow;
            token.DateExpiration = DateTime.UtcNow.AddHours(1);
            token.IdUser = user.IdUser;

            _service.PushData();

            SuccessAuth successAuth = new SuccessAuth(user, token.Key);
            _service.HidePassword(user);
            return Ok(new ApiResponse<SuccessAuth>(200, successAuth, "confirmé"));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO loginDTO)
        {
            var user = _service.FindUserByEmail(loginDTO.Email);
            if (user == null)
            {
                Error err = new(500);
                return Unauthorized(new ApiResponse<string>(401, "Email introuvable", err));
            }

            bool passwordMatches = _service.IsPasswordMatches(loginDTO.Password, user.Password);
            var statusBloque = _service.FindStatus("bloque");
            if (!passwordMatches)
            {
                if (user.NbTentative >= 3 - 1)
                {
                    user.IdStatus = statusBloque.IdStatus;
                    _service.PushData();

                    Error err = new(550);
                    return Unauthorized(new ApiResponse<string>(401, "Mot de passe incorrect, Votre compte est bloqué, Reinitialiser", err));
                }

                user.NbTentative++;
                _service.PushData();

                Error err2 = new(500);
                return Unauthorized(new ApiResponse<string>(401, "Mot de passe incorrect.", err2));
            }

            if (user.IdStatus == statusBloque.IdStatus)
            {
                Error err = new(550);
                return Unauthorized(new ApiResponse<string>(401, "Utilisateur Bloqué , Reinitialiser votre compte", err));
            }

            if (user.NbTentative > 0)
            {
                user.NbTentative = 0;
                _service.PushData();
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
                MailSender.SendEmail(user.Email, "Code Pin", AppUtil.GeneratePinDiv(pin));
            } catch (Exception)
            {
                Error err = new(500);
                return StatusCode(500, new ApiResponse<string>(500, "Email Sending failed", err));
            }

            _service.RemoveAuthentificationsOf(user);
            _service.AddAuthentification(auth);
            _service.PushData();

            return Ok(new ApiResponse<string>(200, auth.AKey, "Verifier votre mail")); 
        }

        [HttpPost("auth")]
        public IActionResult Auth([FromBody] UserAuthDTO authDTO)
        {
            var authData = _service.FindAuthentificationByKey(authDTO.AKey);
            if (authData == null)
            {
                Error err = new(500);
                return StatusCode(401, new ApiResponse<string>(401, "Authentification introuvable", err));
            }

            if (_service.IsExpired(authData.Expiration))
            {
                Error err = new(500);
                return StatusCode(401, new ApiResponse<string>(401, "Authentification expirée", err));
            }

            var user = _service.FindUserById(authData.IdUser);
            if (user == null) 
            {
                Error err = new(500);
                return StatusCode(401, new ApiResponse<string>(401, "Authentification invalide", err));
            }

            var statusBloque = _service.FindStatus("bloque");
            if (user.IdStatus == statusBloque.IdStatus)
            {
                Error err = new(550);
                return StatusCode(401, new ApiResponse<string>(401, "Utilisateur bloqué", err));
            }

            if (authDTO.Pin != authData.Pin) 
            {
                user.NbTentative++;

                if (user.NbTentative == 3)
                {
                    user.IdStatus = statusBloque.IdStatus;
                    _service.PushData();

                    Error err = new(550);
                    return StatusCode(401, new ApiResponse<string>(401, "Utilisateur bloqué", err));
                }

                _service.PushData();

                Error err2 = new(500);
                return StatusCode(401, new ApiResponse<string>(401, "Pin incorrect", err2));
            }

            _service.RemoveAuthentification(authData);

            Token token = new Token();
            token.Key = _tokenService.GenerateToken(user.Username);
            token.DateCreation = DateTime.UtcNow;
            token.DateExpiration = DateTime.UtcNow.AddHours(1);
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
                return Unauthorized(new ApiResponse<string>(401, "Email introuvable", err));
            }

            var statusBloque = _service.FindStatus("bloque");
            if (user.IdStatus == statusBloque.IdStatus)
            {
                string hashKey = AppUtil.GenerateSecretUniqueKey(user);
                ResetEmailRequest req = new ResetEmailRequest(mailDTO.Email , hashKey);
                _service.AddResetEmailRequest(req);
                string url = _service.GetResetUrl(hashKey, "localhost", "5106");
                try
                {
                    MailSender.SendEmail(user.Email, "Reinitialisation", url);
                } catch (Exception)
                {
                    Error err = new(500);
                    return StatusCode(500, new ApiResponse<string>(500, "Email Sending failed", err));
                }
            } else
            {
                Error err = new(500);
                return StatusCode(401, new ApiResponse<string>(401, "Non accepté", err));
            }

            _service.PushData();
            return Ok(new ApiResponse<string>(true, 200, "Email envoyé"));
        }


        [HttpPost("reset")]
        public IActionResult Reset([FromQuery(Name = "key")] string key)
        {
            var resetReq = _service.FindResetEmailRequest(key);
            if (resetReq == null)
            {
                Error err = new(500);
                return Unauthorized(new ApiResponse<string>(401, "Reinitialisation key introuvable", err));
            }

            var statusBloque = _service.FindStatus("bloque");
            var statusNormal = _service.FindStatus("normal");

            var user = _service.FindUserByEmail(resetReq.Email);
            if (user == null)
            {
                Error err = new(500);
                return Unauthorized(new ApiResponse<string>(401, "Internal server error", err));
            }

            if (user.IdStatus != statusBloque.IdStatus)
            {
                Error err = new(500);
                return StatusCode(500, new ApiResponse<string>(500, "Reinitialisation non autorisé", err));
            }

            user.NbTentative = 0;
            user.IdStatus = statusNormal.IdStatus;

            _service.RemoveResetEmailRequest(resetReq);
            _service.PushData();

            return Ok(new ApiResponse<string>(true, 200, "reinitialisation fait"));
        }
    }
}
