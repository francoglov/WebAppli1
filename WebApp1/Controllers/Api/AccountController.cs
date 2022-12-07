using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp1.Constant;
using WebApp1.Services;
using WebApp1.ViewModels;
using WebApp1.ViewModels.Identity;

namespace WebApp1.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [Authorize]
        [HttpGet("GetUsers")]
        public async Task<ActionResult<UserVM[]>> GetUsers()
        {
            var users = await _accountService.GetUsers();



            return users.Where(u => u.UserName != "admin@admin.com").ToArray();
        }

        //[Authorize(Roles = RoleNames.Administrator)]
        [Authorize]
        [HttpPost("AddOrEditUser")]
        public async Task<ActionResult<AddOrEditResponseVM>> AddOrEditUser([FromBody] UserVM vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _accountService.AddOrEditUser(vm);
                    return result;


                }
                catch (Exception ex)
                {
                    return BadRequest(new AddOrEditResponseVM { Errors = new string[] { ex.Message }, Succeeded = false });
                }
            }
            else
            {
                return BadRequest(new AddOrEditResponseVM { Errors = new string[] { "Invalid Model State" }, Succeeded = false });
            }


        }

        [Authorize]
        [HttpGet("IsAuthorized")]
        public async Task<ActionResult<bool>> IsAuthorized()
        {
            return true;
        }


        //   [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<TokenVM>> Login([FromBody] LoginVM inputVM)
        {
            TokenVM result = null;
            if (ModelState.IsValid)
            {
                result = await _accountService.LoginUser(inputVM);
                return result;
            }

            return BadRequest();
        }

        //Delete User
        [Authorize(Roles = RoleNames.Administrator)]
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<bool>> DeleteUser(string user)
        {
            // bool result = false;
            if (ModelState.IsValid)
            {
                await _accountService.DeleteUser(user);
                return true;
            }

            return false;
        }



        [HttpGet("EnsureInitialIdentitiesCreated")]
        public async Task<string> EnsureInitialIdentitiesCreated()
        {
            await _accountService.EnsureInitialUsersAndRolesCreated();
            return "admin created";
        }
    }
}
