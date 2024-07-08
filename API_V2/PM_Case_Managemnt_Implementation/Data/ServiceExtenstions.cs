using Microsoft.Extensions.DependencyInjection;
using PM_Case_Managemnt_Implementation.Helpers;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Services.Auth;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.Applicants;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentService;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.AppointmentWithCalenderService;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseAttachments;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseForwardService;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.CaseMessagesService;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.FileInformationService;
using PM_Case_Managemnt_Implementation.Services.CaseMGMT.History;
using PM_Case_Managemnt_Implementation.Services.CaseService.CaseTypes;
using PM_Case_Managemnt_Implementation.Services.CaseService.Encode;
using PM_Case_Managemnt_Implementation.Services.CaseService.FileSettings;
using PM_Case_Managemnt_Implementation.Services.Common;
using PM_Case_Managemnt_Implementation.Services.Common.Analytics;
using PM_Case_Managemnt_Implementation.Services.Common.Dashoboard;
using PM_Case_Managemnt_Implementation.Services.Common.FolderService;
using PM_Case_Managemnt_Implementation.Services.Common.RowService;
using PM_Case_Managemnt_Implementation.Services.Common.ShelfService;
using PM_Case_Managemnt_Implementation.Services.Common.SmsTemplate;
using PM_Case_Managemnt_Implementation.Services.Common.SubOrganization;
using PM_Case_Managemnt_Implementation.Services.Common.SubsidiaryOrganization;
using PM_Case_Managemnt_Implementation.Services.KPI;
using PM_Case_Managemnt_Implementation.Services.PM;
using PM_Case_Managemnt_Implementation.Services.PM.Activityy;
using PM_Case_Managemnt_Implementation.Services.PM.Commite;
using PM_Case_Managemnt_Implementation.Services.PM.Plann;
using PM_Case_Managemnt_Implementation.Services.PM.Program;
using PM_Case_Managemnt_Implementation.Services.PM.ProgresReport;

namespace PM_Case_Managemnt_Implementation.Data
{
    public static class ServiceExtenstions
    {
        public static IServiceCollection AddCoreBusiness(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManagerService, LoggerManagerService>();
            services.AddScoped<IOrganizationProfileService, OrganzationProfileService>();
            services.AddScoped<IOrgBranchService, OrgBranchService>();
            services.AddScoped<IOrgStructureService, OrgStructureService>();
            services.AddScoped<IBudgetyearService, BudgetYearService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IUnitOfMeasurmentService, UnitOfMeasurmentService>();

            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IRowService, RowService>();
            services.AddScoped<IShelfService, ShelfService>();

            services.AddScoped<IProgramService, ProgramService>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ICommiteService, CommiteService>();
            services.AddScoped<IActivityService, ActivityService>();

            services.AddScoped<ICaseTypeService, CaseTypeService>();
            services.AddScoped<IFileSettingsService, FileSettingService>();
            services.AddScoped<ICaseEncodeService, CaseEncodeService>();
            services.AddScoped<ICaseAttachementService, CaseAttachementService>();
            services.AddScoped<IApplicantService, ApplicantService>();
            services.AddScoped<ICaseHistoryService, CaseHistoryService>();
            services.AddScoped<ICaseForwardService, CaseForwardService>();
            services.AddScoped<ICaseMessagesService, CaseMessagesService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IAppointmentWithCalenderService, AppointmentWithCalenderService>();
            services.AddScoped<IFilesInformationService, FilesInformationService>();
            services.AddScoped<ICaseProccessingService, CaseProccessingService>();
            services.AddScoped<ICaseReportService, CaserReportService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ICaseIssueService, CaseIssueService>();
            services.AddScoped<ISMSHelper, SMSHelper>();
            services.AddScoped<IProgressReportService, ProgressReportService>();
            services.AddScoped<ISubsidiaryOrganizationService, SubsidiaryOrganizationService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<ISmsTemplateService, SmsTemplateService>();
            services.AddScoped<IKPIService, KPIService>();

            return services;
        }
    }
}

