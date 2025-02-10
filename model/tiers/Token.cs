using fast_authenticator.model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fast_auth.model.tiers
{
    [Table("tokens")]
    public class Token
    {
        [Key]
        [Column("id_token")]
        public int IdToken { get; set; }

        [Column("token")]
        public string Key { get; set; }

        [Column("date_creation")]
        public DateTime DateCreation { get; set; }

        [Column("date_expiration")]
        public DateTime DateExpiration { get; set; }

        [Column("id_user")]
        public int IdUser { get; set; }

        [ForeignKey(nameof(IdUser))]
        public User User { get; set; }
    }
}
