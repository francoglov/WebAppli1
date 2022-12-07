using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApp1.Constant;
using WebApp1.Data;
using WebApp1.Models.Identity;
using WebApp1.ViewModels;
using WebApp1.ViewModels.Identity;

namespace WebApp1.Services
{
    public class AccountService
    {
        private RoleManager<IdentityRole> _roleManager;
        private IMemoryCache _memoryCache;
        private IMapper _mapper;

        //private AccountGenerationAndMaintenanceService _accountGenerationAndMaintenanceService;
        private SignInManager<WebUser> _signInManager;
        private UserManager<WebUser> _userManager;
        private IConfiguration _config;
        private DataContext _context;

        public string _usersCacheKey;
        private int _userBranchesCacheExpirationMinutes;

        public AccountService(
           DataContext context,
           RoleManager<IdentityRole> roleManager,
           SignInManager<WebUser> signInManager,
         UserManager<WebUser> userManager,
         IConfiguration config, IMemoryCache memoryCache, IMapper mapper)
        {
            _mapper = mapper;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
            _context = context;
            _roleManager = roleManager;
            _memoryCache = memoryCache;


            _usersCacheKey = "users-data-cache";



            //   var test = GetUsersBranchesDictionary().Result;

        }


        public async Task<IdentityResult> ChangeUserPassword(WebUser user, string newPassword)
        {
            // var existingUser = await FindUserByGuid(userGuid);
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (removePasswordResult.Succeeded)
            {
                var updatePasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
                return updatePasswordResult;
                //if (updatePasswordResult.Succeeded)
                //{
                //    return true;
                //}
            }

            return removePasswordResult;
        }

        public async Task<AddOrEditResponseVM> ProcessIdentityResult(IdentityResult input)
        {
            var response = new AddOrEditResponseVM { Succeeded = input.Succeeded };

            if (response.Succeeded == false)
            {
                if (input.Errors != null && input.Errors.Count() > 0)
                {
                    var listErrors = new List<string>();
                    foreach (var error in input.Errors)
                    {
                        if (error != null)
                        {
                            listErrors.Add($"Code: {error.Code}, Description: {error.Description}");
                        }
                    }
                    if (listErrors.Count > 0)
                    {
                        response.Errors = listErrors.ToArray();
                    }
                }
            }
            return response;
        }

        public async Task<AddOrEditResponseVM> AddOrEditUser(UserVM vm)
        {
            IdentityResult identityResult;

            if (vm.Id == null || vm.Id == default)
            {
                return await CreateUserIfDoesntExist(vm.Email, vm.UserName, vm.Password, vm.DisplayName, RoleNames.User, vm.PhoneNumber, vm.EmailNotification, vm.SmsNotification);


            }
            else
            {
                var user = await FindUserByGuid(vm.Id.ToString());


                user.DisplayName = vm.DisplayName;
                user.Email = vm.Email;

                user.PhoneNumber = vm.PhoneNumber;
                user.UserName = vm.UserName;
                user.EmailNotification = vm.EmailNotification;
                user.SmsNotification = vm.SmsNotification;



                identityResult = await _userManager.UpdateAsync(user);

                if (identityResult.Succeeded)
                {
                    if (string.IsNullOrWhiteSpace(vm.Password) == false)
                    {
                        identityResult = await ChangeUserPassword(user, vm.Password);

                    }
                    else
                    {

                    }
                }
            }
            return await ProcessIdentityResult(identityResult);
        }

        public async Task<UserVM[]> GetUsers()
        {
            var users = await _context.Users.ToArrayAsync();

            var mappedUsersVM = _mapper.Map<UserVM[]>(users);

            return mappedUsersVM;
        }


        public async Task<WebUser[]> GetUsersCache()
        {
            WebUser[] cachedData;

            if (!_memoryCache.TryGetValue(_usersCacheKey, out cachedData))
            {
                var users = await _context.Users.AsNoTracking().ToArrayAsync();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(_userBranchesCacheExpirationMinutes));

                cachedData = users;

                _memoryCache.Set(_usersCacheKey, cachedData, cacheEntryOptions);
            }

            return cachedData;
        }








