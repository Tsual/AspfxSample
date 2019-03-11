using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSample.Models
{
    [Table("UserLogin")]
    public class mUserLogin
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public mUser User { get; set; }

        public DateTime Time { get; set; }
        public eUserLoginWay LoginWay { get; set; }
        public eUserLoginAccess LoginAccess { get; set; }
    }

    public enum eUserLoginWay
    {
        Email,
        LoginId,
        PhoneNumber
    }

    public enum eUserLoginAccess
    {
        CookieSession,
        Jwt,
        OAuth2
    }
}
