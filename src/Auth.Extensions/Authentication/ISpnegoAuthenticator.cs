using Microsoft.AspNetCore.Authentication;
using System.Web;

namespace PivotalServices.AspNet.Auth.Extensions.Authentication
{
    public interface ISpnegoAuthenticator
    {
        AuthenticateResult Authenticate(HttpContextBase contextBase);
        void Challenge(AuthenticationProperties properties, HttpContextBase contextBase);
    }
}