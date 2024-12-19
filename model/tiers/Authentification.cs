using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using fast_authenticator.model.tiers;
using fast_authenticator.model;

namespace fast_auth.model.tiers
{
    [Table("authentification")]
    public class Authentification
    {
        [Key]
        [Column("id_auth")]
        public int IdAuth { get; set; }


        [Column("id_user")]
        public int IdUser { get; set; }  

        public string Pin { get; set; }  
        public DateTime Expiration { get; set; }

        [ForeignKey(nameof(IdUser))]
        public User User { get; set; }
        public string AKey { get; set; }
    }
}
