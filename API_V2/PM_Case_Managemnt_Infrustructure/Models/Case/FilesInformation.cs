using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    public class FilesInformation : CommonModel
    {

        public string? FilePath { get; set; }
        // public Filelookup FileLookup { get; set; }        
        //  public Guid ParentId { get; set; }       
        public string? FileDescription { get; set; }
        public Guid FileSettingId { get; set; }
        public virtual FileSetting FileSetting { get; set; } = null!;

        public Guid CaseId { get; set; }
        public virtual Case Case { get; set; }
        public string filetype { get; set; }
    }

    public enum Filelookup
    {
        Case
    }


}