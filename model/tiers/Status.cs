using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace fast_authenticator.model.tiers
{
    [Table("status")]
    public class Status
    {
        [Key]
        [Column("id_status")]
        public int IdStatus { get; set; }
        public string Description { get; set; }


        [JsonIgnore]
        public ICollection<User> Users { get; set; }
    }

}
