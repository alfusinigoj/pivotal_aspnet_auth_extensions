﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace PivotalServices.AspNet.Auth.Extensions.Authentication
{
    internal class TestTicketIssuer : ITicketIssuer
    {
        public AuthenticationTicket Authenticate(string base64Token)
        {
            return new AuthenticationTicket(
                            new ClaimsPrincipal(
                                new ClaimsIdentity(new[]
                                {
                                        new Claim(ClaimTypes.Name,"Test User"),
                                }, AuthConstants.SPNEGO_DEFAULT_SCHEME)),
                            AuthConstants.SPNEGO_DEFAULT_SCHEME);
        }
    }
}
