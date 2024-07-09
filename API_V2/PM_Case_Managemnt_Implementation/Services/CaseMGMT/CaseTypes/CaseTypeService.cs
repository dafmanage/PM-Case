using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;
using System.Net;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.CaseTypes
{

    public class CaseTypeService : ICaseTypeService
    {

        private readonly ApplicationDbContext _dbContext;
        public CaseTypeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResponseMessage<int>> Add(CaseTypePostDto caseTypeDto)
        {
            var response = new ResponseMessage<int>();
            try
            {
                CaseType caseType = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    RowStatus = RowStatus.Active,
                    CreatedBy = caseTypeDto.CreatedBy,
                    CaseTypeTitle = caseTypeDto.CaseTypeTitle,
                    Code = caseTypeDto.Code,
                    TotlaPayment = caseTypeDto.TotalPayment,
                    Counter = caseTypeDto.Counter,
                    MeasurementUnit = Enum.Parse<TimeMeasurement>(caseTypeDto.MeasurementUnit),
                    CaseForm = string.IsNullOrEmpty(caseTypeDto.CaseForm) ? _dbContext.CaseTypes.Find(caseTypeDto.ParentCaseTypeId).CaseForm : Enum.Parse<CaseForm>(caseTypeDto.CaseForm),
                    Remark = caseTypeDto.Remark,
                    OrderNumber = caseTypeDto.OrderNumber,
                    ParentCaseTypeId = caseTypeDto.ParentCaseTypeId,
                    SubsidiaryOrganizationId = caseTypeDto.SubsidiaryOrganizationId,
                };

                if (caseTypeDto.ParentCaseTypeId != null)
                    caseType.ParentCaseTypeId = caseTypeDto.ParentCaseTypeId;

                await _dbContext.AddAsync(caseType);
                await _dbContext.SaveChangesAsync();

                response.Data = 1;
                response.Message = "Opertaion Successfull";
                response.Success = true;

                return response;

            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }




        public async Task<ResponseMessage<int>> UpdateCaseType(CaseTypePostDto caseTypeDto)
        {
            var response = new ResponseMessage<int>();
            try
            {
                var caseType = await _dbContext.CaseTypes.FindAsync(caseTypeDto.Id);
                if (caseType == null)
                {

                    response.Message = "Could not find matching case type.";
                    response.Data = 0;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();
                    response.Success = false;

                    return response;
                }
                caseType.CaseTypeTitle = caseTypeDto.CaseTypeTitle;
                caseType.TotlaPayment = caseTypeDto.TotalPayment;
                caseType.Code = caseTypeDto.Code;
                caseType.Remark = caseTypeDto.Remark;
                caseType.Counter = caseTypeDto.Counter;
                caseType.MeasurementUnit = Enum.Parse<TimeMeasurement>(caseTypeDto.MeasurementUnit);


                await _dbContext.SaveChangesAsync();
                response.Message = "Operation Succsessfull.";
                response.Data = 1;
                response.Success = true;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }





        public async Task<ResponseMessage<List<CaseTypeGetDto>>> GetAll(Guid subOrgId)
        {
            var response = new ResponseMessage<List<CaseTypeGetDto>>();
            try
            {
                List<CaseType> caseTypes = await _dbContext.CaseTypes.Include(p => p.ParentCaseType).Where(x => x.ParentCaseTypeId == null && x.SubsidiaryOrganizationId == subOrgId).ToListAsync();
                List<CaseTypeGetDto> result = [];

                foreach (CaseType caseType in caseTypes)
                {
                    result.Add(new CaseTypeGetDto
                    {
                        Id = caseType.Id,
                        CaseTypeTitle = caseType.CaseTypeTitle,
                        Code = caseType.Code,
                        CreatedAt = caseType.CreatedAt.ToString(),
                        CreatedBy = caseType.CreatedBy,
                        MeasurementUnit = caseType.MeasurementUnit.ToString(),
                        Remark = caseType.Remark,
                        RowStatus = caseType.RowStatus.ToString(),
                        Counter = caseType.Counter,

                        TotalPayment = caseType.TotlaPayment,
                        Children = _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseType.Id).Select(y => new CaseTypeGetDto
                        {
                            Id = y.Id,
                            CaseTypeTitle = y.CaseTypeTitle,
                            Code = y.Code,
                            CreatedAt = y.CreatedAt.ToString(),
                            CreatedBy = y.CreatedBy,
                            Counter = y.Counter,
                            MeasurementUnit = y.MeasurementUnit.ToString(),
                            Remark = y.Remark,
                            RowStatus = y.RowStatus.ToString(),
                            TotalPayment = y.TotlaPayment,

                        }).ToList()
                        //ParentCaseType = caseType.ParentCaseType
                    });
                }

                response.Message = "Operation Successfull.";
                response.Data = result;
                response.Success = true;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }
        public async Task<ResponseMessage<List<SelectListDto>>> GetAllByCaseForm(string caseForm, Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();
            try
            {
                List<CaseType> caseTypes = await _dbContext.CaseTypes.Include(p => p.ParentCaseType).Where(x => x.CaseForm == Enum.Parse<CaseForm>(caseForm) && x.ParentCaseTypeId == null && x.SubsidiaryOrganizationId == subOrgId).ToListAsync();
                List<SelectListDto> result = [];

                foreach (CaseType caseType in caseTypes)
                {
                    result.Add(new SelectListDto
                    {
                        Id = caseType.Id,
                        Name = caseType.CaseTypeTitle,


                    });
                }

                response.Message = "Operation Successfull";
                response.Data = result;
                response.Success = true;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }
        }


        public async Task<ResponseMessage<List<SelectListDto>>> GetAllSelectList(Guid subOrgId)
        {
            var response = new ResponseMessage<List<SelectListDto>>();

            List<SelectListDto> result = await (from c in _dbContext.CaseTypes.Where(x => x.SubsidiaryOrganizationId == subOrgId)
                                                select new SelectListDto
                                                {
                                                    Id = c.Id,
                                                    Name = c.CaseTypeTitle

                                                }).ToListAsync();
            response.Data = result;
            response.Message = "Operation Successfull.";
            response.Success = true;

            return response;

        }
        public async Task<ResponseMessage<List<SelectListDto>>> GetFileSettigs(Guid caseTypeId)
        {

            var response = new ResponseMessage<List<SelectListDto>>();

            List<SelectListDto> result = await (from f in _dbContext.FileSettings.Where(x => x.CaseTypeId == caseTypeId)
                                                select new SelectListDto
                                                {

                                                    Id = f.Id,
                                                    Name = f.FileName

                                                }).ToListAsync();

            response.Message = "Operation Successfull.";
            response.Data = result;
            response.Success = true;

            return response;


        }

        public async Task<ResponseMessage<List<SelectListDto>>> GetChildCases(Guid caseTypeId)
        {

            var response = new ResponseMessage<List<SelectListDto>>();
            List<SelectListDto> result = await (from f in _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseTypeId)
                    .OrderBy(x => x.OrderNumber)
                                                select new SelectListDto
                                                {

                                                    Id = f.Id,
                                                    Name = f.CaseTypeTitle

                                                }).ToListAsync();

            response.Message = "Operational Message.";
            response.Data = result;
            response.Success = true;

            return response;
        }


        public async Task<ResponseMessage<int>> GetChildOrder(Guid caseTypeId)
        {

            var response = new ResponseMessage<int>();

            var childCases = await _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseTypeId).OrderByDescending(x => x.OrderNumber).ToListAsync();
            response.Message = "Operation Successfull.";
            response.Success = true;
            if (!childCases.Any())
                response.Data = 1;
            else
                response.Data = (int)childCases.FirstOrDefault().OrderNumber + 1;

            return response;

        }

        public async Task<ResponseMessage<int>> DeleteCaseType(Guid caseTypeId)
        {

            var response = new ResponseMessage<int>();
            var caseType = await _dbContext.CaseTypes.FindAsync(caseTypeId);

            var childCases = await _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseTypeId).ToListAsync();

            if (caseType != null)
            {
                _dbContext.CaseTypes.RemoveRange(childCases);
                await _dbContext.SaveChangesAsync();

                _dbContext.CaseTypes.Remove(caseType);
                await _dbContext.SaveChangesAsync();
                response.Message = "Deleted Successfully.";
                response.Data = 1;
                response.Success = true;

                return response;
            }

            response.Message = "Could not find case type.";
            response.Data = 0;
            response.ErrorCode = HttpStatusCode.NotFound.ToString();
            response.Success = false;

            return response;

            ;
        }


        public async Task<ResponseMessage<List<CaseTypeGetDto>>> GetCaseTypeChildren(Guid caseTypeId)
        {

            var response = new ResponseMessage<List<CaseTypeGetDto>>();
            try
            {

                List<CaseTypeGetDto> children = await _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseTypeId).Select(y => new CaseTypeGetDto
                {
                    Id = y.Id,
                    CaseTypeTitle = y.CaseTypeTitle,
                    Code = y.Code,
                    CreatedAt = y.CreatedAt.ToString(),
                    CreatedBy = y.CreatedBy,
                    Counter = y.Counter,
                    MeasurementUnit = y.MeasurementUnit.ToString(),
                    Remark = y.Remark,
                    RowStatus = y.RowStatus.ToString(),
                    TotalPayment = y.TotlaPayment,

                }).ToListAsync();

                response.Success = true;
                response.Data = children;
                response.Message = "Opertaion Successfull.";

                return response;
            }

            catch (Exception ex)
            {

                response.Message = $"{ex.Message}";
                response.Data = null;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();
                response.Success = false;

                return response;
            }

        }

    }
}
