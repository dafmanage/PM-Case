
using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.Services.Auth;
using PM_Case_Managemnt_Infrustructure.Models.Auth;

namespace PM_Case_Managemnt_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        IAuthenticationService _authenticationService;



        public ApplicationUserController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }



        [HttpPost]
        [Route("Register")]
        //POST : /api/ApplicationUser/Register
        public async Task<IActionResult> PostApplicationUser(ApplicationUserModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _authenticationService.PostApplicationUser(model));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPost]
        [Route("Login")]
        //POST : /api/ApplicationUser/Login
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _authenticationService.Login(model));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }


        [HttpGet]
        [Route("getroles")]

        public async Task<IActionResult> GetRolesForUser()
        {

            return Ok(await _authenticationService.GetRolesForUser());
        }

        [HttpGet("users")]

        public async Task<IActionResult> getUsers(Guid subOrgId)
        {


            return Ok(await _authenticationService.getUsers(subOrgId));
        }

        [HttpPost("ChangePassword")]

        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _authenticationService.ChangePassword(model));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpGet("getNotAssignedRole")]

        public async Task<IActionResult> GetNotAssignedRole(string userId)
        {


            return Ok(await _authenticationService.GetNotAssignedRole(userId));
        }

        [HttpGet("getAssignedRoles")]

        public async Task<IActionResult> GetAssignedRoles(string userId)
        {


            return Ok(await _authenticationService.GetAssignedRoles(userId));
        }

        [HttpPost("assignRole")]

        public async Task<IActionResult> AssignRole(UserRoleDto userRole)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _authenticationService.AssignRole(userRole));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [HttpPost("revokeRole")]

        public async Task<IActionResult> ChangePassword(UserRoleDto userRole)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _authenticationService.RevokeRole(userRole));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [HttpPost("ChangePasswordAdmin")]

        public async Task<IActionResult> ChangePasswordAdmin(ChangePasswordModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _authenticationService.ChangePasswordAdmin(model));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                return Ok(await _authenticationService.DeleteUser(userId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error - {ex}");
            }
        }
    }
}