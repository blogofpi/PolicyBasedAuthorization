
using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuthorization.AuthorizationRequirement;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolicyBasedAuthorization.Handlers
{
    public class ExcludeContractorHandler : AuthorizationHandler<InternalUserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InternalUserRequirement requirement)
        {
            var roles = ((ClaimsIdentity)context.User.Identity).Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            if (!roles.Contains("Contractor"))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
