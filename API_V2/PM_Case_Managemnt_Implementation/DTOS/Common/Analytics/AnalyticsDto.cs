namespace PM_Case_Managemnt_Implementation.DTOS.Common.Analytics
{
    public record OverallBudgetDto
    {
        public string? SubOrganiztionName { get; set; }
        public float? SubOrganizationBudget { get; set; }

    }

    public record SubOrgsPlannedandusedBudgetDtos
    {
        public List<OverallBudgetDto> PlannedBudget { get; set; }
        public List<OverallBudgetDto> Usedbudget { get; set; }
    }

    public record OverallPerformanceDto
    {
        public string? SubOrganiztionName { get; set; }
        public float? SubOrganizationPerformace { get; set; }

    }






}
