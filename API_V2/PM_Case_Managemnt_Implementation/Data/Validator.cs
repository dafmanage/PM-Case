using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using System.Text.Json;

namespace PM_Case_Managemnt_Implementation.Data;

public class ValidateModelAttribute : ActionFilterAttribute
{
    private readonly ILoggerManagerService _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ValidateModelAttribute(ILoggerManagerService logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
            _logger.LogException("ValidateModel", userId, $"Model validation failed: {JsonSerializer.Serialize(errors)}");

            throw new InvalidModelStateException(errors);
        }
    }
}
