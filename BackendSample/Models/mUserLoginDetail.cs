using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSample.Models
{
    [Table("UserLoginDetail")]
    public class mUserLoginDetail
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public mUserLogin UserLogin { get; set; }

        public DateTime Time { get; set; }
        public string Operation { get; set; }

    }
}
