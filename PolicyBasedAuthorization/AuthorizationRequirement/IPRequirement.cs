using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace PolicyBasedAuthorization.AuthorizationRequirement
{
    public class IPRequirement : IAuthorizationRequirement
    {
        public List<string> Whitelist { get; }
        public IPRequirement(ApplicationOptions applicationOptions)
        {
            Whitelist = applicationOptions.Whitelist;
        }
    }
}
