using System.Security.Claims;
using Common.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace Common.Web.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User is unauthorized");

    public bool IsAuthenticated => UserId != null;
}