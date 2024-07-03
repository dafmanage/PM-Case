using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.PM;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Services.PM.Plann;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using PM_Case_Managemnt_Infrustructure.Models.PM;

//
using Tasks = System.Threading.Tasks.Task;

namespace PM_Case_Managemnt_Implementation.Services.PM.Program
{
    public class ProgramService(ApplicationDbContext context, IPlanService planService) : IProgramService
    {

        private readonly ApplicationDbContext _dBContext = context;
        private readonly IPlanService planService = planService;

        public async Task<int> CreateProgram(Programs program)
        {
            program.Id = Guid.NewGuid();
            program.CreatedAt = DateTime.Now;

            await _dBContext.AddAsync(program);
            await _dBContext.SaveChangesAsync();

           return 1;
        }

        public async Task<List<ProgramDto>> GetPrograms(Guid subOrgId)
        {
            var programs = await (from p in _dBContext.Programs
                                .Where(x => x.SubsidiaryOrganizationId == subOrgId)
                                .Include(x => x.ProgramBudgetYear)
                                select new ProgramDto
                                {
                                    Id = p.Id,
                                    ProgramName = p.ProgramName,
                                    ProgramBudgetYear = $"{p.ProgramBudgetYear.Name} ({p.ProgramBudgetYear.FromYear} - {p.ProgramBudgetYear.ToYear})",
                                    NumberOfProjects = _dBContext.Plans.Count(x => x.ProgramId == p.Id),
                                    ProgramStructure = _dBContext.Plans
                                                        .Include(x => x.Structure)
                                                        .Where(x => x.ProgramId == p.Id)
                                                        .GroupBy(x => x.Structure.StructureName)
                                                        .Select(g => new ProgramStructureDto
                                                        {
                                                            StructureName = $"{g.Key} ({_dBContext.Employees.FirstOrDefault(y => y.OrganizationalStructureId == g.First().StructureId && y.Position == Position.Director).FullName})",
                                                            StructureHead = $"{g.Count()} Projects"
                                                        })
                                                        .ToList(),
                                    ProgramPlannedBudget = p.ProgramPlannedBudget,
                                    Remark = p.Remark
                                })
                                .ToListAsync();

            return programs;
        }


        public async Task<List<SelectListDto>> GetProgramsSelectList(Guid subOrgId)
        {
            return await (from p in _dBContext.Programs.Where(n => n.SubsidiaryOrganizationId == subOrgId)
                          select new SelectListDto
                          {
                              Id = p.Id,
                              Name = p.ProgramName
                          }).ToListAsync();
        }

        public async Task<ProgramDto> GetProgramsById(Guid programId)
        {
            var program = await _dBContext.Programs
                                        .Include(x => x.ProgramBudgetYear)
                                        .FirstOrDefaultAsync(x => x.Id == programId);

            if (program == null)
            {
                // Handle case where program with given id is not found
                return null;
            }

            var numberOfProjects = await _dBContext.Plans
                                                .CountAsync(x => x.ProgramId == programId);

            var remainingBudget = program.ProgramPlannedBudget - await _dBContext.Plans
                                                                                .Where(x => x.ProgramId == program.Id)
                                                                                .SumAsync(x => x.PlandBudget);

            var remainingWeight = 100 - await _dBContext.Plans
                                                        .Where(x => x.ProgramId == program.Id)
                                                        .SumAsync(x => x.PlanWeight);

            var programDto = new ProgramDto
            {
                ProgramName = program.ProgramName,
                ProgramBudgetYear = $"{program.ProgramBudgetYear.Name} ({program.ProgramBudgetYear.FromYear} - {program.ProgramBudgetYear.ToYear})",
                NumberOfProjects = numberOfProjects,
                ProgramPlannedBudget = program.ProgramPlannedBudget,
                RemainingBudget = remainingBudget,
                RemainingWeight = remainingWeight,
                Remark = program.Remark
            };

            return programDto;
        }

        public async Task<ResponseMessage> UpdateProgram(ProgramPostDto program)
        {
            var prog = await _dBContext.Programs.FindAsync(program.Id);
            if (prog != null)
            {
                prog.ProgramName = program.ProgramName;
                prog.ProgramPlannedBudget = program.ProgramPlannedBudget;
                prog.ProgramBudgetYearId = program.ProgramBudgetYearId;
                prog.Remark = program.Remark;

                await _dBContext.SaveChangesAsync();

                return new ResponseMessage
                {
                    Success = true,
                    Message = "Program Updated Successfully"
                };
            }
            else
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Program Not Found"
                };
            }
        }

        public async Task<ResponseMessage> DeleteProgram(Guid programId)
        {
            var prog = await _dBContext.Programs.FindAsync(programId);
            
            if (prog == null)
            {
                return new ResponseMessage
                {
                    Message = "Program Not Found!!!",
                    Success = false
                };
            }

            try
            {
                var plans = await _dBContext.Plans.Where(x => x.ProgramId == programId).ToListAsync();

                foreach (var plan in plans)
                {
                    await DeletePlanTasks(plan.Id);
                    _dBContext.Plans.Remove(plan);
                }

                _dBContext.Programs.Remove(prog);
                await _dBContext.SaveChangesAsync();

                return new ResponseMessage
                {
                    Success = true,
                    Message = "Program Deleted Successfully !!!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private async Tasks DeletePlanTasks(Guid planId)
        {
            var tasks = await _dBContext.Tasks.Where(x => x.PlanId == planId).ToListAsync();

            foreach (var task in tasks)
            {
                await DeleteTaskDetails(task.Id);
                _dBContext.Tasks.Remove(task);
            }
        }

        private async Tasks DeleteTaskDetails(Guid taskId)
        {
            var taskMemos = await _dBContext.TaskMemos.Where(x => x.TaskId == taskId).ToListAsync();
            var taskMembers = await _dBContext.TaskMembers.Where(x => x.TaskId == taskId).ToListAsync();
            var activityParents = await _dBContext.ActivityParents.Where(x => x.TaskId == taskId).ToListAsync();

            foreach (var activityParent in activityParents)
            {
                await DeleteActivities(activityParent.Id);
                _dBContext.ActivityParents.Remove(activityParent);
            }

            _dBContext.TaskMemos.RemoveRange(taskMemos);
            _dBContext.TaskMembers.RemoveRange(taskMembers);
        }
        private async Tasks DeleteActivities(Guid activityParentId)
        {
            var activities = await _dBContext.Activities.Where(x => x.ActivityParentId == activityParentId).ToListAsync();

            foreach (var activity in activities)
            {
                await DeleteActivityDetails(activity.Id);
                _dBContext.Activities.Remove(activity);
            }
        }
        private async Tasks DeleteActivityDetails(Guid activityId)
        {
            var actProgresses = await _dBContext.ActivityProgresses.Where(x => x.ActivityId == activityId).ToListAsync();
            var progAttachments = await _dBContext.ProgressAttachments.Where(x => x.ActivityProgressId == activityId).ToListAsync();
            var activityTargets = await _dBContext.ActivityTargetDivisions.Where(x => x.ActivityId == activityId).ToListAsync();
            var employeesAssigned = await _dBContext.EmployeesAssignedForActivities.Where(x => x.ActivityId == activityId).ToListAsync();

            _dBContext.ActivityProgresses.RemoveRange(actProgresses);
            _dBContext.ProgressAttachments.RemoveRange(progAttachments);
            _dBContext.ActivityTargetDivisions.RemoveRange(activityTargets);
            _dBContext.EmployeesAssignedForActivities.RemoveRange(employeesAssigned);
        }

    }
}
