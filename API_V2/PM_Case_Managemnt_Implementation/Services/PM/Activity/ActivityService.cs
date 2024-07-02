using Microsoft.EntityFrameworkCore;
using System.Linq;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;

// 
using PMActivity = PM_Case_Managemnt_Infrustructure.Models.PM;
using Task = PM_Case_Managemnt_Infrustructure.Models.PM.Task;
using Tasks = System.Threading.Tasks.Task;
{
    
}

namespace PM_Case_Managemnt_Implementation.Services.PM.Activity
{
    public class ActivityService(ApplicationDbContext context) : IActivityService
    
    {
        private readonly ApplicationDbContext _dBContext = context;
        private static readonly string[] separator = new string[] { "/" };

        public async Task<int> AddActivityDetails(ActivityDetailDto activityDetail)
    {
        // Create or find ActivityParent
        var activityParent = await _dBContext.ActivityParents.FirstOrDefaultAsync(x => x.TaskId == activityDetail.TaskId);
        if (activityParent == null)
        {
            activityParent = new ActivityParent
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedBy = activityDetail.CreatedBy,
                ActivityParentDescription = activityDetail.ActivityDescription,
                HasActivity = activityDetail.HasActivity,
                TaskId = activityDetail.TaskId
            };

            await _dBContext.AddAsync(activityParent);
        }

        // Add activities
        foreach (var item in activityDetail.ActivityDetails)
        {
            PM_Case_Managemnt_Infrustructure.Models.PM.Activity activity = new PM_Case_Managemnt_Infrustructure.Models.PM.Activity
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedBy = activityDetail.CreatedBy,
                ActivityParentId = activityDetail.TaskId,
                ActivityDescription = item.SubActivityDesctiption,
                ActivityType = ActivityType.Office_Work,
                Begining = item.PreviousPerformance,
                FieldWork = 50,
                OfficeWork = 50,
                Goal = item.Goal,
                PlanedBudget = item.PlannedBudget,
                UnitOfMeasurementId = item.UnitOfMeasurement,
                Weight = item.Weight,
                ShouldStart = DateTime.Parse(item.StartDate),
                ShouldEnd = DateTime.Parse(item.EndDate),
                CaseTypeId = activityDetail.CaseTypeId,
                OrganizationalStructureId = item.BranchId
            };

            await _dBContext.Activities.AddAsync(activity);

            // Add employees assigned for activities
            if (item.Employees != null)
            {
                foreach (var employee in item.Employees)
                {
                    if (!string.IsNullOrEmpty(employee))
                    {
                        EmployeesAssignedForActivities EAFA = new EmployeesAssignedForActivities
                        {
                            CreatedAt = DateTime.Now,
                            CreatedBy = activityParent.CreatedBy,
                            RowStatus = RowStatus.Active,
                            Id = Guid.NewGuid(),
                            ActivityId = activity.Id,
                            EmployeeId = Guid.Parse(employee)
                        };
                        await _dBContext.EmployeesAssignedForActivities.AddAsync(EAFA);
                    }
                }
            }
        }

        // Update ActivityParent
        if (activityDetail.ActivityDetails.Count != 0)
        {
            activityParent.AssignedToBranch = true;
        }

        await _dBContext.SaveChangesAsync();

