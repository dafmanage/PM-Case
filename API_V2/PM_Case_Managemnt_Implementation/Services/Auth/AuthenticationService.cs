using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Implementation.Hubs.EncoderHub;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PM_Case_Managemnt_Implementation.Services.Auth
{
    public class AuthenticationService : IAuthenticationService
    {

        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _singInManager;
        private readonly ApplicationSettings _appSettings;
        private readonly ApplicationDbContext _dbcontext;
        private IHubContext<EncoderHub, IEncoderHubInterface> _encoderHub;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILoggerManagerService _logManager;

        public AuthenticationService(
            ApplicationDbContext dbcontext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<ApplicationSettings> appSettings,
            IHubContext<EncoderHub, IEncoderHubInterface> encoderHub,
            RoleManager<IdentityRole> roleManager,
            ILoggerManagerService logManager
)
        {
            _userManager = userManager;
            _singInManager = signInManager;
            _appSettings = appSettings.Value;
            _dbcontext = dbcontext;
            _encoderHub = encoderHub;
            _roleManager = roleManager;
            _logManager = logManager;
        }



        public async Task<Object> PostApplicationUser(ApplicationUserModel model)
        {
            // model.Role = "Admin";
            var applicationUser = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.UserName + "@daftech.com",
                FullName = model.FullName,
                EmployeesId = model.EmployeeId,
                SubsidiaryOrganizationId = model.SubsidiaryOrganizationId
            };

            try
            {
                var result = await _userManager.CreateAsync(applicationUser, model.Password);

                foreach (var role in model.Roles)
                {
                    await _userManager.AddToRoleAsync(applicationUser, role);
                }

                var subOrg = _dbcontext.SubsidiaryOrganizations.Find(model.SubsidiaryOrganizationId);
                if (subOrg.isRegulatoryBody)
                {
                    await _userManager.AddToRoleAsync(applicationUser, "Regulator");
                }

                if (model.EmployeeId != Guid.Empty)
                {
                    var emp = _dbcontext.Employees.Find(model.EmployeeId);
                    emp.UserName = model.UserName;
                    emp.Password = model.Password;

                    _dbcontext.Entry(emp).State = EntityState.Modified;
                    _dbcontext.SaveChangesAsync();

                }

                return 1;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public async Task<IActionResult> Login(LoginModel model)
        {

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                throw new Exception("Username or password is incorrect.");
            //{
            //    StatusCode = 400
            //};
            Employee emp = _dbcontext.Employees.Find(user.EmployeesId);
            string empPhoto = "";
            if (emp != null)
                empPhoto = emp.Photo;
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                //Get role assigned to the user
                var role = await _userManager.GetRolesAsync(user);
                var str = String.Join(",", role);
                IdentityOptions _options = new IdentityOptions();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID",user.Id.ToString()),
                        new Claim("FullName",user.FullName),
                        new Claim("EmployeeId",user.EmployeesId.ToString()),
                        new Claim("Photo",empPhoto),
                        new Claim(_options.ClaimsIdentity.RoleClaimType,str),
                        new Claim("SubsidiaryOrganizationId",user.SubsidiaryOrganizationId.ToString()),
                        new Claim("StructureId",emp.OrganizationalStructureId.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);

                _logManager.LogCreate("AuthenticationModule", user.Id, "Login SuccessFul For Create");
                _logManager.LogUpdate("AuthenticationModule", user.Id, "Login SuccessFul For Update");
                // await _encoderHub.Groups.AddToGroupAsync(Context.ConnectionId, user.EmployeesId);
                return new ObjectResult(new { token });
            }
            else
                throw new Exception("Username or password is incorrect.");

        }



        public async Task<List<SelectRolesListDto>> GetRolesForUser()
        {
            string[] excludedRoleName = new string[] { "REGULATOR", "MONITOR" };

            return await (from x in _roleManager.Roles
                          where !excludedRoleName.Contains(x.Name)
                          select new SelectRolesListDto
                          {
                              Id = x.Id,
                              Name = x.Name
                          }

                    ).ToListAsync();
        }


        public async Task<List<EmployeeDto>> getUsers(Guid subOrgId)
        {


            var Users = await _userManager.Users.Where(g => g.SubsidiaryOrganizationId == subOrgId).ToListAsync();


            return (from u in Users
                    join e in _dbcontext.Employees.Include(x => x.OrganizationalStructure) on u.EmployeesId equals e.Id
                    select new EmployeeDto
                    {
                        Id = Guid.Parse(u.Id),
                        UserName = u.UserName,
                        FullName = e.FullName,
                        Photo = e.Photo,
                        Title = e.Title,
                        Gender = e.Gender.ToString(),
                        PhoneNumber = e.PhoneNumber,
                        StructureName = e.OrganizationalStructure.StructureName,
                        Position = e.Position.ToString(),
                        Remark = e.Remark,

                    }).ToList();
        }


        public async Task<ResponseMessage<int>> ChangePassword(ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return new ResponseMessage<int>
                {
                    Message = "User not found.",
                    Success = false
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return new ResponseMessage<int>
                {
                    Success = false,
                    Message = result.Errors.ToString()
                };
            }

            return new ResponseMessage<int>
            {
                Success = true,
                Message = "Password Changed Successfully"
            };
        }



        public async Task<List<SelectRolesListDto>> GetNotAssignedRole(string userId)
        {
            var currentuser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            string[] excludedRoleName = new string[] { "REGULATOR", "MONITOR" };
            if (currentuser != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(currentuser);
                if (currentRoles.Any())
                {
                    var notAssignedRoles = await _roleManager.Roles.
                                  Where(x =>
                                  !currentRoles.Contains(x.Name) && !excludedRoleName.Contains(x.Name)).Select(x => new SelectRolesListDto
                                  {
                                      Id = x.Id,
                                      Name = x.Name
                                  }).ToListAsync();

                    return notAssignedRoles;
                }
                else
                {
                    var notAssignedRoles = await _roleManager.Roles.Where(x => !excludedRoleName.Contains(x.Name))
                                .Select(x => new SelectRolesListDto
                                {
                                    Id = x.Id,
                                    Name = x.Name
                                }).ToListAsync();

                    return notAssignedRoles;

                }


            }

            throw new FileNotFoundException();
        }


        public async Task<List<SelectRolesListDto>> GetAssignedRoles(string userId)
        {
            var currentuser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            string[] excludedRoleName = new string[] { "REGULATOR", "MONITOR" };
            if (currentuser != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(currentuser);
                if (currentRoles.Any())
                {
                    var notAssignedRoles = await _roleManager.Roles.
                                      Where(x =>
                                      currentRoles.Contains(x.Name) && !excludedRoleName.Contains(x.Name)).Select(x => new SelectRolesListDto
                                      {
                                          Id = x.Id,
                                          Name = x.Name
                                      }).ToListAsync();

                    return notAssignedRoles;
                }

                return new List<SelectRolesListDto>();

            }

            throw new FileNotFoundException();
        }


        public async Task<ResponseMessage<int>> AssignRole(UserRoleDto userRole)
        {
            var currentUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userRole.UserId);


            if (currentUser != null)
            {
                var roleExists = await _roleManager.RoleExistsAsync(userRole.RoleName);

                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(currentUser, userRole.RoleName);

                    return new ResponseMessage<int>
                    {
                        Success = true,
                        Message = "Successfully Added Roles."
                    };
                }
                else
                {
                    return new ResponseMessage<int>
                    {
                        Success = false,
                        Message = "Role does not exist."
                    };
                }
            }
            else
            {
                return new ResponseMessage<int>
                {
                    Message = "User not found.",
                    Success = false
                };
            }
        }


        public async Task<ResponseMessage<int>> RevokeRole(UserRoleDto userRole)
        {
            var curentUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id.Equals(userRole.UserId));

            if (curentUser != null)
            {
                await _userManager.RemoveFromRoleAsync(curentUser, userRole.RoleName);
                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "Succesfully Revoked Roles."
                };
            }
            return new ResponseMessage<int>
            {
                Message = "User not found.",
                Success = false
            };

        }
        /////
        //



        public async Task<ResponseMessage<int>> ChangePasswordAdmin(ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return new ResponseMessage<int>
                {
                    Message = "User not found.",
                    Success = false
                };
            }
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            // var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return new ResponseMessage<int>
                {
                    Success = false,
                    Message = result.Errors.ToString()
                };
            }

            return new ResponseMessage<int>
            {
                Success = true,
                Message = "Password Changed Successfully."
            };
        }

        public async Task<ResponseMessage<int>> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ResponseMessage<int>
                {
                    Message = "User not found.",
                    Success = false
                };
            }
            else
            {

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return new ResponseMessage<int>
                    {
                        Message = "User deleted successfully.",
                        Success = true
                    };
                }
                else
                {
                    return new ResponseMessage<int>
                    {
                        Message = result.Errors.ToString(),
                        Success = false
                    };
                }

            }

        }


    }
}
