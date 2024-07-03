using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Services.PM.Activity;
using System.Net.Http.Headers;

namespace PM_Case_Managemnt_API.Controllers.PM
{
    [Route("api/PM/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {

        private readonly IActivityService _activityService;
        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }


        [HttpPost]
        public IActionResult Create([FromBody] ActivityDetailDto addActivityDto)
        {
            try
            {
                var response = _activityService.AddActivityDetails(addActivityDto);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }


        [HttpPost("AddSubActivity")]
        public async Task<IActionResult> AddSubActivity([FromBody] SubActivityDetailDto subActivity)
        {
            try
            {
                var response = await _activityService.AddSubActivity(subActivity);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }


        [HttpPost("targetDivision")]
        public IActionResult AddTargetDivisionActivity(ActivityTargetDivisionDto activityTarget)
        {
            try
            {
                var response = _activityService.AddTargetActivities(activityTarget);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }
        [HttpPost("addProgress")]

        public IActionResult AddActivityProgress()
        {

            try
            {
                var files = Request.Form.Files;



                List<string> DocumentPathlist = new List<string>();




                string FinancePath = "";
                if (files.Any())

                {
                    foreach (var file in files)
                    {

                        var folderName = Path.Combine("Assets", "ActivityDocuments");

                        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);


                        if (!Directory.Exists(pathToSave))

                            Directory.CreateDirectory(pathToSave);




                        if (file.Name == "Finance")
                        {
                            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                            var fileNameSave = Guid.NewGuid() + "." + fileName.Split('.')[1];
                            var fullPath = Path.Combine(pathToSave, fileNameSave);
                            FinancePath = Path.Combine(folderName, fileNameSave);

                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                        }
                        if (file.Name == "files")
                        {
                            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                            var fileNameSave = Guid.NewGuid() + "." + fileName.Split('.')[1];
                            var fullPath = Path.Combine(pathToSave, fileNameSave);
                            DocumentPathlist.Add(Path.Combine(folderName, fileNameSave));

                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                        }




                    }




                }
                var progress = new AddProgressActivityDto
                {

                    DcoumentPath = DocumentPathlist.ToArray(),
                    FinacncePath = FinancePath,
                    QuarterId = Guid.Parse(Request.Form["QuarterId"]),
                    ActualBudget = float.Parse(Request.Form["ActualBudget"]),
                    ActualWorked = float.Parse(Request.Form["ActualWorked"]),
                    Remark = Request.Form["Remark"],
                    ActivityId = Guid.Parse(Request.Form["ActivityId"]),
                    ProgressStatus = Request.Form["ProgressStatus"],
                    CreatedBy = Guid.Parse(Request.Form["CreatedBy"]),
                    EmployeeValueId = Guid.Parse(Request.Form["EmployeeValueId"]),
                    lat = Request.Form["lat"],
                    lng = Request.Form["lng"],

                };




                var response = _activityService.AddProgress(progress);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }

        [HttpGet("viewProgress")]
        public async Task<List<ProgressViewDto>> ViewActivityProgress(Guid actId)
        {
            var result = await _activityService.ViewProgress(actId);
            return result.Data;
        }

        [HttpGet("getEmployeesFromBranch")]
        public async Task<List<SelectListDto>> GetEmployeesFromBranch(Guid branchId)
        {
            var result = await _activityService.GetEmployeesInBranch(branchId);
            return result.Data;
            
        }

        [HttpGet("getAssignedActivties")]
        public async Task<List<ActivityViewDto>> GetAssignedActivity(Guid employeeId)
        {
            var result = await _activityService.GetAssignedActivity(employeeId);
            return result.Data;
        }

        [HttpGet("getAssignedActivtiesNumber")]
        public async Task<int> GetAssignedActivityNumber(Guid employeeId)
        {
            var result = await _activityService.GetAssignedActivityNumber(employeeId);
            return result.Data;
        }


        [HttpGet("forApproval")]
        public async Task<List<ActivityViewDto>> forApproval(Guid employeeId)
        {
            var result = await _activityService.GetActivtiesForApproval(employeeId);
            return result.Data;
        }

        [HttpPost("approve")]
        public IActionResult ApproveProgress(ApprovalProgressDto approvalProgressDto)
        {
            try
            {
                var response = _activityService.ApproveProgress(approvalProgressDto);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }

        [HttpGet("getActivityAttachments")]
        public async Task<List<ActivityAttachmentDto>> GetActivityAtachments(Guid taskId)
        {

            var result = await _activityService.getAttachemnts(taskId);
            return result.Data;


        }

        [HttpGet("byActivityId")]
        public async Task<ActivityViewDto> getActivityById(Guid actId)
        {
            var result = await _activityService.getActivityById(actId);
            return result.Data;
        }
        [HttpPost("AssignEmployee")]

        public async Task<ReponseMessage> AssignEmmployees(ActivityEmployees activityEmployees)
        {
            return await _activityService.AssignEmployees(activityEmployees);
        }

        [HttpPut("editActivity")]
        public async Task<IActionResult> UpdateActivityDetail(SubActivityDetailDto activityDetail)
        {

            if (ModelState.IsValid)
            {
                return Ok(await _activityService.UpdateActivityDetails(activityDetail));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("deleteActivity")]
        public async Task<IActionResult> DeleteActivity(Guid activityid, Guid taskId)
        {
            try
            {

                return Ok(await _activityService.DeleteActivity(activityid, taskId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error - {ex}");
            }
        }
    }
}
