namespace PM_Case_Managemnt_Implementation.Helpers.Logger
{
    public interface ILoggerManagerService
    {
        void LogCreate(string module, string createdById, string message);
        void LogException(string module, string createdById, string message);
        void LogUpdate(string module, string createdById, string message);
    }
}
