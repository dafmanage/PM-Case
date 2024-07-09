﻿using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Services.PM.Commite;

namespace PM_Case_Managemnt_API.Controllers.PM
{
    [Route("api/PM/[controller]")]
    [ApiController]
    public class CommiteController(ICommiteService commiteService) : Controller
    {
        private readonly ICommiteService _commiteService = commiteService;

        [HttpPost]
        public IActionResult Create([FromBody] AddCommiteDto addCommiteDto)
        {
            try
            {
                var response = _commiteService.AddCommite(addCommiteDto);
                return Ok(new { response });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }


        [HttpPut]
        public IActionResult Update([FromBody] UpdateCommiteDto updateCommiteDto)
        {
            try
            {
                var response = _commiteService.UpdateCommite(updateCommiteDto);
                return Ok(new { response });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }

        [HttpGet]
        public async Task<List<CommiteListDto>> GetCommiteLists(Guid subOrgId)
        {
            var response = await _commiteService.GetCommiteLists(subOrgId);
            return response;
        }

        [HttpGet("getNotIncludedEmployees")]
        public async Task<List<SelectListDto>> GetNotIncludedEmployees(Guid commiteId, Guid subOrgId)
        {
            var response = await _commiteService.GetNotIncludedEmployees(commiteId, subOrgId);

            return response;
        }
        [HttpGet("getSelectListCommittee")]

        public async Task<List<SelectListDto>> getCommitteeSelectList(Guid subOrgId)
        {

            var response = await _commiteService.GetSelectListCommittee(subOrgId);
            return response;
        }

        [HttpGet("GetCommiteeEmployees")]

        public async Task<List<SelectListDto>> GetCommiteeEmployees(Guid commiteId)
        {
            var response = await _commiteService.GetCommiteeEmployees(commiteId);

            return response;

        }




        [HttpPost("addEmployesInCommitee")]

        public IActionResult addEmployee([FromBody] CommiteEmployeesdto commite)
        {
            try
            {
                var response = _commiteService.AddEmployeesToCommittee(commite);
                return Ok(new { response });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }
        [HttpPost("removeEmployesInCommitee")]

        public IActionResult removeEmployee([FromBody] CommiteEmployeesdto commite)
        {
            try
            {
                var response = _commiteService.RemoveEmployeesFromCommittee(commite);
                return Ok(new { response });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }
    }
}
