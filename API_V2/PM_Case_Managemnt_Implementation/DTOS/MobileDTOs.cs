namespace PM_Case_Managemnt_Implementation.DTOS
{
    public class MobileDTOs
    {



    }

    public class ActiveAffairsViewModel
    {
        public Guid historyId { get; set; }
        public string applicant { get; set; }
        public Guid affairId { get; set; }
        public string affairNumber { get; set; }
        public string affairType { get; set; }
        public string subject { get; set; }
        public string fromEmplyee { get; set; }
        public string fromStructure { get; set; }
        public string remark { get; set; }
        public string reciverType { get; set; }
        public string affairHistoryStatus { get; set; }
        public string createdAt { get; set; }
        public List<string> document { get; set; }
        public string confirmedSecratary { get; set; }





    }
}
