using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp1.Constant
{
    public struct ClaimNames
    {
        public const string UserId = "user_guid";
        public const string UserName = "user_name";
        public const string DisplayName = "display_name";
        public const string Email = "email_address";
        public const string RoleType = ClaimTypes.Role;
        public const string TokenExpiry = JwtRegisteredClaimNames.Exp;
    }
}
