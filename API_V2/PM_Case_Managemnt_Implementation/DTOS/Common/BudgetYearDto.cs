namespace PM_Case_Managemnt_Implementation.DTOS.Common
{
    public class BudgetYearDto
    {
        public Guid? Id { get; set; }
        public int Year { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public Guid ProgramBudgetYearId { get; set; }

        public string Remark { get; set; }

        public Guid CreatedBy { get; set; }
    }

    public class ProgramBudgetYearDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = null!;

        public int FromYear { get; set; }

        public int ToYear { get; set; }
        public Guid SubsidiaryOrganizationId { get; set; }
        public string? Remark { get; set; }
        public Guid CreatedBy { get; set; }

    }
}
