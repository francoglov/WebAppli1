using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp1.ViewModels.Identity
{
    public class UserVM
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }

        public string Password { get; set; }
        public bool EmailNotification { get; set; }
        public bool SmsNotification { get; set; }
    }
}
