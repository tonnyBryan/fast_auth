using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace fast_auth.model.tiers
{
    [Table("reset_mail_request")]
    public class ResetEmailRequest
    {
        [Key]
        [Column("id_reset")]
        public int IdReset {  get; set; }
        public string Email { get; set; }
        public string Rkey { get; set; }

        public ResetEmailRequest (string email, string rkey)
        {   
            Email = email;
            Rkey = rkey;
        } 
    }
}
