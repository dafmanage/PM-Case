using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using PM_Case_Managemnt_Implementation.DTOS.Common;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Implementation.Services.Common
{
    public class OrgStructureService : IOrgStructureService
    {
        private readonly ApplicationDbContext _dBContext;
        private readonly ILoggerManagerService _logger;
        public OrgStructureService(ApplicationDbContext context, ILoggerManagerService logger)
        {
            _dBContext = context;
            _logger = logger;
        }

        public async Task<ResponseMessage<int>> CreateOrganizationalStructure(OrgStructureDto orgStructure)
        {


            //var orgainzationProfile = _dBContext.OrganizationProfile.FirstOrDefault();
            //var id = Guid.NewGuid();
            var response = new ResponseMessage<int>();
            
            if (orgStructure.Id == Guid.Empty || orgStructure.Id == null)
            {
                orgStructure.Id = Guid.NewGuid();
            }
            if (orgStructure.OrganizationBranchId == Guid.Empty || orgStructure.OrganizationBranchId == null)
            {
                orgStructure.OrganizationBranchId = (Guid)orgStructure.Id;
            }
            
            var orgStructure2 = new OrganizationalStructure
            {
                Id = (Guid)orgStructure.Id,
                OrganizationBranchId = orgStructure.OrganizationBranchId,
                //OrganizationProfileId = orgainzationProfile.Id,
                SubsidiaryOrganizationId = orgStructure.SubsidiaryOrganizationId,
                ParentStructureId = orgStructure.ParentStructureId,
                StructureName = orgStructure.StructureName,
                Order = orgStructure.Order,
                IsBranch = orgStructure.IsBranch,
                OfficeNumber = orgStructure.OfficeNumber,
                Weight = orgStructure.Weight,
                Remark = orgStructure.Remark,
                CreatedAt = DateTime.Now,


            };


            await _dBContext.AddAsync(orgStructure2);
            await _dBContext.SaveChangesAsync();

            response.Message = "Operation Successful.";
            response.Data = 1;
            response.Success = true;
            _logger.LogCreate("OrgStructureService", orgStructure.Id.ToString(), "Organizational structure created Successfully");
            return response;

        }
        public async Task<ResponseMessage<List<OrgStructureDto>>> GetOrganizationStructures(Guid SubOrgId, Guid? BranchId)
        {

            var response = new ResponseMessage<List<OrgStructureDto>>();
            
            List<OrgStructureDto> structures = await (from x in _dBContext.OrganizationalStructures.Include(x => x.ParentStructure).Where(x=>x.SubsidiaryOrganizationId == SubOrgId && x.OrganizationBranchId == BranchId)
                                                      
                                                      select new OrgStructureDto
                                                      {
                                                          Id = x.Id,
                                                          OrganizationBranchId = x.OrganizationBranchId,
                                                          SubsidiaryOrganizationId = x.SubsidiaryOrganizationId,
                                                          ParentStructureName = x.ParentStructure.StructureName,
                                                          ParentStructureId = x.ParentStructure.Id,
                                                          StructureName = x.StructureName,
                                                          Order = x.Order,
                                                          Weight = x.Weight,
                                                          IsBranch = x.IsBranch,
                                                          OfficeNumber = x.OfficeNumber,
                                                          ParentWeight = x.ParentStructure.Weight,
                                                          Remark = x.Remark

                                                      }).ToListAsync();
            foreach(var structure in structures)
            {

                var orgBranch = await _dBContext.OrganizationalStructures.FindAsync(structure.OrganizationBranchId);

                if (orgBranch == null)
                {
                    
                    response.Message = "Branch Not found.";
                    response.Data = null;
                    response.Success = false;
                    response.ErrorCode = HttpStatusCode.NotFound.ToString();

                    return response;
                }
                
                structure.BranchName = orgBranch.StructureName;
            }


            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = structures;

            return response;
        }

        public async Task<ResponseMessage<List<SelectListDto>>> getParentStrucctureSelectList(Guid branchId)
        {

            var response = new ResponseMessage<List<SelectListDto>>();
            
            List<SelectListDto> list = await (from x in _dBContext.OrganizationalStructures.Where(y => y.OrganizationBranchId == branchId  && (!y.IsBranch || y.Id == branchId))
                select new SelectListDto
                {
                    Id = x.Id,
                    Name = x.StructureName + (x.IsBranch ? "( Branch )" : "")

                }).ToListAsync();


            if (!list.Any())
            {
                list =  await (from x in _dBContext.OrganizationalStructures.Where(y => y.Id == branchId)
                    select new SelectListDto
                    {
                        Id = x.Id,
                        Name = x.StructureName

                    }).ToListAsync();
            }

            response.Message = "Operation Successful.";
            response.Data = list;
            response.Success = true;

            return response;
        }



        public async Task<ResponseMessage<int>> UpdateOrganizationalStructure(OrgStructureDto orgStructure)
        {
            var response = new ResponseMessage<int>();

            var orgStructure2 = _dBContext.OrganizationalStructures.Find(orgStructure.Id);

            if (orgStructure2 == null)
            {
                
                response.Message = "Structure Not found.";
                response.Data = -1;
                response.Success = false;
                response.ErrorCode = HttpStatusCode.NotFound.ToString();

                return response;
            }
            orgStructure2.OrganizationBranchId = orgStructure.OrganizationBranchId;
            orgStructure2.ParentStructureId = orgStructure.ParentStructureId;
            orgStructure2.StructureName = orgStructure.StructureName;
            orgStructure2.Order = orgStructure.Order;
            orgStructure2.Weight = orgStructure.Weight;
            orgStructure2.IsBranch = orgStructure.IsBranch;
            orgStructure2.OfficeNumber = orgStructure.OfficeNumber;
            orgStructure2.Remark = orgStructure.Remark;
            orgStructure2.RowStatus = orgStructure.RowStatus == 0 ? RowStatus.Active : RowStatus.InActive;
            //orgStructure2.SubsidiaryOrganizationId= orgStructure.SubsidiaryOrganizationId;

            _dBContext.Entry(orgStructure2).State = EntityState.Modified;
            await _dBContext.SaveChangesAsync();

            response.Message = "Operation Successful.";
            response.Success = true;
            response.Data = 1;
            _logger.LogUpdate("OrgStructureService", orgStructure.Id.ToString(), "Organizational structure updated Successfully");
            return response;

        }


        public async Task<ResponseMessage<List<DiagramDto>>> getDIagram(Guid? BranchId)
        {
            var response = new ResponseMessage<List<DiagramDto>>();

            var orgStructures = _dBContext.OrganizationalStructures.Include(x => x.ParentStructure).Where(x=>x.OrganizationBranchId==BranchId)
                                                 .ToList();//Where(x=>x.ParentStructureId==BranchId)
            var employess = _dBContext.Employees.ToList();
            var childs = new List<DiagramDto>();

            var parentStructure = _dBContext.OrganizationalStructures.Include(x => x.ParentStructure).FirstOrDefault(x => x.Id == BranchId);

            var DiagramDro = new DiagramDto()
            {
                data = new
                {
                    name = parentStructure.StructureName,
                    weight = "  ( " + Decimal.Round((decimal)(parentStructure.Weight), 2) + "% ",
                    head = employess.FirstOrDefault(x => x.OrganizationalStructureId == parentStructure.Id && x.Position == Position.Director)?.Title + " " +
                                       employess.FirstOrDefault(x => x.OrganizationalStructureId == parentStructure.Id && x.Position == Position.Director)?.FullName

                },
                label = parentStructure.StructureName,
                expanded = true,
                type = "organization",
                styleClass = "bg-success text-white",
                id = parentStructure.Id,
                order = parentStructure.Order,
                children = new List<DiagramDto>()
          
            };

            childs.Add(DiagramDro);

            var remainingStractures = orgStructures.Where(x => x.ParentStructureId != null).OrderBy(x => x.Order).Select(x => x.ParentStructureId).Distinct();

            foreach (var items in remainingStractures)
            {
                var children = orgStructures.Where(x => x.ParentStructureId == items).Select(x => new DiagramDto
                {
                    data = new
                    {
                        name = x.StructureName,
                        weight = "  ( " + Decimal.Round((decimal)((x.Weight / x.ParentStructure.Weight) * 100), 2) + "% of " + Decimal.Round((decimal)x.ParentStructure.Weight, 2) + " ) ",
                        head = employess.FirstOrDefault(x => x.OrganizationalStructureId == x.Id && x.Position == Position.Director)?.Title + " " +
                                       employess.FirstOrDefault(x => x.OrganizationalStructureId == x.Id && x.Position == Position.Director)?.FullName

                    },

                    label = x.StructureName,
                    expanded = true,
                    type = "organization",
                    styleClass = x.Order % 2 == 1 ? "bg-secondary text-white" : "bg-success text-white",
                    id = x.Id,
                    parentId = x.ParentStructureId,
                    order = x.Order,
                    children = new List<DiagramDto>()
                }).ToList();


                childs.AddRange(children);


            }
            for (var j = childs.Max(x => x.order); j >= 0; j--)
            {
                var childList = childs.Where(x => x.order == j).ToList();
                foreach (var item in childList)
                {

                    var org = childs.FirstOrDefault(x => x.id == item.parentId);

                    if (org != null)
                    {
                        org.children.Add(item);
                    }


                }
            }
            List<DiagramDto> result = new List<DiagramDto>();

            if (childs.Any())
            {
                result.Add(childs[0]);
            }

            response.Message = "Operation Successful.";
            response.Data = result;
            response.Success = true;
            
            return response;           

        }



    }
}
