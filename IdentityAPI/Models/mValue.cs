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


        public static implicit operator KeyValuePair<string, string>(mValue value)
        {
            return new KeyValuePair<string, string>(value.Key, value.Value);
        }

        public static implicit operator mValue(KeyValuePair<string, string> keyValue)
        {
            return new mValue()
            {
                Key = keyValue.Key,
                Value = keyValue.Value
            };
        }

        public override string ToString()
        {
            return "{" + Key + "," + Value + "}";
        }
    }
}
