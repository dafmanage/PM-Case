using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_Implementation.DTOS.CaseDto;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Implementation.Hubs.EncoderHub;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Auth;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.CaseService.Encode
{
    public class CaseEncodeService : ICaseEncodeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private IHubContext<EncoderHub, IEncoderHubInterface> _encoderHub;

        public CaseEncodeService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager,
            IHubContext<EncoderHub, IEncoderHubInterface> encoderHub)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _encoderHub = encoderHub;
        }

        public async Task<ResponseMessage<string>> Add(CaseEncodePostDto caseEncodePostDto)
        {
            var response = new ResponseMessage<string>();
            try
            {
                if (caseEncodePostDto.EmployeeId == null && caseEncodePostDto.ApplicantId == null){

                    response.Message = "Please Provide an Applicant ID or Employee ID";
                    response.Data = null;
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();

                    return response;

                }


                Case newCase = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    RowStatus = RowStatus.Active,
                    CreatedBy = caseEncodePostDto.CreatedBy,
                    ApplicantId = caseEncodePostDto.ApplicantId,
                    EmployeeId = caseEncodePostDto.EmployeeId,
                    LetterNumber = caseEncodePostDto.LetterNumber,
                    LetterSubject = caseEncodePostDto.LetterSubject,
                    CaseTypeId = caseEncodePostDto.CaseTypeId,
                    AffairStatus = AffairStatus.Encoded,
                    PhoneNumber2 = caseEncodePostDto.PhoneNumber2,
                    Representative = caseEncodePostDto.Representative,
                    SubsidiaryOrganizationId = caseEncodePostDto.SubsidiaryOrganizationId

                };
                ResponseMessage<string> caseNumber = await GetCaseNumber(caseEncodePostDto.SubsidiaryOrganizationId);
                newCase.CaseNumber = caseNumber.Data;

                await _dbContext.AddAsync(newCase);
                await _dbContext.SaveChangesAsync();

                CaseType? caseType = _dbContext.CaseTypes.Find(caseEncodePostDto.CaseTypeId);

                if (caseType == null){
                    response.Message = "Case type not found.";
                    response.Data = null;
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();

                    return response;
                }
                if (caseType.CaseForm == CaseForm.Inside)
                {
                    Employee emp = _dbContext.Employees.Find(caseEncodePostDto.EmployeeId);
                    CaseAssignDto caseAssignDto = new CaseAssignDto
                    {
                        CaseId = caseType.Id,
                        AssignedByEmployeeId = emp.Id,
                        AssignedToEmployeeId = emp.Id,
                        AssignedToStructureId = emp.OrganizationalStructureId,
                        ForwardedToStructureId = new Guid[] { emp.OrganizationalStructureId }

                    };


                    try
                    {
                        var user = _userManager.Users.Where(x => x.EmployeesId == caseAssignDto.AssignedByEmployeeId).FirstOrDefault();
                        
                        if (user == null){

                            response.Message = "User not found.";
                            response.Data = null;
                            response.Success = false;
                            response.ErrorCode = HttpStatusCode.NotFound.ToString();

                            return response;

                        }

                        string userId = user.Id;
                        
                        Case? caseToAssign = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(newCase.Id));
                        // CaseHistory caseHistory = await _dbContext.CaseHistories.SingleOrDefaultAsync(el => el.CaseId.Equals(caseAssignDto.CaseId));
                        if (caseToAssign == null){

                            response.Message = "Case to assign not found.";
                            response.Data = null;
                            response.Success = false;
                            response.ErrorCode = HttpStatusCode.NotFound.ToString();

                            return response;
                        }
                        if (caseAssignDto.ForwardedToStructureId != null)
                        {
                            var startupHistory = new CaseHistory
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = DateTime.Now,
                                CreatedBy = Guid.Parse(userId),
                                RowStatus = RowStatus.Active,
                                CaseId = newCase.Id,
                                CaseTypeId = caseToAssign.CaseTypeId,
                                AffairHistoryStatus = AffairHistoryStatus.Pend,
                                FromEmployeeId = caseAssignDto.AssignedByEmployeeId,
                                FromStructureId = emp.OrganizationalStructureId,
                                ReciverType = ReciverType.Orginal,
                                ToStructureId = caseAssignDto.AssignedToStructureId,
                                ToEmployeeId = emp.Id,

                            };
                            startupHistory.SecreateryNeeded = (caseAssignDto.AssignedToEmployeeId == Guid.Empty || caseAssignDto.AssignedToEmployeeId == null) ? true : false;

                            caseToAssign.AffairStatus = AffairStatus.Assigned;
                            _dbContext.Entry(caseToAssign).Property(x => x.AffairStatus).IsModified = true;

                            await _dbContext.CaseHistories.AddAsync(startupHistory);
                            await _dbContext.SaveChangesAsync();

                            //foreach (var row in caseAssignDto.ForwardedToStructureId)
                            //{



                            //    CaseHistory history = new()
                            //    {
                            //        Id = Guid.NewGuid(),
                            //        CreatedAt = DateTime.Now,
                            //        CreatedBy = Guid.Parse(userId),
                            //        CaseId = newCase.Id,
                            //        AffairHistoryStatus = AffairHistoryStatus.Pend,
                            //        FromEmployeeId = caseAssignDto.AssignedByEmployeeId,
                            //        FromStructureId = emp.OrganizationalStructureId,
                            //        ToStructureId = row,
                            //        ReciverType = ReciverType.Cc,
                            //        ToEmployeeId = emp.Id,
                            //        RowStatus = RowStatus.Active,

                            //    };
                            //    await _dbContext.CaseHistories.AddAsync(history);
                            //    await _dbContext.SaveChangesAsync();

                            //}
                        }
                        ResponseMessage<List<CaseEncodeGetDto>> assigndCase = await GetAllTransfred(emp.Id);


                        await _encoderHub.Clients.Group(emp.Id.ToString()).getNotification(assigndCase.Data, emp.Id.ToString());

                        //await _encoderHub.Clients.All.getNotification(assigndCase);


                    }
                    catch (Exception ex)
                    {
                        response.Message = $"{ex.Message}";
                        response.Data = null;
                        response.Success = false;
                        response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                        return response;
                    }
                    
                }

                response.Success = true;
                response.Message = "Operation Successfull.";
                response.Data = newCase.Id.ToString();

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }
        }

        public async Task<ResponseMessage<string>> Update(CaseEncodePostDto caseEncodePostDto)
        {

            var response = new ResponseMessage<string>();
            try
            {
                if (caseEncodePostDto.EmployeeId == null && caseEncodePostDto.ApplicantId == null){
                    
                    response.Message = "Please Provide an Applicant ID or Employee ID";
                    response.Data = null;
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();

                    return response;
                }

                var case1 = _dbContext.Cases.Find(caseEncodePostDto.caseID);

                if (case1 == null){
                    
                    response.Message = "Case not found.";
                    response.Data = null;
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();

                    return response;
                }

                case1.ApplicantId = caseEncodePostDto.ApplicantId;
                //EmployeeId = caseEncodePostDto.EmployeeId,
                case1.LetterNumber = caseEncodePostDto.LetterNumber;
                case1.LetterSubject = caseEncodePostDto.LetterSubject;
                case1.CaseTypeId = caseEncodePostDto.CaseTypeId;
                case1.AffairStatus = AffairStatus.Encoded;
                case1.PhoneNumber2 = caseEncodePostDto.PhoneNumber2;
                case1.Representative = caseEncodePostDto.Representative;



                _dbContext.Cases.Update(case1);
                await _dbContext.SaveChangesAsync();



                response.Message = "Operation Successfull";
                response.Data = case1.Id.ToString();
                response.Success = true;
                
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }
        }

        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetAll(Guid userId)
        {

            var response =  new ResponseMessage<List<CaseEncodeGetDto>>();
            try
            {
                List<CaseEncodeGetDto> cases2 = new List<CaseEncodeGetDto>();
                List<CaseEncodeGetDto> cases =
                    await _dbContext.Cases.Where(ca => ca.CreatedBy.Equals(userId) && ca.AffairStatus.Equals(AffairStatus.Encoded))
                    .Include(p => p.Employee)
                    .Include(p => p.CaseType)
                    .Include(p => p.Applicant)

                    .Select(st => new CaseEncodeGetDto
                    {
                        Id = st.Id,
                        CaseNumber = st.CaseNumber,
                        LetterNumber = st.LetterNumber,
                        LetterSubject = st.LetterSubject,
                        CaseTypeName = st.CaseType.CaseTypeTitle,
                        ApplicantName = st.Applicant.ApplicantName,
                        EmployeeName = st.Employee.FullName,
                        ApplicantPhoneNo = st.Applicant.PhoneNumber,
                        EmployeePhoneNo = st.Employee.PhoneNumber,
                        ApplicantId = st.ApplicantId.ToString(),
                        CaseTypeId = st.CaseTypeId.ToString(),
                        Representative = st.Representative,
                        CreatedAt = st.CreatedAt.ToString(),


                    }).OrderByDescending(x => x.CreatedAt).ToListAsync();

                foreach (var item in cases)
                {


                    item.Attachments = await _dbContext.CaseAttachments.Where(x => x.CaseId == item.Id).Select(x => new SelectListDto
                    {
                        Name = x.FilePath,
                        Id = x.Id

                    }).ToListAsync();

                    cases2.Add(item);


                }

                response.Message = "Opertaion Successfull";
                response.Success = true;
                response.Data = cases2;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }
        }
        public async Task<ResponseMessage<CaseEncodeGetDto>> GetSingleCase(Guid caseId)
        {
            var response = new ResponseMessage<CaseEncodeGetDto>();
            try
            {

                CaseEncodeGetDto? case1 =
                    await _dbContext.Cases.Where(ca => ca.Id == caseId)
                    .Include(p => p.Employee)
                    .Include(p => p.CaseType)
                    .Include(p => p.Applicant)

                    .Select(st => new CaseEncodeGetDto
                    {
                        Id = st.Id,
                        CaseNumber = st.CaseNumber,
                        LetterNumber = st.LetterNumber,
                        LetterSubject = st.LetterSubject,
                        CaseTypeName = st.CaseType.CaseTypeTitle,
                        ApplicantName = st.Applicant.ApplicantName,
                        EmployeeName = st.Employee.FullName,
                        ApplicantPhoneNo = st.Applicant.PhoneNumber,
                        EmployeePhoneNo = st.Employee.PhoneNumber,
                        ApplicantId = st.ApplicantId.ToString(),
                        CaseTypeId = st.CaseTypeId.ToString(),
                        Representative = st.Representative,
                        CreatedAt = st.CreatedAt.ToString(),
                        CreatedBy = st.CreatedBy.ToString()


                    }).FirstOrDefaultAsync();

                if (case1 == null){

                    response.Message = "Case not found.";
                    response.Data = null;
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();

                    return response;
                }

                case1.Attachments = await _dbContext.CaseAttachments.Where(x => x.CaseId == case1.Id).Select(x => new SelectListDto
                {
                    Name = x.FilePath,
                    Id = x.Id

                }).ToListAsync();




                
                response.Message = "Operation Successfull.";
                response.Success = true;
                response.Data = case1;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }

        }
        public async Task<ResponseMessage<string>> GetCaseNumber(Guid subOrgId)
        {

            var response = new ResponseMessage<string>();

            var subOrgName = _dbContext.SubsidiaryOrganizations.Where(x => x.Id == subOrgId).Select(c => c.OrganizationNameEnglish).FirstOrDefault();



            string output = string.Concat(subOrgName.Split(' ')
                                      .Where(w => !string.IsNullOrWhiteSpace(w))
                                      .Select(w => char.ToUpper(w[0])));

            var EthYear = EthiopicDateTime.GetEthiopicYear(DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);

            string CaseNumber = $"{output}{EthYear}-";


            var latestNumber = _dbContext.Cases.Where(x => x.SubsidiaryOrganizationId == subOrgId).OrderByDescending(x => x.CreatedAt).Select(c => c.CaseNumber).FirstOrDefault();


            if (latestNumber != null)
            {
                int currCaseNumber = int.Parse(latestNumber.Split('-')[1]);
                CaseNumber += (currCaseNumber + 1).ToString();
            }
            else
            {
                CaseNumber += "1";
            }

            response.Message = "Operation Successfull.";
            response.Data = CaseNumber;
            response.Success = true;

            return response;

        }

        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetAllTransfred(Guid employeeId)


        {
            var response = new ResponseMessage<List<CaseEncodeGetDto>>();

            Employee? user = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == employeeId).FirstOrDefault();
            if (user == null){

                response.Message = "User not found.";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();

                return response;
            }
            List<CaseEncodeGetDto> notfications = new List<CaseEncodeGetDto>();

            if (user.Position == Position.Secertary)
            {
                notfications = await _dbContext.CaseHistories.Include(c
                   => c.Case.CaseType).Include(x => x.Case.Applicant).Where(x => x.ToStructureId == user.OrganizationalStructureId &&
                 (x.AffairHistoryStatus == AffairHistoryStatus.Pend || x.AffairHistoryStatus == AffairHistoryStatus.Transfered) &&
                 (!x.IsConfirmedBySeretery)).Select(x => new CaseEncodeGetDto
                 {
                     Id = x.Id,
                     CaseTypeName = x.Case.CaseType.CaseTypeTitle,
                     CaseNumber = x.Case.CaseNumber,
                     CreatedAt = x.CreatedAt.ToString(),
                     ApplicantName = x.Case.Applicant.ApplicantName,
                     ApplicantPhoneNo = x.Case.Applicant.PhoneNumber,
                     EmployeeName = x.Case.Employee.FullName,
                     EmployeePhoneNo = x.Case.Employee.PhoneNumber,
                     LetterNumber = x.Case.LetterNumber,
                     LetterSubject = x.Case.LetterSubject,
                     FromStructure = x.FromStructure.StructureName,
                     ToEmployee = x.ToEmployee.FullName,
                     ToStructure = x.ToStructure.StructureName,
                     FromEmployeeId = x.FromEmployee.FullName,
                     ReciverType = x.ReciverType.ToString(),
                     SecreateryNeeded = x.SecreateryNeeded,
                     IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                     Position = user.Position.ToString(),
                     AffairHistoryStatus = x.AffairHistoryStatus.ToString(),




                 }).ToListAsync();
            }
            else
            {
                notfications = await _dbContext.CaseHistories.Where(x => x.ToEmployeeId == employeeId && (x.AffairHistoryStatus == AffairHistoryStatus.Pend || x.AffairHistoryStatus == AffairHistoryStatus.Transfered)).Select(x => new CaseEncodeGetDto
                {
                    Id = x.Id,
                    CaseTypeName = x.Case.CaseType.CaseTypeTitle,
                    CaseNumber = x.Case.CaseNumber,
                    CreatedAt = x.CreatedAt.ToString(),
                    ApplicantName = x.Case.Applicant.ApplicantName,
                    ApplicantPhoneNo = x.Case.Applicant.PhoneNumber,
                    EmployeeName = x.Case.Employee.FullName,
                    EmployeePhoneNo = x.Case.Employee.PhoneNumber,
                    LetterNumber = x.Case.LetterNumber,
                    LetterSubject = x.Case.LetterSubject,
                    FromStructure = x.FromStructure.StructureName,
                    FromEmployeeId = x.FromEmployee.FullName,
                    ReciverType = x.ReciverType.ToString(),
                    SecreateryNeeded = x.SecreateryNeeded,
                    IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                    AffairHistoryStatus = x.AffairHistoryStatus.ToString(),
                    ToEmployee = x.ToEmployee.FullName,
                    ToStructure = x.ToStructure.StructureName,
                    Position = user.Position.ToString(),

                }).ToListAsync(); ;
            }
            response.Message = "Operatinonal successfull.";
            response.Data = notfications.OrderByDescending(x => x.CreatedAt).ToList();

            return response;
        }



        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> MyCaseList(Guid employeeId)
        {

            var response = new ResponseMessage<List<CaseEncodeGetDto>>();
            Employee? user = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == employeeId).FirstOrDefault();
            
            if (user == null){

                response.Message = "User not found.";
                response.Data = null;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();

                return response;
                
            }

            if (user.Position == Position.Secertary)
            {
                var HeadEmployees =
                    _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(
                        x =>
                            x.OrganizationalStructureId == user.OrganizationalStructureId &&
                            x.Position == Position.Director).ToList();
                var allAffairHistory = await _dbContext.CaseHistories
                    .Include(x => x.Case)
                    .Include(x => x.FromEmployee)
                    .Include(x => x.FromStructure)
                    .OrderByDescending(x => x.CreatedAt)
                    .Where(x => ((x.ToEmployee.OrganizationalStructureId == user.OrganizationalStructureId &&
                                x.ToEmployee.Position == Position.Director && !x.IsConfirmedBySeretery)
                                || (x.FromEmployee.OrganizationalStructureId == user.OrganizationalStructureId &&
                                x.FromEmployee.Position == Position.Director &&
                                !x.IsForwardedBySeretery &&
                                !x.IsConfirmedBySeretery && x.SecreateryNeeded)
                                ) && x.AffairHistoryStatus != AffairHistoryStatus.Seen).Select(x => new CaseEncodeGetDto
                                {
                                    Id = x.Id,
                                    CaseTypeName = x.Case.CaseType.CaseTypeTitle,
                                    CaseNumber = x.Case.CaseNumber,
                                    CreatedAt = x.Case.CreatedAt.ToString(),
                                    ApplicantName = x.Case.Applicant.ApplicantName,
                                    ApplicantPhoneNo = x.Case.Applicant.PhoneNumber,
                                    EmployeeName = x.Case.Employee.FullName,
                                    EmployeePhoneNo = x.Case.Employee.PhoneNumber,
                                    LetterNumber = x.Case.LetterNumber,
                                    LetterSubject = x.Case.LetterSubject,
                                    Position = user.Position.ToString(),
                                    FromStructure = x.FromStructure.StructureName,
                                    FromEmployeeId = x.FromEmployee.FullName,
                                    ReciverType = x.ReciverType.ToString(),
                                    SecreateryNeeded = x.SecreateryNeeded,
                                    IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                                    ToEmployee = x.ToEmployee.FullName,
                                    ToStructure = x.ToStructure.StructureName,
                                    AffairHistoryStatus = x.AffairHistoryStatus.ToString()
                                }).ToListAsync();


                response.Message = "Operation Successful.";
                response.Data = allAffairHistory;
                response.Success = true;
                return response;
            }
            else
            {
                var allAffairHistory = await _dbContext.CaseHistories
                .Include(x => x.Case)

                .Include(x => x.FromEmployee)
                .Include(x => x.FromStructure)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.AffairHistoryStatus != AffairHistoryStatus.Completed
                            //&& x.AffairHistoryStatus != AffairHistoryStatus.Waiting
                            && x.AffairHistoryStatus != AffairHistoryStatus.Transfered
                            && x.AffairHistoryStatus != AffairHistoryStatus.Revert
                            && x.ToEmployeeId == employeeId).Select(x => new CaseEncodeGetDto
                            {
                                Id = x.Id,
                                CaseTypeName = x.Case.CaseType.CaseTypeTitle,
                                CaseNumber = x.Case.CaseNumber,
                                CreatedAt = x.Case.CreatedAt.ToString(),
                                ApplicantName = x.Case.Applicant.ApplicantName,
                                ApplicantPhoneNo = x.Case.Applicant.PhoneNumber,
                                EmployeeName = x.Case.Employee.FullName,
                                EmployeePhoneNo = x.Case.Employee.PhoneNumber,
                                LetterNumber = x.Case.LetterNumber,
                                LetterSubject = x.Case.LetterSubject,
                                Position = user.Position.ToString(),
                                FromStructure = x.FromStructure.StructureName,
                                FromEmployeeId = x.FromEmployee.FullName,
                                ReciverType = x.ReciverType.ToString(),
                                SecreateryNeeded = x.SecreateryNeeded,
                                IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                                ToEmployee = x.ToEmployee.FullName,
                                ToStructure = x.ToStructure.StructureName,
                                AffairHistoryStatus = x.AffairHistoryStatus.ToString()
                            }).ToListAsync();
                response.Message = "Operation Successfull";
                response.Data = allAffairHistory;
                response.Success = true;

                return response;

            }
            
        }


        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> SearchCases(string filter, Guid subOrgId)
        {

            var response = new ResponseMessage<List<CaseEncodeGetDto>>();


            var allAffairHistory = await _dbContext.CaseHistories
                   .Include(x => x.Case).ThenInclude(x => x.Applicant)
                   .Include(x => x.Case).ThenInclude(x => x.Employee)
                   .Include(x => x.FromEmployee)
                   .Include(x => x.FromStructure)
                   .Include(x => x.ToEmployee)
                   .Include(x => x.ToStructure)
                   .OrderByDescending(x => x.CreatedAt)
                   .Where(x => (x.Case.Applicant.ApplicantName.ToLower().Contains(filter.ToLower()) || x.Case.Applicant.PhoneNumber.Contains(filter) || x.Case.CaseNumber.ToLower().Equals(filter.ToLower())) && x.ReciverType == ReciverType.Orginal && x.Case.SubsidiaryOrganizationId == subOrgId)
                    .Select(x => new CaseEncodeGetDto
                    {
                        Id = x.CaseId,
                        CaseTypeName = x.Case.CaseType.CaseTypeTitle,
                        CaseNumber = x.Case.CaseNumber,
                        CreatedAt = x.Case.CreatedAt.ToString(),
                        ApplicantName = x.Case.Applicant.ApplicantName,
                        ApplicantPhoneNo = x.Case.Applicant.PhoneNumber,
                        EmployeeName = x.Case.Employee.FullName,
                        EmployeePhoneNo = x.Case.Employee.PhoneNumber,
                        LetterNumber = x.Case.LetterNumber,
                        LetterSubject = x.Case.LetterSubject,
                        Position = "",
                        FromStructure = x.FromStructure.StructureName,
                        FromEmployeeId = x.FromEmployee.FullName,
                        ReciverType = x.ReciverType.ToString(),
                        SecreateryNeeded = x.SecreateryNeeded,
                        IsConfirmedBySeretery = x.IsConfirmedBySeretery,
                        ToEmployee = x.ToEmployee.FullName,
                        ToStructure = x.ToStructure.StructureName,
                        AffairHistoryStatus = x.AffairHistoryStatus.ToString(),
                        ChildOrder = x.childOrder
                    }).OrderByDescending(x => x.ChildOrder)
                        .ToListAsync();




            response.Message = "Operational Successfull.";
            response.Data = allAffairHistory;
            response.Success = true;

            return response;

        }


        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> CompletedCases(Guid subOrgId)
        {
            // Employee user = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == employeeId).FirstOrDefault();

            var response = new ResponseMessage<List<CaseEncodeGetDto>>();

            List<CaseEncodeGetDto> cases = await _dbContext.Cases.Where(ca => ca.SubsidiaryOrganizationId == subOrgId && ca.AffairStatus.Equals(AffairStatus.Completed) && !ca.IsArchived).Include(p => p.Employee).Include(p => p.CaseType).Include(p => p.Applicant).Select(st => new CaseEncodeGetDto
            {

                Id = st.Id,
                CaseNumber = st.CaseNumber,
                LetterNumber = st.LetterNumber,
                LetterSubject = st.LetterSubject,
                CaseTypeName = st.CaseType.CaseTypeTitle,
                ApplicantName = st.Applicant.ApplicantName,
                EmployeeName = st.Employee.FullName,
                ApplicantPhoneNo = st.Applicant.PhoneNumber,
                EmployeePhoneNo = st.Employee.PhoneNumber,
                CreatedAt = st.CreatedAt.ToString(),
                AffairHistoryStatus = st.AffairStatus.ToString()

            }).ToListAsync();

            response.Message = "Operation Successfull.";
            response.Data = cases;
            response.Success = true;

            return response;

        }


        public async Task<ResponseMessage<List<CaseEncodeGetDto>>> GetArchivedCases(Guid subOrgId)
        {

            var response = new ResponseMessage<List<CaseEncodeGetDto>>();

            List<CaseEncodeGetDto> cases = await _dbContext.Cases.Where(ca => ca.SubsidiaryOrganizationId == subOrgId && ca.IsArchived).Include(p => p.Employee).Include(p => p.CaseType).Include(p => p.Applicant).Include(x => x.Folder.Row.Shelf).Include(x => x.CaseAttachments).Select(st => new CaseEncodeGetDto
            {

                Id = st.Id,
                CaseNumber = st.CaseNumber,
                LetterNumber = st.LetterNumber,
                LetterSubject = st.LetterSubject,
                CaseTypeName = st.CaseType.CaseTypeTitle,
                ApplicantName = st.Applicant.ApplicantName,
                EmployeeName = st.Employee.FullName,
                ApplicantPhoneNo = st.Applicant.PhoneNumber,
                EmployeePhoneNo = st.Employee.PhoneNumber,
                CreatedAt = st.CreatedAt.ToString(),
                AffairHistoryStatus = st.AffairStatus.ToString(),
                FolderName = st.Folder.FolderName,
                RowNumber = st.Folder.Row.RowNumber,
                ShelfNumber = st.Folder.Row.Shelf.ShelfNumber,
                Attachments = st.CaseAttachments.Select(x => new SelectListDto
                {
                    Id = x.Id,
                    Name = x.FilePath
                }).ToList()

            }).ToListAsync();

            response.Message = "OPeration Successfull.";
            response.Data = cases;
            response.Success = true;

            return response;



        }


    }



}




