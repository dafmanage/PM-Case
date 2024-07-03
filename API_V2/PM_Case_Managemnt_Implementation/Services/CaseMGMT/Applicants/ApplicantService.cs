using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;


namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.Applicants
{
    public class ApplicantService : IApplicantService
    {
        private readonly ApplicationDbContext _dbContext;

        public ApplicantService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Add(ApplicantPostDto applicantPost)
        {
            try
            {
                Applicant applicant = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = applicantPost.CreatedBy,
                    ApplicantName = applicantPost.ApplicantName,
                    ApplicantType = Enum.Parse<ApplicantType>(applicantPost.ApplicantType),
                    CustomerIdentityNumber = applicantPost.CustomerIdentityNumber,
                    Email = applicantPost.Email,
                    PhoneNumber = applicantPost.PhoneNumber,
                    Remark = applicantPost.PhoneNumber,
                    RowStatus = RowStatus.Active,
                    SubsidiaryOrganizationId = applicantPost.SubsidiaryOrganizationId,
                };

                await _dbContext.Applicants.AddAsync(applicant);
                await _dbContext.SaveChangesAsync();
                return applicant.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding applicant");
            }
        }

        public async Task<Guid> Update(ApplicantPostDto applicantPost)
        {
            try
            {
                var applicant = await _dbContext.Applicants.FindAsync(applicantPost.ApplicantId);


                applicant.ApplicantName = applicantPost.ApplicantName;
                applicant.ApplicantType = Enum.Parse<ApplicantType>(applicantPost.ApplicantType);
                applicant.CustomerIdentityNumber = applicantPost.CustomerIdentityNumber;
                applicant.Email = applicantPost.Email;
                applicant.PhoneNumber = applicantPost.PhoneNumber;

                await _dbContext.SaveChangesAsync();

                return applicant.Id;

            }
            catch (Exception ex)
            {
                throw new Exception("Error adding applicant");
            }
        }

        public async Task<List<ApplicantGetDto>> GetAll(Guid subOrgId)
        {
            try
            {
                List<Applicant> applicants = await _dbContext.Applicants.Where(x => x.SubsidiaryOrganizationId == subOrgId).ToListAsync();
                List<ApplicantGetDto> result = [];

                foreach (Applicant applicant in applicants)
                {
                    result.Add(new ApplicantGetDto()
                    {
                        Id = applicant.Id,
                        ApplicantName = applicant.ApplicantName,
                        ApplicantType = applicant.ApplicantType.ToString(),
                        CreatedAt = applicant.CreatedAt,
                        CreatedBy = applicant.CreatedBy,
                        CustomerIdentityNumber = applicant.CustomerIdentityNumber,
                        Email = applicant.Email,
                        PhoneNumber = applicant.PhoneNumber,
                        RowStatus = applicant.RowStatus
                    });
                }

                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<Applicant> GetApplicantById(Guid? applicantId)
        {

            var applicant = await _dbContext.Applicants.FindAsync(applicantId);

            return applicant;
        }
        public async Task<List<SelectListDto>> GetSelectList(Guid subOrgId)
        {
            try
            {
                List<Applicant> applicants = await _dbContext.Applicants.Where(x => x.SubsidiaryOrganizationId == subOrgId).OrderBy(x => x.ApplicantName).ToListAsync();
                List<SelectListDto> result = [];

                foreach (Applicant applicant in applicants)
                {
                    result.Add(new SelectListDto()
                    {
                        Id = applicant.Id,
                        Name = applicant.ApplicantName + " ( " + applicant.CustomerIdentityNumber + " ) ",

                    });
                }

                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
