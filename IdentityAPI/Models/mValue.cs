using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI.Models
{
    [Table(name: "VALUE")]
    public class mValue
    {
        [Required, Key]
        public string Key { get; set; }

        public string Value { get; set; }


        public static implicit operator KeyValuePair<string, string>()
        {

        }
    }
}
