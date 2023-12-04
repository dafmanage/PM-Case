﻿using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_API.Models.Auth;
using PM_Case_Managemnt_API.Services.Auth;

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

    }
}