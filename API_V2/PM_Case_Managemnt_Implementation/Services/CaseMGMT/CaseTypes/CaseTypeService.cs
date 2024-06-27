using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.CaseTypes
{

    public class CaseTypeService : ICaseTypeService
    {

        private readonly ApplicationDbContext _dbContext;
        public CaseTypeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(CaseTypePostDto caseTypeDto)
        {
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

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




        public async Task UpdateCaseType(CaseTypePostDto caseTypeDto)
        {
            try
            {
                var caseType = await _dbContext.CaseTypes.FindAsync(caseTypeDto.Id);

                caseType.CaseTypeTitle = caseTypeDto.CaseTypeTitle;
                caseType.TotlaPayment = caseTypeDto.TotalPayment;
                caseType.Code = caseTypeDto.Code;
                caseType.Remark = caseTypeDto.Remark;
                caseType.Counter = caseTypeDto.Counter;
                caseType.MeasurementUnit = Enum.Parse<TimeMeasurement>(caseTypeDto.MeasurementUnit);


                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }





        public async Task<List<CaseTypeGetDto>> GetAll(Guid subOrgId)
        {
            try
            {
                List<CaseType> caseTypes = await _dbContext.CaseTypes.Include(p => p.ParentCaseType).Where(x => x.ParentCaseTypeId == null && x.SubsidiaryOrganizationId == subOrgId).ToListAsync();
                List<CaseTypeGetDto> result = new();

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

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<SelectListDto>> GetAllByCaseForm(string caseForm, Guid subOrgId)
        {
            try
            {
                List<CaseType> caseTypes = await _dbContext.CaseTypes.Include(p => p.ParentCaseType).Where(x => x.CaseForm == Enum.Parse<CaseForm>(caseForm) && x.ParentCaseTypeId == null && x.SubsidiaryOrganizationId == subOrgId).ToListAsync();
                List<SelectListDto> result = new();

                foreach (CaseType caseType in caseTypes)
                {
                    result.Add(new SelectListDto
                    {
                        Id = caseType.Id,
                        Name = caseType.CaseTypeTitle,


                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<SelectListDto>> GetAllSelectList(Guid subOrgId)
        {

            return await (from c in _dbContext.CaseTypes.Where(x => x.SubsidiaryOrganizationId == subOrgId)
                          select new SelectListDto
                          {
                              Id = c.Id,
                              Name = c.CaseTypeTitle

                          }).ToListAsync();

        }
        public async Task<List<SelectListDto>> GetFileSettigs(Guid caseTypeId)
        {

            return await (from f in _dbContext.FileSettings.Where(x => x.CaseTypeId == caseTypeId)
                          select new SelectListDto
                          {

                              Id = f.Id,
                              Name = f.FileName

                          }).ToListAsync();

        }

        public async Task<List<SelectListDto>> GetChildCases(Guid caseTypeId)
        {
            return await (from f in _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseTypeId)
                          .OrderBy(x => x.OrderNumber)
                          select new SelectListDto
                          {

                              Id = f.Id,
                              Name = f.CaseTypeTitle

                          }).ToListAsync();
        }


        public int GetChildOrder(Guid caseTypeId)
        {

            var childCases = _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseTypeId).OrderByDescending(x => x.OrderNumber).ToList();

            if (!childCases.Any())
                return 1;
            else
                return (int)childCases.FirstOrDefault().OrderNumber + 1;

        }

        public async Task DeleteCaseType(Guid caseTypeId)
        {

            var caseType = await _dbContext.CaseTypes.FindAsync(caseTypeId);

            var childCases = await _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseTypeId).ToListAsync();

            if (caseType != null)
            {
                _dbContext.CaseTypes.RemoveRange(childCases);
                await _dbContext.SaveChangesAsync();

                _dbContext.CaseTypes.Remove(caseType);
                await _dbContext.SaveChangesAsync();
            }

;
        }


        public async Task<List<CaseTypeGetDto>> GetCaseTypeChildren(Guid caseTypeId)
        {
            var children = await _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == caseTypeId).Select(y => new CaseTypeGetDto
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

            return children;
        }

    }
}
