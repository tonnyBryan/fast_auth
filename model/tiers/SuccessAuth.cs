using fast_authenticator.model;

namespace fast_auth.model.tiers
{
    public class SuccessAuth
    {
        public User User { get; set; }
        public string Token { get; set; }

        public SuccessAuth (User user, string token)
        {
            User = user;
            this.Token = token;
        }
    }
}
