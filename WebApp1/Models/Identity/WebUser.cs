using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp1.Models.Identity
{
    public class WebUser: IdentityUser
    {
        public string DisplayName { get; set; }
        public DateTime LastLoggedIn { get; set; }


        public bool EmailNotification { get; set; }
        public bool SmsNotification { get; set; }
    }
}
