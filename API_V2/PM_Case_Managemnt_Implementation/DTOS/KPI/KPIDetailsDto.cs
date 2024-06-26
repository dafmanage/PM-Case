namespace PM_Case_Managemnt_Implementation.DTOS.KPI
{
    public class KPIDetailsPostDto
    {
        public Guid KPIId { get; set; }
        public List<SimilarGoals> Titles { get; set; }
        public string Goal { get; set; }
        public Guid CreatedBy { get; set; }
    }
    public class KPIGoalPostDto
    {
        public Guid KPIId { get; set; }
        public string Goal { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class SimilarGoals
    {
        //public string Goal { get; set; }
        public string Title { get; set; }
        public float StartYearProgress { get; set; }
    }

    public class KPIDetailsGetDto
    {
        public Guid Id { get; set; }
        public Guid? KPIId { get; set; }
        public string Title { get; set; }
        public string MainGoal { get; set; }
        public float StartYearProgress { get; set; }
        public Guid? CreatedBy { get; set; }
        public List<KPIDataGetDto>? KPIDatas { get; set; }
    }
    public class GroupedKPIDetailsGetDto
    {
        public string MainGoal { get; set; }
        public List<KPIDetailsGetDto> Details { get; set; }
    }
}
