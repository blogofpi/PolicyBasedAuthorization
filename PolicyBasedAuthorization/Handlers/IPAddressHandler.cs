
using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuthorization.AuthorizationRequirement;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PolicyBasedAuthorization.Handlers
{
    public class IPAddressHandler : AuthorizationHandler<IPRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IPRequirement requirement)
        {
            var authFilterCtx = (Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)context.Resource;
            var httpContext = authFilterCtx.HttpContext;
            var ipAddress = httpContext.Connection.RemoteIpAddress;

            List<string> whiteListIPList = requirement.Whitelist;
            var isInwhiteListIPList = whiteListIPList
                .Where(a => IPAddress.Parse(a)
                .Equals(ipAddress))
                .Any();
            if (isInwhiteListIPList)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