        return 1;
    }
    public async Task<int> AddSubActivity(SubActivityDetailDto activityDetail)
    {

        if (activityDetail.IsClassfiedToBranch)
        {
                ActivityParent activity = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = activityDetail.CreatedBy,
                    ActivityParentDescription = activityDetail.SubActivityDesctiption,
                    Goal = activityDetail.Goal,
                    IsClassfiedToBranch = true,

                    PlanedBudget = (float)activityDetail.PlannedBudget,
                    UnitOfMeasurmentId = activityDetail.UnitOfMeasurement,
                    Weight = activityDetail.Weight
                };


                if (activityDetail.TaskId != null)
            {
                activity.TaskId = activityDetail.TaskId;
            }

            if (!string.IsNullOrEmpty(activityDetail.StartDate))
            {
               activity.ShouldStartPeriod = ConvertToGregorianDate(activityDetail.StartDate);
            }

            if (!string.IsNullOrEmpty(activityDetail.EndDate))
            {
                activity.ShouldEnd = ConvertToGregorianDate(activityDetail.EndDate);
            }
            await _dBContext.ActivityParents.AddAsync(activity);
            await _dBContext.SaveChangesAsync();


            if (activityDetail.TaskId != Guid.Empty)
            {
                var Task = await _dBContext.Tasks.FirstOrDefaultAsync(x => x.Id.Equals(activityDetail.TaskId));
                if (Task != null)
                {
                    var plan = await _dBContext.Plans.FirstOrDefaultAsync(x => x.Id.Equals(Task.PlanId));

                    Task.ShouldStartPeriod = activity.ShouldStartPeriod;
                    Task.ShouldEnd = activity.ShouldEnd;
                    Task.Weight = activity.Weight;
                    if (plan != null)
                    {
                        var tasks = await _dBContext.Tasks.Where(x => x.PlanId == plan.Id).ToListAsync();
                        plan.PeriodStartAt = tasks.Min(x => x.ShouldStartPeriod);
                        plan.PeriodEndAt = tasks.Max(x => x.ShouldEnd);
                    }
                }
            }
            _dBContext.SaveChanges();
        }
        else
        {
            PMActivity.Activity activity = new PMActivity.Activity();
            activity.Id = Guid.NewGuid();
            activity.CreatedAt = DateTime.Now;
            activity.CreatedBy = activityDetail.CreatedBy;
            activity.ActivityDescription = activityDetail.SubActivityDesctiption;
            activity.ActivityType = (ActivityType)activityDetail.ActivityType;
            activity.Begining = activityDetail.PreviousPerformance;
            if (activityDetail.CommiteeId != null)
            {
                activity.CommiteeId = activityDetail.CommiteeId;
            }
            activity.FieldWork = activityDetail.FieldWork;
            activity.Goal = activityDetail.Goal;
            activity.OfficeWork = activityDetail.OfficeWork;
            activity.PlanedBudget = activityDetail.PlannedBudget;
            activity.UnitOfMeasurementId = activityDetail.UnitOfMeasurement;
            activity.Weight = activityDetail.Weight;
            if (activityDetail.PlanId != null)
            {
                activity.PlanId = activityDetail.PlanId;
            }
            else if (activityDetail.TaskId != null)
            {
                activity.TaskId = activityDetail.TaskId;
            }
            if (activityDetail.HasKpiGoal)
            {
                activity.HasKpiGoal = activityDetail.HasKpiGoal;
                activity.KpiGoalId = activityDetail.KpiGoalId;
            }

            if (!string.IsNullOrEmpty(activityDetail.StartDate))
            {
                activity.ShouldStart = ConvertToGregorianDate(activityDetail.StartDate);
        
            }

            if (!string.IsNullOrEmpty(activityDetail.EndDate))
            {
                string[] endDate = activityDetail.EndDate.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                DateTime ShouldEnd = Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(Int32.Parse(endDate[1]), Int32.Parse(endDate[0]), Int32.Parse(endDate[2])));
                activity.ShouldEnd = ShouldEnd;
            }
            await _dBContext.Activities.AddAsync(activity);
            await _dBContext.SaveChangesAsync();
            if (activityDetail.Employees != null)
            {
                foreach (var employee in activityDetail.Employees)
                {
                    if (!string.IsNullOrEmpty(employee))
                    {
                        EmployeesAssignedForActivities EAFA = new EmployeesAssignedForActivities
                        {
                            CreatedAt = DateTime.Now,
                            CreatedBy = activityDetail.CreatedBy,
                            RowStatus = RowStatus.Active,
                            Id = Guid.NewGuid(),

                            ActivityId = activity.Id,
                            EmployeeId = Guid.Parse(employee),
                        };
                        await _dBContext.EmployeesAssignedForActivities.AddAsync(EAFA);
                        await _dBContext.SaveChangesAsync();
                    }
                }
            }

            if (activityDetail.PlanId != Guid.Empty && activityDetail.PlanId != null)
            {
                var plan = await _dBContext.Plans.FirstOrDefaultAsync(x => x.Id.Equals(activityDetail.PlanId));
                if (plan != null)
                {
                    plan.PeriodStartAt = activity.ShouldStart;
                    plan.PeriodEndAt = activity.ShouldEnd;
                }
            }
            else if (activityDetail.TaskId != Guid.Empty)
            {
                var Task = await _dBContext.Tasks.FirstOrDefaultAsync(x => x.Id.Equals(activityDetail.TaskId));
                if (Task != null)
                {
                    var plan = await _dBContext.Plans.FirstOrDefaultAsync(x => x.Id.Equals(Task.PlanId));

                    Task.ShouldStartPeriod = activity.ShouldStart;
                    Task.ShouldEnd = activity.ShouldEnd;
                    Task.Weight = activity.Weight;
                    if (plan != null)
                    {
                        var tasks = await _dBContext.Tasks.Where(x => x.PlanId == plan.Id).ToListAsync();
                        plan.PeriodStartAt = tasks.Min(x => x.ShouldStartPeriod);
                        plan.PeriodEndAt = tasks.Max(x => x.ShouldEnd);
                    }
                }
            }
                _dBContext.SaveChanges();

            }

        return 1;

        }
    
    private static DateTime ConvertToGregorianDate(string date)
    {
        if (string.IsNullOrEmpty(date))
        {
            throw new ArgumentException("Date string cannot be null or empty", nameof(date));
        }

        string[] dateParts = date.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        if (dateParts.Length != 3)
        {
            throw new ArgumentException("Date string is not in the expected format", nameof(date));
        }

        int day = int.Parse(dateParts[0]);
        int month = int.Parse(dateParts[1]);
        int year = int.Parse(dateParts[2]);

        return Convert.ToDateTime(EthiopicDateTime.GetGregorianDate(year, month, day));
    }

    public async Task<int> AddBranchTargetAsActivity(List<BranchTargetDto> branchTargetDtos)
    {
        try
        {
            var actParent = await _dBContext.ActivityParents
                .Where(x => x.Id == branchTargetDtos[0].ActParentId)
                .FirstOrDefaultAsync();

            if (actParent != null)
            {
                foreach (var branchTargetDto in branchTargetDtos)
                {
                    PM_Case_Managemnt_Infrustructure.Models.PM.Activity activity = new PM_Case_Managemnt_Infrustructure.Models.PM.Activity
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = branchTargetDto.CreatedBy,
                        ActivityParentId = actParent.Id,
                        ActivityDescription = branchTargetDto.ActivityName,
                        ActivityType = ActivityType.Office_Work,
                        Begining = actParent.BaseLine,
                        Goal = branchTargetDto.Target,
                        PlanedBudget = (float)branchTargetDto.Budget,
                        UnitOfMeasurementId = actParent.UnitOfMeasurmentId ?? Guid.Empty,
                        Weight = branchTargetDto.Weight,
                        ShouldStart = actParent.ShouldStartPeriod ?? DateTime.MinValue,
                        ShouldEnd = actParent.ShouldEnd ?? DateTime.MinValue
                    };

                    await _dBContext.Activities.AddAsync(activity);
                }

                await _dBContext.SaveChangesAsync();

            return 1;

            }
            else
            {
                return 0;
            }
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    public async Task<int> AddTargetActivities(ActivityTargetDivisionDto targetDivisions)
    {
        try
        {
            foreach (var target in targetDivisions.TargetDivisionDtos)
            {
                var targetDivision = new ActivityTargetDivision
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = targetDivisions.CreatedBy,
                    CreatedAt = DateTime.Now,
                    ActivityId = targetDivisions.ActiviyId,
                    Order = target.Order + 1,
                    Target = target.Target,
                    TargetBudget = target.TargetBudget
                };

                _dBContext.ActivityTargetDivisions.Add(targetDivision);
            }

            await _dBContext.SaveChangesAsync();

            return 1;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }


    public async Task<int> AddProgress(AddProgressActivityDto activityProgress)
    {
        try
        {
            var activityProgressEntity = new ActivityProgress
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                FinanceDocumentPath = activityProgress.FinacncePath,
                QuarterId = activityProgress.QuarterId,
                ActualBudget = activityProgress.ActualBudget,
                ActualWorked = activityProgress.ActualWorked,
                progressStatus = int.Parse(activityProgress.ProgressStatus) == 0 ? ProgressStatus.SimpleProgress : ProgressStatus.Finalize,
                Remark = activityProgress.Remark,
                ActivityId = activityProgress.ActivityId,
                CreatedBy = activityProgress.CreatedBy,
                EmployeeValueId = activityProgress.EmployeeValueId,
                Lat = activityProgress.lat ?? "",
                Lng = activityProgress.lng ?? ""
            };

            await _dBContext.ActivityProgresses.AddAsync(activityProgressEntity);
            await _dBContext.SaveChangesAsync();

            foreach (var file in activityProgress.DcoumentPath)
            {
                var attachment = new ProgressAttachment
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = activityProgress.CreatedBy,
                    CreatedAt = DateTime.Now,
                    RowStatus = RowStatus.Active,
                    FilePath = file,
                    ActivityProgressId = activityProgressEntity.Id
                };

                await _dBContext.ProgressAttachments.AddAsync(attachment);
            }

            await _dBContext.SaveChangesAsync();

            var activity = await _dBContext.Activities.FindAsync(activityProgressEntity.ActivityId);
            activity.Status = activityProgressEntity.progressStatus == ProgressStatus.SimpleProgress ? Status.OnProgress : Status.Finalized;

            if (activity.ActualStart == null)
            {
                activity.ActualStart = DateTime.Now;
            }

            if (activityProgressEntity.progressStatus == ProgressStatus.Finalize)
            {
                activity.ActualEnd = DateTime.Now;
            }

            activity.ActualWorked += activityProgressEntity.ActualWorked;
            activity.ActualBudget += activityProgressEntity.ActualBudget;

            await _dBContext.SaveChangesAsync();

            return 1;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    public async Task<List<ProgressViewDto>> ViewProgress(Guid actId)
    {
        var progressView = await (from p in _dBContext.ActivityProgresses.Where(x => x.ActivityId == actId)
                                select new ProgressViewDto
                                {
                                    Id = p.Id,
                                    ActalWorked = p.ActualWorked,
                                    UsedBudget = p.ActualBudget,
                                    Remark = p.Remark,
                                    IsApprovedByManager = p.IsApprovedByManager.ToString(),
                                    IsApprovedByFinance = p.IsApprovedByFinance.ToString(),
                                    IsApprovedByDirector = p.IsApprovedByDirector.ToString(),
                                    ManagerApprovalRemark = p.CoordinatorApprovalRemark,
                                    FinanceApprovalRemark = p.FinanceApprovalRemark,
                                    DirectorApprovalRemark = p.DirectorApprovalRemark,
                                    FinanceDocument = p.FinanceDocumentPath,
                                    CaseId = p.CaseId,
                                    Documents = _dBContext.ProgressAttachments.Where(x => x.ActivityProgressId == p.Id).Select(y => y.FilePath).ToArray(),
                                    CreatedAt = p.CreatedAt
                                }).ToListAsync();

        return progressView;
    }

     public async Task<List<ActivityViewDto>> GetAssignedActivity(Guid employeeId)
    {
        var employeeAssignedActivityIds = await GetEmployeeAssignedActivityIds(employeeId);
        var assignedActivities = await GetAssignedActivities(employeeId, employeeAssignedActivityIds);
        return assignedActivities;
    }

    private async Task<List<Guid>> GetEmployeeAssignedActivityIds(Guid employeeId)
    {
        return await _dBContext.EmployeesAssignedForActivities
            .Where(x => x.EmployeeId == employeeId)
            .Select(x => x.ActivityId)
            .ToListAsync();
    }

    private async Task<List<ActivityViewDto>> GetAssignedActivities(Guid employeeId, List<Guid> employeeAssignedActivityIds)
    {
        return await _dBContext.Activities
            .Where(x => x.ActualEnd == null &&
                (employeeAssignedActivityIds.Contains(x.Id) ||
                (x.CommiteeId != null && x.Commitee.Employees.Any(e => e.EmployeeId == employeeId))))
            .Select(e => new ActivityViewDto()
            {
                Id = e.Id,
                Name = e.ActivityDescription,
                PlannedBudget = e.PlanedBudget,
                ActivityType = e.ActivityType.ToString(),
                Weight = e.Weight,
                Begining = e.Begining,
                Target = e.Goal,
                UnitOfMeasurment = e.UnitOfMeasurement.Name,
                OverAllPerformance = 0,
                HasKpiGoal = e.HasKpiGoal,
                KpiGoalId = e.KpiGoalId,
                StartDate = e.ShouldStart.ToString(),
                EndDate = e.ShouldEnd.ToString(),
                Members = GetActivityMembers(e.Id),
                MonthPerformance = GetMonthPerformance(e.Id),
                ProjectType = GetProjectType(e)
            })
            .ToListAsync();
    }

    private List<SelectListDto> GetActivityMembers(Guid activityId)
    {
        return [.. _dBContext.EmployeesAssignedForActivities
            .Include(x => x.Employee)
            .Where(x => x.ActivityId == activityId)
            .Select(y => new SelectListDto
            {
                Id = y.Id,
                Name = y.Employee.FullName,
                Photo = y.Employee.Photo,
                EmployeeId = y.EmployeeId.ToString(),
            })];
    }

    private List<MonthPerformanceViewDto> GetMonthPerformance(Guid activityId)
    {
        return [.. _dBContext.ActivityTargetDivisions
            .Where(x => x.ActivityId == activityId)
            .OrderBy(x => x.Order)
            .Select(y => new MonthPerformanceViewDto
            {
                Id = y.Id,
                Order = y.Order,
                Planned = y.Target,
                Actual = _dBContext.ActivityProgresses
                    .Where(x => x.QuarterId == y.Id)
                    .Sum(mp => mp.ActualWorked),
                Percentage = y.Target != 0
                    ? (_dBContext.ActivityProgresses
                        .Where(x => x.QuarterId == y.Id &&
                                    x.IsApprovedByDirector == ApprovalStatus.Approved &&
                                    x.IsApprovedByFinance == ApprovalStatus.Approved &&
                                    x.IsApprovedByManager == ApprovalStatus.Approved)
                        .Sum(x => x.ActualWorked) / y.Target) * 100
                    : 0
            })];
    }

    private ProjectType GetProjectType(PMActivity.Activity activity)
    {
        if (activity.ActivityParentId != null)
        {
            return activity.ActivityParent.Task.Plan.ProjectType;
        }

        if (activity.TaskId != null)
        {
            return activity.Task.Plan.ProjectType;
        }

        return activity.Plan.ProjectType;
    }


    public async Task<int> GetAssignedActivityNumber(Guid employeeId)
        {
            var employeeAssignedCount = await _dBContext.EmployeesAssignedForActivities.CountAsync(x => x.EmployeeId == employeeId);

            return employeeAssignedCount;
        }

    public async Task<List<ActivityViewDto>> GetActivitiesForApproval(Guid employeeId)
        {
            try
            {
                // Query to get the list of activity progress IDs
                var activityProgressIdsQuery = (
                    from p in _dBContext.Plans
                        .Where(x => x.FinanceId == employeeId || x.ProjectManagerId == employeeId)
                    join a in _dBContext.Activities on p.Id equals a.PlanId
                    join ap in _dBContext.ActivityProgresses on a.Id equals ap.ActivityId
                    select ap.Id
                ).Union(
                    from p in _dBContext.Plans
                        .Where(x => x.FinanceId == employeeId || x.ProjectManagerId == employeeId)
                    join t in _dBContext.Tasks on p.Id equals t.PlanId
                    join a in _dBContext.Activities on t.Id equals a.TaskId
                    join ap in _dBContext.ActivityProgresses on a.Id equals ap.ActivityId
                    select ap.Id
                ).Union(
                    from p in _dBContext.Plans
                        .Where(x => x.FinanceId == employeeId || x.ProjectManagerId == employeeId)
                    join t in _dBContext.Tasks on p.Id equals t.PlanId
                    join ac in _dBContext.ActivityParents on t.Id equals ac.TaskId
                    join a in _dBContext.Activities on ac.Id equals a.ActivityParentId
                    join ap in _dBContext.ActivityProgresses on a.Id equals ap.ActivityId
                    select ap.Id
                );

                var activityProgressIds = await activityProgressIdsQuery.ToListAsync();

                // Query to get the list of ActivityViewDto
                var activityViewDtos = await (
                    from e in _dBContext.ActivityProgresses
                        .Include(x => x.Activity.ActivityParent.Task.Plan.Structure)
                        .Where(a => activityProgressIds.Contains(a.Id)
                                    && (a.IsApprovedByManager == ApprovalStatus.Pending 
                                        || a.IsApprovedByDirector == ApprovalStatus.Pending 
                                        || a.IsApprovedByFinance == ApprovalStatus.Pending))

                select new ActivityViewDto
                    {
                        Id = e.ActivityId,
                        Name = e.Activity.ActivityDescription,
                        PlannedBudget = e.Activity.PlanedBudget,
                        ActivityType = e.Activity.ActivityType.ToString(),
                        Weight = e.Activity.Weight,
                        Begining = e.Activity.Begining,
                        Target = e.Activity.Goal,
                        UnitOfMeasurment = e.Activity.UnitOfMeasurement.Name,
                        OverAllPerformance = 0,
                        StartDate = e.Activity.ShouldStart.ToString(),
                        EndDate = e.Activity.ShouldEnd.ToString(),
                        Members = _dBContext.EmployeesAssignedForActivities
                            .Include(x => x.Employee)
                            .Where(x => x.ActivityId == e.ActivityId)
                            .Select(y => new SelectListDto
                            {
                                Id = y.Id,
                                Name = y.Employee.FullName,
                                Photo = y.Employee.Photo,
                                EmployeeId = y.EmployeeId.ToString(),
                            }).ToList(),
                        MonthPerformance = _dBContext.ActivityTargetDivisions
                            .Where(x => x.ActivityId == e.ActivityId)
                            .OrderBy(x => x.Order)
                            .Select(y => new MonthPerformanceViewDto
                            {
                                Id = y.Id,
                                Order = y.Order,
                                Planned = y.Target,
                                Actual = _dBContext.ActivityProgresses
                                    .Where(x => x.QuarterId == y.Id)
                                    .Sum(mp => mp.ActualWorked),
                                Percentage = y.Target != 0 ? (_dBContext.ActivityProgresses
                                    .Where(x => x.QuarterId == y.Id 
                                                && x.IsApprovedByDirector == ApprovalStatus.Approved 
                                                && x.IsApprovedByFinance == ApprovalStatus.Approved 
                                                && x.IsApprovedByManager == ApprovalStatus.Approved)
                                    .Sum(x => x.ActualWorked) / y.Target) * 100 : 0
                            }).ToList(),

                        ProgresscreatedAt = e.CreatedAt.ToString(),
                        IsFinance = e.Activity.Plan.FinanceId == employeeId
                                    || e.Activity.Task.Plan.FinanceId == employeeId
                                    || e.Activity.ActivityParent.Task.Plan.FinanceId == employeeId,
                        IsProjectManager = e.Activity.Plan.ProjectManagerId == employeeId
                                        || e.Activity.Task.Plan.ProjectManagerId == employeeId
                                        || e.Activity.ActivityParent.Task.Plan.ProjectManagerId == employeeId,
                        IsDirector = _dBContext.Employees
                            .Include(x => x.OrganizationalStructure)
                            .Any(x => x.Id == employeeId && x.Position == Position.Director
                                    && (x.OrganizationalStructureId == e.Activity.Plan.StructureId
                                        || x.OrganizationalStructureId == e.Activity.Task.Plan.StructureId
                                        || x.OrganizationalStructureId == e.Activity.ActivityParent.Task.Plan.StructureId))
                    }
                ).ToListAsync();

                return activityViewDtos.DistinctBy(x => x.Id).ToList();
            }
            catch (Exception ex)
            {
                return new List<ActivityViewDto>();
            }
        }
        public async Task<int> ApproveProgress(ApprovalProgressDto approvalProgressDto)
        {
            var progress = _dBContext.ActivityProgresses.Find(approvalProgressDto.progressId);
            if (progress == null) return 0;

            UpdateApprovalStatus(progress, approvalProgressDto);
            await _dBContext.SaveChangesAsync();
    
            return 1;
        }
        private void UpdateApprovalStatus(ActivityProgress progress, ApprovalProgressDto approvalProgressDto)
        {
          var approvalStatus = approvalProgressDto.actiontype == "Accept" ? ApprovalStatus.Approved : ApprovalStatus.Rejected;

            switch (approvalProgressDto.userType)
            {
                case "Director":
                    progress.DirectorApprovalRemark = approvalProgressDto.Remark;
                    progress.IsApprovedByDirector = approvalStatus;
                    break;
                case "Project Manager":
                    progress.CoordinatorApprovalRemark = approvalProgressDto.Remark;
                    progress.IsApprovedByManager = approvalStatus;
                    break;
                case "Finance":
                    progress.FinanceApprovalRemark = approvalProgressDto.Remark;
                    progress.IsApprovedByFinance = approvalStatus;
                    break;
            }
        }

        public async Task<List<ActivityAttachmentDto>> GetAttachments(Guid taskId)
        {
            var attachmentQuery = from x in _dBContext.ProgressAttachments
                    .Include(x => x.ActivityProgress.Activity.ActivityParent)
                where x.ActivityProgress.Activity.TaskId == taskId ||
                      x.ActivityProgress.Activity.PlanId == taskId ||
                      x.ActivityProgress.Activity.ActivityParent.TaskId == taskId
                select new ActivityAttachmentDto
                {
                    ActivityDesctiption = x.ActivityProgress.Activity.ActivityDescription,
                    FilePath = x.FilePath,
                    FileType = "Attachments"
                };

            var financeQuery = from x in _dBContext.ActivityProgresses
                    .Include(x => x.Activity.ActivityParent)
                where x.Activity.TaskId == taskId ||
                      x.Activity.ActivityParent.TaskId == taskId
                select new ActivityAttachmentDto
                {
                    ActivityDesctiption = x.Activity.ActivityDescription,
                    FilePath = x.FinanceDocumentPath,
                    FileType = "Finance"
                };

            var attachmentResults = await attachmentQuery.ToListAsync();
            var financeResults = await financeQuery.ToListAsync();

            attachmentResults.AddRange(financeResults);

            return attachmentResults;
        }
        
         public async Task<ActivityViewDto> GetActivityById(Guid actId)
        {
            var activity = await _dBContext.Activities
                .Include(x => x.UnitOfMeasurement)
                .FirstOrDefaultAsync(e => e.Id == actId);

            if (activity == null) return null;

            var members = await GetActivityMembers(activity);
            var monthPerformance = await GetMonthPerformance(activity);
            var overAllProgress = CalculateOverallProgress(activity);

            var activityViewDto = new ActivityViewDto
            {
                Id = activity.Id,
                Name = activity.ActivityDescription,
                PlannedBudget = activity.PlanedBudget,
                ActivityType = activity.ActivityType.ToString(),
                Weight = activity.Weight,
                Begining = activity.Begining,
                Target = activity.Goal,
                UnitOfMeasurment = activity.UnitOfMeasurement.Name,
                OverAllPerformance = 0,
                StartDate = activity.ShouldStart.ToString(),
                EndDate = activity.ShouldEnd.ToString(),
                Members = members,
                MonthPerformance = monthPerformance,
                OverAllProgress = overAllProgress
            };

            return activityViewDto;
        }
        private async Task<List<SelectListDto>> GetActivityMembers(PMActivity.Activity activity)
        {
            if (activity.CommiteeId == null)
            {
                return await _dBContext.EmployeesAssignedForActivities
                    .Include(x => x.Employee)
                    .Where(x => x.ActivityId == activity.Id)
                    .Select(y => new SelectListDto
                    {
                        Id = y.Id,
                        Name = y.Employee.FullName,
                        Photo = y.Employee.Photo,
                        EmployeeId = y.EmployeeId.ToString(),
                    }).ToListAsync();
            }

            return await _dBContext.CommiteEmployees
                .Include(x => x.Employee)
                .Where(x => x.CommiteeId == activity.CommiteeId)
                .Select(y => new SelectListDto
                {
                    Id = y.Id,
                    Name = y.Employee.FullName,
                    Photo = y.Employee.Photo,
                    EmployeeId = y.EmployeeId.ToString(),
                }).ToListAsync();
        }
        private async Task<List<MonthPerformanceViewDto>> GetMonthPerformance(PMActivity.Activity activity)
        {
            var activityProgress = _dBContext.ActivityProgresses;

            return await _dBContext.ActivityTargetDivisions
                .Where(x => x.ActivityId == activity.Id)
                .OrderBy(x => x.Order)
                .Select(y => new MonthPerformanceViewDto
                {
                    Id = y.Id,
                    Order = y.Order,
                    Planned = y.Target,
                    Actual = activityProgress.Where(x => x.QuarterId == y.Id).Sum(x => x.ActualWorked),
                    Percentage = y.Target != 0 
                        ? (activityProgress.Where(x => x.QuarterId == y.Id 
                                                        && x.IsApprovedByDirector == ApprovalStatus.Approved 
                                                        && x.IsApprovedByFinance == ApprovalStatus.Approved 
                                                        && x.IsApprovedByManager == ApprovalStatus.Approved)
                                                        .Sum(x => x.ActualWorked) / y.Target) * 100 
                        : 0
                }).ToListAsync();
        }
        private float CalculateOverallProgress(PMActivity.Activity activity)
        {
            var activityProgress = _dBContext.ActivityProgresses;
            return activity.Goal != 0 
                ? activityProgress.Where(x => x.ActivityId == activity.Id 
                                                && x.IsApprovedByDirector == ApprovalStatus.Approved 
                                                && x.IsApprovedByFinance == ApprovalStatus.Approved 
                                                && x.IsApprovedByManager == ApprovalStatus.Approved)
                                                .Sum(x => x.ActualWorked) * 100 / activity.Goal 
                : 0;
        }
        public async Task<List<SelectListDto>> GetEmployeesInBranch(Guid branchId)
        {
            var employees = await _dBContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.OrganizationalStructure.OrganizationBranchId == branchId)

                .Select(x => new SelectListDto
                {
                    Id = x.Id,
                    Name = $"{x.FullName} ({x.OrganizationalStructure.StructureName})"
                })

                .ToListAsync();
            
            return employees;
        }



        public async Task<ReponseMessage> AssignEmployees(ActivityEmployees activityEmployee)
        {
            try
            {
                if (activityEmployee != null)
                {

                    var activityEmployees = await _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == activityEmployee.ActivityId).ToListAsync();

                    if (activityEmployees.Count != 0)
                    {
                        _dBContext.RemoveRange(activityEmployees);
                    }

                    foreach (var employee in activityEmployee.Employees)
                    {
                        if (!string.IsNullOrEmpty(employee))
                        {
                            EmployeesAssignedForActivities EAFA = new EmployeesAssignedForActivities
                            {
                                CreatedAt = DateTime.Now,
                                CreatedBy = activityEmployee.CreatedBy,
                                RowStatus = RowStatus.Active,
                                Id = Guid.NewGuid(),
                                ActivityId = activityEmployee.ActivityId,
                                EmployeeId = Guid.Parse(employee),
                            };
                            await _dBContext.EmployeesAssignedForActivities.AddAsync(EAFA);
                            await _dBContext.SaveChangesAsync();
                        }
                    }

                    return new ReponseMessage
                    {
                        Success = true,
                        Message = "Employee Assigned Successfully"
                    };

                }
                else
                {
                    return new ReponseMessage
                    {
                        Success = false,
                        Message = "No Employee Was Successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ReponseMessage
                {
                    Success = false,
                    Message = ex.Message
                };
            }


        }

        public async Task<ResponseMessage> UpdateActivityDetails(SubActivityDetailDto activityDetail)
        {
            if (activityDetail.IsClassfiedToBranch)
            {
                UpdateActivityParentDetails(activityDetail);
            }
            else
            {
                await UpdateActivityDetail(activityDetail);
            }

            return new ResponseMessage
            {
                Success = true,
                Message = "Activity Updated Successfully"
            };
        }

        private async Tasks UpdateActivityParentDetails(SubActivityDetailDto activityDetail)
        {
            var activity = await _dBContext.ActivityParents.FindAsync(activityDetail.Id);

            activity.ActivityParentDescription = activityDetail.SubActivityDesctiption;
            activity.Goal = activityDetail.Goal;
            activity.IsClassfiedToBranch = true;
            activity.PlanedBudget = activityDetail.PlannedBudget;
            activity.UnitOfMeasurmentId = activityDetail.UnitOfMeasurement;
            activity.Weight = activityDetail.Weight;
            activity.TaskId = activityDetail.TaskId ?? activity.TaskId;

            if (!string.IsNullOrEmpty(activityDetail.StartDate))
            {
                activity.ShouldStartPeriod = ConvertToGregorianDate(activityDetail.StartDate);
            }

            if (!string.IsNullOrEmpty(activityDetail.EndDate))
            {
                activity.ShouldEnd = ConvertToGregorianDate(activityDetail.EndDate);
            }

            await _dBContext.SaveChangesAsync();
            await UpdateTaskAndPlanDetails(activityDetail.TaskId, activity.ShouldStartPeriod, activity.ShouldEnd, activity.Weight);
        }

        private async Tasks UpdateActivityDetail(SubActivityDetailDto activityDetail)
        {
            var activity = await _dBContext.Activities.FindAsync(activityDetail.Id);

            activity.ActivityDescription = activityDetail.SubActivityDesctiption;
            activity.ActivityType = (ActivityType)activityDetail.ActivityType;
            activity.Begining = activityDetail.PreviousPerformance;
            activity.CommiteeId = activityDetail.CommiteeId ?? activity.CommiteeId;
            activity.FieldWork = activityDetail.FieldWork;
            activity.Goal = activityDetail.Goal;
            activity.OfficeWork = activityDetail.OfficeWork;
            activity.PlanedBudget = activityDetail.PlannedBudget;
            activity.UnitOfMeasurementId = activityDetail.UnitOfMeasurement;
            activity.Weight = activityDetail.Weight;
            activity.PlanId = activityDetail.PlanId ?? activity.PlanId;
            activity.TaskId = activityDetail.TaskId ?? activity.TaskId;
            activity.HasKpiGoal = activityDetail.HasKpiGoal;
            activity.KpiGoalId = activityDetail.KpiGoalId ?? activity.KpiGoalId;

            if (!string.IsNullOrEmpty(activityDetail.StartDate))
            {
                activity.ShouldStart = ConvertToGregorianDate(activityDetail.StartDate);
            }

            if (!string.IsNullOrEmpty(activityDetail.EndDate))
            {
                activity.ShouldEnd = ConvertToGregorianDate(activityDetail.EndDate);
            }

            await _dBContext.SaveChangesAsync();
            await UpdateActivityAssignments([.. activityDetail.Employees], activity);
            await UpdateTaskAndPlanDetails(activityDetail.PlanId, activity.ShouldStart, activity.ShouldEnd, activity.Weight);
        }

        private async Tasks UpdateTaskAndPlanDetails(Guid? taskId, DateTime? start, DateTime? end, float weight)
        {
            if (taskId == null || taskId == Guid.Empty) return;

            var task = await _dBContext.Tasks.FirstOrDefaultAsync(x => x.Id.Equals(taskId));
            if (task == null) return;

            task.ShouldStartPeriod = start;
            task.ShouldEnd = end;
            task.Weight = weight;

            var plan = await _dBContext.Plans.FirstOrDefaultAsync(x => x.Id.Equals(task.PlanId));
            if (plan == null) return;

            var tasks = await _dBContext.Tasks.Where(x => x.PlanId == plan.Id).ToListAsync();
            plan.PeriodStartAt = tasks.Min(x => x.ShouldStartPeriod);
            plan.PeriodEndAt = tasks.Max(x => x.ShouldEnd);

            await _dBContext.SaveChangesAsync();
        }

        private async Tasks UpdateActivityAssignments(List<string> employees, PMActivity.Activity activity)
        {
            if (employees == null) return;

            var assignmentsToRemove = await _dBContext.EmployeesAssignedForActivities.Where(ea => ea.ActivityId == activity.Id).ToListAsync();
            _dBContext.EmployeesAssignedForActivities.RemoveRange(assignmentsToRemove);

            try
            {
                await _dBContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {   
                //TODO: Log error
                Console.WriteLine("Error updating assignments: " + ex.Message);
            }

            foreach (var employee in employees)
            {
                if (string.IsNullOrEmpty(employee)) continue;

                var EAFA = new EmployeesAssignedForActivities
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = activity.CreatedBy,
                    Id = Guid.NewGuid(),
                    ActivityId = activity.Id,
                    EmployeeId = Guid.Parse(employee),
                };

                _dBContext.EmployeesAssignedForActivities.Add(EAFA);
            }

            try
            {
                await _dBContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {   
                //TODO: Log error
                Console.WriteLine("Error updating assignments: " + ex.Message);
            }
        }

        public async Task<ResponseMessage> DeleteActivity(Guid activityId, Guid taskId)
    {
        try
        {
            // Get activities and their related data
            var activityParents = await _dBContext.ActivityParents.Where(x => x.Id == activityId).ToListAsync();
            var activities = await _dBContext.Activities.Where(x => x.Id == activityId || activityParents.All(ap => ap.Id == x.ActivityParentId)).ToListAsync();
            
            // Collect related data for deletion
            var activityProgresses = await _dBContext.ActivityProgresses.Where(x => activities.Select(a => a.Id).Contains(x.ActivityId)).ToListAsync();
            var progressAttachments = await _dBContext.ProgressAttachments.Where(x => activityProgresses.Select(ap => ap.Id).Contains(x.ActivityProgressId)).ToListAsync();
            var activityTargetDivisions = await _dBContext.ActivityTargetDivisions.Where(x => activities.Select(a => a.Id).Contains(x.ActivityId)).ToListAsync();
            var employeesAssigned = await _dBContext.EmployeesAssignedForActivities.Where(x => activities.Select(a => a.Id).Contains(x.ActivityId)).ToListAsync();

            // Remove related data
            _dBContext.ProgressAttachments.RemoveRange(progressAttachments);
            _dBContext.ActivityProgresses.RemoveRange(activityProgresses);
            _dBContext.ActivityTargetDivisions.RemoveRange(activityTargetDivisions);
            _dBContext.EmployeesAssignedForActivities.RemoveRange(employeesAssigned);
            _dBContext.Activities.RemoveRange(activities);
            _dBContext.ActivityParents.RemoveRange(activityParents);

            // Save all changes
            await _dBContext.SaveChangesAsync();

            // Update task and plan information
            var task = await _dBContext.Tasks.FirstOrDefaultAsync(x => x.Id.Equals(taskId));
            if (task != null)
            {
                var plan = await _dBContext.Plans.FirstOrDefaultAsync(x => x.Id.Equals(task.PlanId));
                if (plan != null)
                {
                    var actParents = await _dBContext.ActivityParents.Where(x => x.TaskId == task.Id).ToListAsync();
                    if (actParents.Count != 0)
                    {
                        task.ShouldStartPeriod = actParents.Min(x => x.ShouldStartPeriod);
                        task.ShouldEnd = actParents.Max(x => x.ShouldEnd);
                        task.Weight = actParents.Sum(x => x.Weight);
                    }

                    var tasks = await _dBContext.Tasks.Where(x => x.PlanId == plan.Id).ToListAsync();
                    if (tasks.Count != 0)
                    {
                        plan.PeriodStartAt = tasks.Min(x => x.ShouldStartPeriod);
                        plan.PeriodEndAt = tasks.Max(x => x.ShouldEnd);
                    }

                    await _dBContext.SaveChangesAsync();
                }
            }

            return new ResponseMessage
            {
                Message = "Activity Deleted Successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            //TODO: Log the error
            return new ResponseMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    }
}

