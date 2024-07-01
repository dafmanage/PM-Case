namespace PM_Case_Managemnt_Implementation.Helpers
{
    public class ResponseMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ?ErrorCode { get; set; }
        public T ?Data { get; set;}
    }
}
