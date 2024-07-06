using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_API.Models.KPI;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.DTOS.KPI;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.KPI;
using PM_Case_Managemnt_Infrustructure.Models.PM;

namespace PM_Case_Managemnt_Implementation.Services.KPI
{
    public class KPIService : IKPIService
    {
        private readonly ApplicationDbContext _dbContext;

        public KPIService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResponseMessage<int>> AddKPI(KPIPostDto kpiPost)
        {
            try
            {
                KPIList kpi = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = kpiPost.CreatedBy,
                    Title = kpiPost.Title,
                    StartYear = kpiPost.StartYear,

                    ActiveYearsString = kpiPost.ActiveYearsString

                };

                if (!kpiPost.HasSubsidiaryOrganization)
                {

                    string output = string.Concat(kpiPost.EncoderOrganizationName.Split(' ')
                                              .Where(w => !string.IsNullOrWhiteSpace(w))
                                              .Select(w => char.ToUpper(w[0])));
                    string uniqueIdentifier = Guid.NewGuid().ToString("N").Substring(0, 5);


                    kpi.AccessCode = $"{output}-{uniqueIdentifier}";
                    kpi.EncoderOrganizationName = kpiPost.EncoderOrganizationName;
                    kpi.EvaluatorOrganizationName = kpiPost.EvaluatorOrganizationName;
                    //kpi.Url = kpiPost.Url;
                }
                else
                {
                    var subOrg = await _dbContext.SubsidiaryOrganizations.Where(x => x.Id == kpiPost.SubsidiaryOrganizationId).Select(x => x.OrganizationNameEnglish).FirstOrDefaultAsync();
                    kpi.EncoderOrganizationName = subOrg;
                    kpi.EvaluatorOrganizationName = kpiPost.EvaluatorOrganizationName;
                    kpi.HasSubsidiaryOrganization = kpiPost.HasSubsidiaryOrganization;
                    kpi.SubsidiaryOrganizationId = kpiPost.SubsidiaryOrganizationId;
                }



                //kpi.SetActiveYearsFromList(kpiPost.ActiveYears);

                await _dbContext.KPIs.AddAsync(kpi);
                await _dbContext.SaveChangesAsync();

                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "KPI Added Successfully"
                };

            }
            catch (Exception ex)
            {
                return new ResponseMessage<int>
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }


        public async Task<List<SelectListDto>> GetKpiGoalSelectList(Guid subOrgId)
        {
            var kpiGoal = await _dbContext.KPIDetails.Where(x => x.KPI.SubsidiaryOrganizationId == subOrgId).Select(x => new SelectListDto
            {

                Name = x.MainGoal,
                Id = x.Id
            }).ToListAsync();

            return kpiGoal;
        }

