using System.Net;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.FileInformationService
{
    public class FilesInformationService : IFilesInformationService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggerManagerService _logger;

        public FilesInformationService(ApplicationDbContext dbContext, ILoggerManagerService logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<ResponseMessage<int>> AddMany(List<FilesInformation> fileInformations)
        {

            var response = new ResponseMessage<int>();
            try
            {
                await _dbContext.FilesInformations.AddRangeAsync(fileInformations);
                await _dbContext.SaveChangesAsync();
                
                response.Message = "operation Successfull.";
                response.Success = true;
                response.Data = 1;
                //didnt know who to put as a creator
                //_logger.LogCreate("FilesInformationService", fileInformations.ToString(), "Applicant added Successfully");
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"{ex.Message}";
                response.Success = false;
                response.Data = 0;
                response.ErrorCode = HttpStatusCode.InternalServerError.ToString();

                return response;
            }
        }
        //public async Task<List<FilesInformation>> 
    }
}
