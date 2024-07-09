using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.Applicants
{
    public class ApplicantService(ApplicationDbContext dbContext) : IApplicantService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<ResponseMessage<Guid>> Add(ApplicantPostDto applicantPost)
        {
            var response = new ResponseMessage<Guid>();
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
                    // Remark = applicantPost.Remark,
                    RowStatus = RowStatus.Active,
                    SubsidiaryOrganizationId = applicantPost.SubsidiaryOrganizationId,
                };

                await _dbContext.Applicants.AddAsync(applicant);
                await _dbContext.SaveChangesAsync();

                response.Success = true;
                response.Message = "Applicant added Successfully";
                response.Data = applicant.Id;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Message = $"Error adding applicant - {ex.Message}";
                response.Data = Guid.Empty;
            }
            return response;
        }

        public async Task<ResponseMessage<Guid>> Update(ApplicantPostDto applicantPost)
        {
            var response = new ResponseMessage<Guid>();
            try
            {
                var applicant = await _dbContext.Applicants.FindAsync(applicantPost.ApplicantId);

                if (applicant == null)
                {
                    response.Success = false;
                    response.Message = "Can't find applicant with that ID";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Data = default(Guid);
                    return response;
                }

                applicant.ApplicantName = applicantPost.ApplicantName;
                applicant.ApplicantType = Enum.Parse<ApplicantType>(applicantPost.ApplicantType);
                applicant.CustomerIdentityNumber = applicantPost.CustomerIdentityNumber;
                applicant.Email = applicantPost.Email;
                applicant.PhoneNumber = applicantPost.PhoneNumber;

                await _dbContext.SaveChangesAsync();
                
                response.Success = true;
                response.Message = "Updated Successfully";
                response.Data = applicant.Id;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error while updating";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Data = Guid.Empty;
            }
            return response;
        }

        public async Task<ResponseMessage<List<ApplicantGetDto>>> GetAll(Guid subOrgId)
        {
            var response = new ResponseMessage<List<ApplicantGetDto>>();
            try
            {
                List<Applicant> applicants = await _dbContext.Applicants.Where(x => x.SubsidiaryOrganizationId == subOrgId).ToListAsync();
                List<ApplicantGetDto> result = [];

                if (applicants == null || applicants.Count == 0)
                {
                    response.Success = false;
                    response.Message = "No applicants with the given requirments";
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Data = null;
                    return response;
                }
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

                response.Success = true;
                response.Message = "Applicants fetched";
                response.Data = result;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Message = $"Faced some kind of error: {ex.Message}";
                response.Data = null;
            }
            
            return response;
        }

        public async Task<ResponseMessage<ApplicantGetDto>> GetApplicantById(Guid? applicantId)
        {
            var response = new ResponseMessage<ApplicantGetDto>();
        
            if (applicantId == null)
            {
                response.Success = false;
                response.Message = "Applicant ID is null";
                response.ErrorCode = HttpStatusCode.BadRequest.ToString();
                return response;
            }
        
            var applicant = await _dbContext.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                response.Success = false;
                response.Message = "Applicant not found";
                response.ErrorCode = HttpStatusCode.NotFound.ToString();
                return response;
            }
        
            var applicantDto = new ApplicantGetDto
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
            };
            
            response.Success = true;
            response.Message = "Applicant found successfully";
            response.Data = applicantDto;
            return response;
        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetSelectList(Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            try
            {
                var result = await _dbContext.Applicants
                    .Where(x => x.SubsidiaryOrganizationId == subOrgId)
                    .OrderBy(x => x.ApplicantName)
                    .Select(applicant => new SelectListDto
                    {
                        Id = applicant.Id,
                        Name = $"{applicant.ApplicantName} ( {applicant.CustomerIdentityNumber} )"
                    })
                    .ToListAsync();
        
                response.Success = true;
                response.Message = "Select list retrieved Successfully";
                response.Data = result;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Faced error while retrieving";
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
            }
        
            return response;
        }
    }
}
