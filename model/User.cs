using fast_authenticator.model.tiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fast_authenticator.model
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id_user")]
        public int IdUser { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        [Column("nb_tentative")]
        public int NbTentative { get; set; }

        [Column("id_status")]
        public int IdStatus { get; set; }

        [ForeignKey(nameof(IdStatus))]
        public Status Status { get; set; }

        [Column("date_creation")]
        public DateTime? DateCreation { get; set; }
    }
}
