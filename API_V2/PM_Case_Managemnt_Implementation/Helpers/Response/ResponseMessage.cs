namespace PM_Case_Managemnt_Implementation.Helpers.Response;

public class ResponseMessage<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string ?ErrorCode { get; set; }
    public T ?Data { get; set;}
}