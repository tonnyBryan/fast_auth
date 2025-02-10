using fast_auth.model.tiers;
using fast_authenticator.context;
using fast_authenticator.model;
using fast_authenticator.model.tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace fast_auth.service
{
    public class AppService
    {
        private readonly MyDbContext _context;

        public AppService(MyDbContext context)
        {
            _context = context;
        }

        public void PushData()
        {
            _context.SaveChanges();
        }

        public IEnumerable<User> FindAllUser()
        {
            return [.. _context.Users.Include(u => u.Status)];
        }

        public User? FindUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.IdUser == id);
        }

        public bool EmailExist(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public Status? FindStatus(string status)
        {
            return _context.Statuses.FirstOrDefault(u => u.Description.Equals(status));
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public void AddUniqueKey(UniqueKey uniqueKey)
        {
            _context.UniqueKeys.Add(uniqueKey);
        }

        public string GetConfirmUrl(UniqueKey uniqueKey, string host, string port)
        {
            return "http://" + host + ":" + port + "/api/User/confirm?key=" + uniqueKey.Skey;
        }

        public string GetResetUrl(string key, string host, string port)
        {
            return "http://" + host + ":" + port + "/api/User/reset?key=" + key;
        }

        public void RemoveUser(User user) 
        { 
            _context.Users.Remove(user);
        }

        public void RemoveUniqueKey(UniqueKey uniqueKey)
        {
            _context.UniqueKeys.Remove(uniqueKey);
        }

        public UniqueKey? FindUniqueKey(string skey)
        {
            return _context.UniqueKeys.FirstOrDefault(u => u.Skey == skey);
        }

        public void HidePassword(User user)
        {
            user.Password = "";
        }

        public User? FindUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public bool IsPasswordMatches(string subject, string subjectCrypted)
        {
            return BCrypt.Net.BCrypt.Verify(subject, subjectCrypted);
        }

        public bool IsPasswordSame(string one , string two) {
            return one == two;
        }

        public void RemoveAuthentificationsOf(User user)
        {
            var authentificationsToRemove = _context.Authentifications
            .Where(a => a.IdUser == user.IdUser)
            .ToList();

            _context.Authentifications.RemoveRange(authentificationsToRemove);
        }

        public void RemoveAuthentification(Authentification auth)
        {
            _context.Authentifications.Remove(auth);
        }

        public void AddAuthentification(Authentification authentification)
        {
            _context.Authentifications.Add(authentification);
        }

        public Authentification? FindAuthentificationByKey(string key)
        {
            return _context.Authentifications.FirstOrDefault(u => u.AKey == key);
        }

        public bool IsExpired(DateTime datetime)
        {
            DateTime now = DateTime.UtcNow.ToLocalTime();
            Console.WriteLine("datetime = " + datetime);
            Console.WriteLine("datetime (local) = " + datetime.ToLocalTime());
            Console.WriteLine("now = " + now);
            Console.WriteLine("now + 3 = " + now.AddHours(3));
            return datetime < DateTime.UtcNow.AddHours(3);
        }

        public void AddResetEmailRequest(ResetEmailRequest request)
        {
            _context.ResetEmailRequests.Add(request);
        }

        public ResetEmailRequest? FindResetEmailRequest(string key)
        {
            return _context.ResetEmailRequests.FirstOrDefault(u => u.Rkey == key);
        }

        public void FindResetEmailRequestByEmailAndRemove(string email)
        {
            var rst = _context.ResetEmailRequests.FirstOrDefault(u => u.Email == email);
            if (rst != null)
            {
                _context.ResetEmailRequests.Remove(rst);
            }
        }

        public void RemoveResetEmailRequest(ResetEmailRequest resetEmailRequest)
        {
            _context.ResetEmailRequests.Remove(resetEmailRequest);
        }

        public void AddToken(Token token)
        {
            var tokenData = _context.Tokens.FirstOrDefault(t => t.IdUser == token.IdUser);
            if (tokenData != null)
            {
                _context.Tokens.Remove(tokenData);
            }
            _context.Tokens.Add(token);
        }
    }
}
