using PM_Case_Managemnt_Infrustructure.Models.Common;

namespace PM_Case_Managemnt_Infrustructure.Models.CaseModel
{
    public class CaseMessages : CommonModel
    {
        public Guid? CaseId { get; set; }
        public virtual Case Case { get; set; } = null!;
        public MessageFrom MessageFrom { get; set; }
        public string MessageBody { get; set; } = null!;
        public bool Messagestatus { get; set; }
    }
    public enum MessageFrom
    {

        Appointment,
        Transfer,
        Revert,
        Complete,
        Assigned,
        Custom_text

    }
}
