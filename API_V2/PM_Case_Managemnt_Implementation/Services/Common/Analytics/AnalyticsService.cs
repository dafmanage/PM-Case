﻿using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.Common.Analytics;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.Services.Common.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {

        private readonly ApplicationDbContext _dBContext;
        private readonly ILoggerManagerService _logger;
        public AnalyticsService(ApplicationDbContext context, ILoggerManagerService logger)
        {
            _dBContext = context;
            _logger = logger;
        }

         public async Task<ResponseMessage<SubOrgsPlannedandusedBudgetDtos>> GetOverallBudget()
        {

            var response = new ResponseMessage<SubOrgsPlannedandusedBudgetDtos>();

            List<OverallBudgetDto> overallPlannedBudgetDtos = [];

            List<OverallBudgetDto> overallUsedBudgetDtos = [];

            float taskUsedBudget = 0;
            float planUsedBudget = 0;
            float programUsedBudget = 0;
            var subOrgs = await _dBContext.SubsidiaryOrganizations.Where(x => x.isMonitor == false).Select(e => new SelectListDto
                                                                                {
                                                                                    Id = e.Id,
                                                                                    Name = e.OrganizationNameEnglish

                                                                                }).ToListAsync();

            foreach(var sub in subOrgs)
            {
                var programs = await _dBContext.Programs.Where(x => x.SubsidiaryOrganizationId == sub.Id).ToListAsync();
                foreach(var program in programs)
                {
                    var plans = await _dBContext.Plans.Where(x => x.ProgramId== program.Id).ToListAsync();

                    foreach (var plan in plans)
                    {
                        var tasks = await _dBContext.Tasks.Where(x => x.PlanId== plan.Id).ToListAsync();
                        
                        foreach( var task in tasks)
                        {
                            var activities = await _dBContext.ActivityParents.Where(x => x.TaskId == task.Id)
                                                        .Join(_dBContext.Activities,
                                                              p => p.Id,
                                                              a => a.ActivityParentId,
                                                            (p, a) => a).ToListAsync();

                            var usedBudget = activities.Select(x => _dBContext.ActivityProgresses.Where(x => x.ActivityId == x.Id && x.IsApprovedByFinance == ApprovalStatus.Approved).Sum(x => x.ActualBudget)).Sum();

                            taskUsedBudget += usedBudget;
                        }

                        planUsedBudget += taskUsedBudget;
                    }
                    programUsedBudget += planUsedBudget;
                }

                overallPlannedBudgetDtos.Add(new OverallBudgetDto
                {
                    SubOrganiztionName = sub.Name,
                    SubOrganizationBudget= programs.Select(x => x.ProgramPlannedBudget).Sum(),
                   
                });

                overallUsedBudgetDtos.Add(new OverallBudgetDto
                {
                    SubOrganiztionName = sub.Name,
                    SubOrganizationBudget = programUsedBudget,

                });
            }

            SubOrgsPlannedandusedBudgetDtos result = new SubOrgsPlannedandusedBudgetDtos{ PlannedBudget = overallPlannedBudgetDtos, Usedbudget = overallPlannedBudgetDtos};

            response.Message = "Operation Successfull.";
            response.Data = result;
            response.Success = true;

            return response;

        }

        //public async Task<List<OverallPerformanceDto>> GetOverallProgress()
        //{
        //    List<OverallPerformanceDto> overallPerformanceDtos = new List<OverallPerformanceDto>();

        //    float taskPerformance = 0;
        //    float planPerformance = 0;
        //    float programPerformance = 0;
        //    var subOrgs = await _dBContext.SubsidiaryOrganizations.Where(x => x.isMonitor == false).Select(e => new SelectListDto
        //    {
        //        Id = e.Id,
        //        Name = e.OrganizationNameEnglish

        //    }).ToListAsync();
        //    var activityProgress = _dBContext.ActivityProgresses;
        //    foreach (var sub in subOrgs)
        //    {

        //        var programs = await _dBContext.Programs.Where(x => x.SubsidiaryOrganizationId == sub.Id).ToListAsync();
        //        foreach (var program in programs)
        //        {
        //            var plans = await _dBContext.Plans.Where(x => x.ProgramId == program.Id).ToListAsync();

        //            foreach (var plan in plans)
        //            {
        //                var tasks = await _dBContext.Tasks.Where(x => x.PlanId == plan.Id).ToListAsync();


        //                foreach (var task in tasks)
        //                {
        //                    var activities = await _dBContext.ActivityParents.Where(x => x.TaskId == task.Id)
        //                                                .Join(_dBContext.Activities,
        //                                                      p => p.Id,
        //                                                      a => a.ActivityParentId,
        //                                                    (p, a) => a).Select(e =>

        //                                                        _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == e.Id).OrderBy(x => x.Order).Select(y => new MonthPerformanceViewDto
        //                                                        {
        //                                                            Id = y.Id,
        //                                                            Order = y.Order,
        //                                                            Planned = y.Target,
        //                                                            Actual = activityProgress.Where(x => x.QuarterId == y.Id).Sum(x => x.ActualWorked),
        //                                                            Percentage = y.Target != 0 ? (activityProgress.Where(x => x.QuarterId == y.Id && x.IsApprovedByDirector == approvalStatus.approved && x.IsApprovedByFinance == approvalStatus.approved && x.IsApprovedByManager == approvalStatus.approved).Sum(x => x.ActualWorked) / y.Target) * 100 : 0

        //                                                        }).ToList()
        //                         ).ToListAsync();


        //                    double totalActual = activities.SelectMany(activityList => activityList) 
        //                                                .Where(mp => mp.Planned != 0) 
        //                                                .Sum(mp => mp.Actual);

        //                    double totalPlanned = activities.SelectMany(activityList => activityList) 
        //                                                     .Where(mp => mp.Planned != 0) 
        //                                                     .Sum(mp => mp.Planned);


        //                    double overallPercentage = totalPlanned != 0 ? (totalActual / totalPlanned) * 100 : 0;

        //                    taskPerformance += usedBudget;
        //                }

        //                planPerformance += taskPerformance;
        //            }
        //            programPerformance += planPerformance;
        //        }

        //        overallPerformanceDtos.Add(new OverallPerformanceDto
        //        {
        //            SubOrganiztionName = sub.Name,
        //            SubOrganizationPerformace = programPerformance

        //        });


        //    }


        // }

    }
}
