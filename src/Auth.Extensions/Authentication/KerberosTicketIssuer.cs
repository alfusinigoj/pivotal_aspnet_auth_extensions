using Kerberos.NET;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;

namespace PivotalServices.AspNet.Auth.Extensions.Authentication
{
    public class KerberosTicketIssuer : ITicketIssuer
    {
        private readonly IConfiguration configuration;
        private readonly KerberosAuthenticator authenticator;

        public KerberosTicketIssuer(KerberosAuthenticator authenticator, IConfiguration configuration)
        {
            this.authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public AuthenticationTicket Authenticate(string base64Token)
        {
            if (string.IsNullOrWhiteSpace(configuration[AuthConstants.PRINCIPAL_PASSWORD_NM]))
                throw new ArgumentNullException($"{AuthConstants.PRINCIPAL_PASSWORD_NM} is not set! Set as an environment variable or via any configuration sources");

            var identity = authenticator.Authenticate(base64Token).Result;

            return new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                AuthConstants.SPNEGO_DEFAULT_SCHEME);
        }
    }
}