        private async Task<AddOrEditResponseVM> CreateUserIfDoesntExist(string email, string username, string password, string displayName, string roleName, string phoneNumber, bool emailNotification, bool smsNotification)
        {
            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser == null)
            {

                var user = new WebUser
                {
                    Email = email,
                    EmailConfirmed = true,
                    DisplayName = displayName,
                    UserName = username,

                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = true,
                    EmailNotification = emailNotification,
                    SmsNotification = smsNotification
                };

                var createUserResult = await _userManager.CreateAsync(user, password);
                if (createUserResult.Succeeded)
                {
                    var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
                    return await ProcessIdentityResult(addToRoleResult);
                }
                else
                {
                    return await ProcessIdentityResult(createUserResult);
                }
            }
            else
            {


                return new AddOrEditResponseVM { Succeeded = false, Errors = new string[] { "User already exists" } };
            }

        }



        public async Task DeleteInitialUsersAndRoles()
        {
            var users = await _context.Users.ToArrayAsync();

            if (users.Count() > 0)
            {
                foreach (var user in users)
                {
                    // await LogoutUser(user.Id); //incase they are still logged in
                    await _userManager.DeleteAsync(user);
                }
            }

            var roles = await _context.Roles.ToArrayAsync();
            if (roles.Count() > 0)
            {
                foreach (var role in roles)
                {
                    await _roleManager.DeleteAsync(role);
                }
            }



        }

        public async Task EnsureInitialUsersAndRolesCreated()
        {
            //how to iterate through a struct
            var members = typeof(RoleNames).GetFields();
            var activatedStruct = new RoleNames();

            foreach (FieldInfo fi in members)
            {   //we know this struct is all strings
                var roleName = fi.GetValue(activatedStruct).ToString();

                if (await _roleManager.FindByNameAsync(roleName) == null)
                    await _roleManager.CreateAsync(new IdentityRole() { Name = roleName });
                else
                    continue;
            }


            ////string email, string username, string password, string displayName, string roleName,  string phoneNumber, bool emailNotification, bool smsNotification)
            await CreateUserIfDoesntExist("admin@admin.com", "admin@admin.com", "admin123", "Administrator", RoleNames.Administrator, null, false, false);

        }

        public async Task<WebUser> FindUserByGuid(string guid)
        {
            var user = await _context.Users.Where(u => u.Id == guid).FirstOrDefaultAsync();
            return user;
        }

        public async Task<bool> DeleteUser(string user)
        {

            var userdelete = await FindUserByUsername(user);
            //delete the user
            if (userdelete != null)
            {
                var result = await _userManager.DeleteAsync(userdelete);
                if (result.Succeeded)
                {
                    return true;
                }

            }
            return false;
        }

        public async Task<WebUser> FindUserByUsername(string username)
        {
            var user = await _context.Users.Where(u => u.NormalizedUserName == username.Trim().ToUpper()).FirstOrDefaultAsync();
            return user;
        }

        public async Task<TokenVM> LoginUser(LoginVM model)
        {
            var user = await FindUserByUsername(model.Username);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (result.Succeeded)
                {
                    return await ProcessLoginAndCreateToken(user, true);

                }
            }
            return null;
        }

        public async Task<TokenVM> ProcessLoginAndCreateToken(WebUser user, bool updateLastLoggedIn)
        {

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimNames.UserId, user.Id));

            claims.Add(new Claim(ClaimNames.Email, user.Email));

            claims.Add(new Claim(ClaimNames.UserName, user.UserName));

            if (string.IsNullOrWhiteSpace(user.DisplayName) == false)
                claims.Add(new Claim(ClaimNames.DisplayName, user.DisplayName));





            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimNames.RoleType, role));
            }

            var token = await CreateTokenExpiryInHours(claims, double.Parse(_config["Tokens:ExpiryHours"]));
            if (updateLastLoggedIn)
            {
                user.LastLoggedIn = DateTime.Now;
                await _userManager.UpdateAsync(user);
            }
            return token;
        }

        public async Task<TokenVM> CreateTokenExpiryInHours(List<Claim> claims, double expiryInHours)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            _config["Tokens:Issuer"],
              _config["Tokens:Audience"],
              claims,
              expires: DateTime.Now.AddHours(expiryInHours),
              signingCredentials: creds);

            var result = new TokenVM()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiry = token.ValidTo
            };

            return result;
        }

    }
}
