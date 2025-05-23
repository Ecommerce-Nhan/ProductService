using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace ProductService.Api.Authorize;
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IDistributedCache _redisCache;

    public PermissionAuthorizationHandler(IDistributedCache redisCache)
    {
        _redisCache = redisCache;
    }
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var listPermissions = new List<string>
        {
            "Permissions.Products.View",
            "Permissions.Products.Create",
            "Permissions.Products.Edit",
            "Permissions.Products.Delete"
        };
        var json = JsonSerializer.Serialize(listPermissions);
        await _redisCache.SetStringAsync($"role_permissions:Admin", json);

        if (context.User == null)
        {
            return;
        }
        var userRoles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
        if (!userRoles.Any())
        {
            return;
        }
        foreach (var role in userRoles)
        {
            var permissionKey = $"role_permissions:{role}";
            var permissionsJson = await _redisCache.GetStringAsync(permissionKey);

            if (!string.IsNullOrEmpty(permissionsJson))
            {
                var permissions = JsonSerializer.Deserialize<HashSet<string>>(permissionsJson);
                if (permissions != null && permissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}