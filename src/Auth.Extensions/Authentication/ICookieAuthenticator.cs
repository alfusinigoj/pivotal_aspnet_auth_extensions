using Microsoft.AspNetCore.Authentication;
using System.Web;

namespace PivotalServices.AspNet.Auth.Extensions.Authentication
{
    public interface ICookieAuthenticator
    {
        AuthenticateResult Authenticate(HttpContextBase contextBase);
        void SignIn(AuthenticateResult authResult, HttpContextBase contextBase);
    }
}