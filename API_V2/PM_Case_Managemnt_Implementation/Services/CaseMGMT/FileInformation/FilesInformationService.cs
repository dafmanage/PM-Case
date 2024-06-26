using PM_Case_Managemnt_Infrustructure.Data;
using PM_Case_Managemnt_Infrustructure.Models.CaseModel;

namespace PM_Case_Managemnt_Implementation.Services.CaseMGMT.FileInformationService
{
    public class FilesInformationService : IFilesInformationService
    {

        private readonly DBContext _dbContext;

        public FilesInformationService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddMany(List<FilesInformation> fileInformations)
        {
            try
            {
                await _dbContext.FilesInformations.AddRangeAsync(fileInformations);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //public async Task<List<FilesInformation>> 
    }
}
