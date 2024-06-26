namespace PM_Case_Managemnt_Implementation.DTOS.KPI
{
    public class KPIPostDto
    {
        public string Title { get; set; }
        public int StartYear { get; set; }
        public string ActiveYearsString { get; set; }
        public string? EncoderOrganizationName { get; set; }
        public string? EvaluatorOrganizationName { get; set; }
        public string? AccessCode { get; set; }
        public bool HasSubsidiaryOrganization { get; set; }
        public Guid? SubsidiaryOrganizationId { get; set; }
        public string? Url { get; set; }
        public Guid CreatedBy { get; set; }


    }

    public class KPIGetDto : KPIPostDto
    {
        public Guid Id { get; set; }
        public List<int> ActiveYears { get; set; }
        public List<GroupedKPIDetailsGetDto>? KpiDetails { get; set; }
    }
}
