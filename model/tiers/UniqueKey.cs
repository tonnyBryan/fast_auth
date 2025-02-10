using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fast_auth.model.tiers
{
    [Table("unique_key")]
    public class UniqueKey
    {
        [Key]
        public string Skey { get; set; }
        [Column("id_user")]
        public int IdUser { get; set; }

        public UniqueKey(string skey, int idUser)
        {
            this.Skey = skey;
            this.IdUser = idUser;
        }
    }
}
