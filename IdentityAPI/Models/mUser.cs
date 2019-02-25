using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI.Models
{
    [Table(name: "USER")]
    public class mUser
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        [Required]
        public string Password { get; set; }

        public string Email { get; set; }

        public string LoginId { get; set; }

        public string PhoneNumber { get; set; }
    }
}
