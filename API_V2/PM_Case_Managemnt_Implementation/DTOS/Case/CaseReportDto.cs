using static PM_Case_Managemnt_Implementation.Services.Common.Dashoboard.DashboardService;

namespace PM_Case_Managemnt_Implementation.DTOS.Case
{
    public class CaseReportDto
    {
        public Guid Id { get; set; }
        public string CaseNumber { get; set; }
        public string CaseType { get; set; }
        public string Subject { get; set; }
        public string IsArchived { get; set; }
        public string OnStructure { get; set; }
        public string OnEmployee { get; set; }
        public string CaseStatus { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public float CaseCounter { get; set; }
        public double ElapsTime { get; set; }
    }

    public class CaseReportChartDto
    {

        public List<string> labels { get; set; }
        public List<DataSets> datasets { get; set; }

    }
    public class DataSets
    {

        public List<int> data { get; set; }
        public List<string> backgroundColor { get; set; }
        public List<string> hoverBackgroundColor { get; set; }


    }
    public class AffairAnalysis
    {
        public string AffairTypeTitle { get; set; }
        public string Remark { get; set; }
        public float Counter { get; set; }
        public float MeanTime { get; set; }
    }


    public class EmployeePerformance
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; }
        public string Image { get; set; }
        public string EmployeeStructure { get; set; }
        public float WorkeDonePercent { get; set; }
        public double ActualTimeTaken { get; set; }
        public double ExpectedTime { get; set; }
        public string PerformanceStatus { get; set; }

    }

    public class SMSReportDto
    {
        public string CaseNumber { get; set; }
        public string ApplicantName { get; set; }
        public string LetterNumber { get; set; }
        public string Subject { get; set; }
        public string CaseTypeTitle { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumber2 { get; set; }
        public string Message { get; set; }
        public string MessageGroup { get; set; }

        public bool IsSMSSent { get; set; }

        public DateTime CreatedAt { get; set; }



    }


    public class CaseDetailReportDto
    {

        public Guid Id { get; set; }
        public string CaseNumber { get; set; }
        public string ApplicantName { get; set; }

        public string LetterNumber { get; set; }

        public string Subject { get; set; }

        public string CaseTypeTitle { get; set; }

        public string CaseTypeStatus { get; set; }

        public string PhoneNumber { get; set; }

        public string Createdat { get; set; }
        public float CaseCounter { get; set; }

    }



    public class CaseProgressReportDto
    {
        public string CaseTypeTitle { get; set; }
        public string ApplicationDate { get; set; }
        public string ApplicantName { get; set; }
        public string CaseNumber { get; set; }
        public string LetterNumber { get; set; }
        public string LetterSubject { get; set; }

        public List<CaseProgressReportHistoryDto> HistoryProgress { get; set; }



    }

    public class CaseProgressReportHistoryDto
    {
        public string FromEmployee { get; set; }
        public string ToEmployee { get; set; }
        public string CreatedDate { get; set; }
        public string Seenat { get; set; }
        public string Status { get; set; }
        public string StatusDateTime { get; set; }
        public string ShouldTake { get; set; }
        public string ElapsedTime { get; set; }
        public string ElapseTimeBasedOnSeenTime { get; set; }
        public string EmployeeStatus { get; set; }

    }

    public class TopAffairsViewmodel
    {
        public Guid Id { get; set; }
        public string CaseTypeTitle { get; set; }
        public string ApplicantName { get; set; }
        public string AffairNumber { get; set; }
        public string Subject { get; set; }
        public string Structure { get; set; }
        public string Employee { get; set; }
        public double Elapstime { get; set; }
        public double Level { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }

    public class DashboardDto
    {
        public List<TopAffairsViewmodel> pendingReports { get; set; }
        public List<TopAffairsViewmodel> completedReports { get; set; }

        public CaseReportChartDto chart { get; set; }
    }


    public class barChartDto
    {

        public List<string> labels { get; set; }
        public List<barChartDetailDto> datasets { get; set; }
    }

    public class barChartDetailDto
    {

        public string type { get; set; }
        public string label { get; set; }
        public string backgroundColor { get; set; }

        public List<int> data { get; set; }
    }

    public class PMDashboardDto
    {

        public int CountPrograms { get; set; }

        public float CountBudget { get; set; }

        public float? CountUsedBudget { get; set; }

        public int TotalProjects { get; set; }

        public float TotalContribution { get; set; }

        public int BudgetYear { get; set; }

        public List<ProjectList> ProjectLists { get; set; }
        public List<AboutToExpireProjects> AboutToExpireProjects { get; set; }


    }




    public enum PerformanceStatus
    {
        OverPlan,
        OnPlan,
        UnderPlan
    }




}
