using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CashRegister.Web.Authorization;

public class BranchAccessHandler : AuthorizationHandler<BranchAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BranchAccessRequirement requirement)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == "Viewer")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userBranchId = context.User.FindFirst("BranchId")?.Value;

        if (userBranchId != null && int.TryParse(userBranchId, out int branchId))
        {
            if (branchId == requirement.BranchId)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
