using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp1.ViewModels.Identity
{
    public class TokenVM
    {
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
