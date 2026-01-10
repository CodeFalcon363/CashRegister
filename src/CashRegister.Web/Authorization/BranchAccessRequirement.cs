using Microsoft.AspNetCore.Authorization;

namespace CashRegister.Web.Authorization;

public class BranchAccessRequirement : IAuthorizationRequirement
{
    public int BranchId { get; }

    public BranchAccessRequirement(int branchId)
    {
        BranchId = branchId;
    }
}
