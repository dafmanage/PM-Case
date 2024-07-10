namespace PM_Case_Managemnt_Implementation.DTOS.CaseDto; 
    public class CaseState
    {
        public string CurrentState { get; set; }
        public string NextState { get; set; }
        public List<string> NeededDocuments { get; set; }
    }