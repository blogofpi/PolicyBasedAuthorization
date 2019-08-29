
using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuthorization.AuthorizationRequirement;
using System.Threading.Tasks;

namespace PolicyBasedAuthorization.Handlers
{
    public class EmailDomainHandler : AuthorizationHandler<InternalUserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InternalUserRequirement requirement)
        {
            var email = context.User.Identity.Name;
            var domain = email.Split('@')[1];
            if (domain.Equals("blogofpi.com"))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