        public async Task<ResponseMessage<int>> AddKpiGoal(KPIGoalPostDto kpiGoalPost)
        {
            try
            {
                var Kpi = await _dbContext.KPIs.AnyAsync(x => x.Id == kpiGoalPost.KPIId);

                if (!Kpi)
                {
                    return new ResponseMessage<int> { Success = false, Message = "KPI Not Found" };
                }

                var kpiGoal = new KPIDetails
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = kpiGoalPost.CreatedBy,
                    KPIId = kpiGoalPost.KPIId,
                    MainGoal = kpiGoalPost.Goal,

                };

                await _dbContext.KPIDetails.AddAsync(kpiGoal);
                await _dbContext.SaveChangesAsync();

                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "KPI Goal Added Successfully"
                };


            }
            catch (Exception ex)
            {
                return new ResponseMessage<int> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ResponseMessage<int>> AddKPIDetail(KPIDetailsPostDto kpiDetailsPost)
        {
            try
            {
                var Kpi = await _dbContext.KPIs.AnyAsync(x => x.Id == kpiDetailsPost.KPIId);

                if (!Kpi)
                {
                    return new ResponseMessage<int> { Success = false, Message = "KPI Not Found" };
                }

                var kpiDetails = new List<KPIDetails>();

                var goalId = Guid.NewGuid();

                foreach (var k in kpiDetailsPost.Titles)
                {
                    kpiDetails.Add(new KPIDetails
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = kpiDetailsPost.CreatedBy,
                        KPIId = kpiDetailsPost.KPIId,
                        MainGoal = kpiDetailsPost.Goal,
                        Title = k.Title,
                        StartYearProgress = k.StartYearProgress,
                        GoalId = goalId
                    });
                }


                await _dbContext.KPIDetails.AddRangeAsync(kpiDetails);
                await _dbContext.SaveChangesAsync();

                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "KPI Details Added Successfully"
                };

            }
            catch (Exception ex)
            {
                return new ResponseMessage<int> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ResponseMessage<int>> AddKPIData(KPIDataPostDto kpiDataPost)
        {
            try
            {

                var KpiDetail = await _dbContext.KPIDetails.FindAsync(kpiDataPost.KPIDetailId);

                if (KpiDetail == null)
                {
                    return new ResponseMessage<int> { Success = false, Message = "KPI Detail Not Found" };
                }



                var kpiData = new KPIData
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = kpiDataPost.CreatedBy,
                    Year = kpiDataPost.Year,
                    Data = kpiDataPost.Data,
                    KPIDetailId = kpiDataPost.KPIDetailId
                };





                await _dbContext.KPIDatas.AddAsync(kpiData);
                await _dbContext.SaveChangesAsync();

                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "KPI Datas Added Successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage<int> { Success = false, Message = ex.Message };
            }
        }

        public async Task<List<KPIGetDto>> GetKPIs()
        {
            var kpis = await _dbContext.KPIs.Select(x => new KPIGetDto
            {
                Title = x.Title,
                StartYear = x.StartYear,
                ActiveYearsString = x.ActiveYearsString,
                EncoderOrganizationName = x.EncoderOrganizationName,
                EvaluatorOrganizationName = x.EvaluatorOrganizationName,
                CreatedBy = x.CreatedBy,
                AccessCode = x.AccessCode,
                HasSubsidiaryOrganization = x.HasSubsidiaryOrganization,
                SubsidiaryOrganizationId = x.SubsidiaryOrganizationId,
                Id = x.Id

            }).ToListAsync();


            return kpis;
        }

        public async Task<KPIGetDto> GetKPIById(Guid id)
        {
            var kpis = await _dbContext.KPIs
                        .Where(x => x.Id == id || x.SubsidiaryOrganizationId == id)
                        .Select(x => new KPIGetDto
                        {
                            Id = x.Id,
                            Title = x.Title,
                            StartYear = x.StartYear,
                            ActiveYearsString = x.ActiveYearsString,
                            EncoderOrganizationName = x.EncoderOrganizationName,
                            EvaluatorOrganizationName = x.EvaluatorOrganizationName,
                            CreatedBy = x.CreatedBy,
                            Url = x.Url,
                            SubsidiaryOrganizationId = x.SubsidiaryOrganizationId,
                            AccessCode = x.AccessCode,
                            HasSubsidiaryOrganization = x.HasSubsidiaryOrganization,

                        })
                        .FirstOrDefaultAsync();

            if (kpis == null)
            {
                return new KPIGetDto();
            }

            kpis.ActiveYears = kpis.ActiveYearsString?.Split(',').Select(int.Parse).ToList() ?? new List<int>();


            if (kpis.HasSubsidiaryOrganization)
            {


                kpis.KpiDetails = await _dbContext.Activities
                                                .Join(_dbContext.KPIDetails.Where(d => d.KPIId == kpis.Id),
                                                      activity => activity.KpiGoalId,
                                                      kpiDetail => kpiDetail.Id,
                                                      (activity, kpiDetail) => new { activity, kpiDetail.MainGoal })
                                                .GroupBy(x => x.activity.KpiGoalId)
                                                .Select(g => new GroupedKPIDetailsGetDto
                                                {
                                                    MainGoal = g.First().activity.KpiGoal.MainGoal,
                                                    Details = g.Select(x => new KPIDetailsGetDto
                                                    {
                                                        Id = x.activity.Id,
                                                        Title = x.activity.ActivityDescription,
                                                        StartYearProgress = x.activity.Begining,
                                                        MainGoal = x.activity.KpiGoal.MainGoal,
                                                        KPIDatas = new List<KPIDataGetDto>
                                                                {
                                                                    new KPIDataGetDto
                                                                    {
                                                                        Id = x.activity.Id,
                                                                        Year = x.activity.ShouldEnd.Year,
                                                                        Data = (_dbContext.ActivityProgresses
                                                                            .Where(z => z.ActivityId == x.activity.Id
                                                                                && (z.IsApprovedByDirector == approvalStatus.approved
                                                                                    || z.IsApprovedByFinance == approvalStatus.approved
                                                                                    || z.IsApprovedByManager == approvalStatus.approved))
                                                                            .Sum(z => z.ActualWorked)).ToString()
                                                                    }
                                                                }
                                                    }).ToList()

                                                })
                                                .ToListAsync();





            }
            else
            {


                kpis.KpiDetails = _dbContext.KPIDetails
                                .Where(d => d.KPIId == kpis.Id)
                                .GroupBy(d => d.GoalId)
                                .Select(group => new GroupedKPIDetailsGetDto
                                {
                                    MainGoal = group.First().MainGoal,
                                    Details = group.Select(d => new KPIDetailsGetDto
                                    {
                                        Id = d.Id,
                                        Title = d.Title,
                                        MainGoal = d.MainGoal,
                                        StartYearProgress = d.StartYearProgress,
                                        KPIDatas = _dbContext.KPIDatas
                                            .Where(z => z.KPIDetailId == d.Id)
                                            .Select(z => new KPIDataGetDto
                                            {
                                                Id = z.Id,
                                                Year = z.Year,
                                                Data = z.Data,
                                            }).ToList()
                                    }).ToList()
                                }).ToList();
            }

            return kpis;
        }
        public async Task<ResponseMessage<int>> UpdateKPI(KPIGetDto kpiGet)
        {
            try
            {
                var kpi = await _dbContext.KPIs.FindAsync(kpiGet.Id);

                if (kpi == null)
                {
                    return new ResponseMessage<int> { Success = false, Message = "KPI Not Found" };
                }
                kpi.StartYear = kpiGet.StartYear;
                kpi.Title = kpiGet.Title;
                kpi.EncoderOrganizationName = kpiGet.EncoderOrganizationName;
                kpi.EvaluatorOrganizationName = kpiGet.EvaluatorOrganizationName;
                kpi.Url = kpiGet.Url;
                //if (kpiGet.ActiveYears.Count > 0)
                //{
                //    kpi.SetActiveYearsFromList(kpiGet.ActiveYears);
                //}
                kpi.ActiveYearsString = kpiGet.ActiveYearsString;

                await _dbContext.SaveChangesAsync();

                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "KPI Updated Successfully"
                };



            }
            catch (Exception ex)
            {
                return new ResponseMessage<int> { Success = false, Message = ex.Message };
            }

        }

        public async Task<ResponseMessage<int>> UpdateKPIDetail(KPIDetailsGetDto kpiDetailsGet)
        {
            try
            {
                var kpiDetail = await _dbContext.KPIDetails.FindAsync(kpiDetailsGet.Id);

                if (kpiDetail == null)
                {
                    return new ResponseMessage<int> { Success = false, Message = "KPI Detail Not Found" };
                }

                kpiDetail.Title = kpiDetailsGet.Title;
                kpiDetail.MainGoal = kpiDetailsGet.MainGoal;

                await _dbContext.SaveChangesAsync();

                return new ResponseMessage<int>
                {
                    Success = true,
                    Message = "KPI Detail Updated Successfully"
                };

            }
            catch (Exception ex)
            {
                return new ResponseMessage<int> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ResponseMessage<string>> LoginKpiDataEncoding(string accessCode)
        {

            var kpiId = await _dbContext.KPIs.AsNoTracking().Where(x => x.AccessCode == accessCode).Select(x => x.Id).FirstOrDefaultAsync();

            if (kpiId == null)
            {
                return new ResponseMessage<string>
                {
                    Success = false,
                    Message = "Access Code Is Invalid"
                };
            }

            return new ResponseMessage<string>
            {
                Success = true,
                Message = "Log In Successfull",
                Data = kpiId.ToString()

            };

        }
    }
}
