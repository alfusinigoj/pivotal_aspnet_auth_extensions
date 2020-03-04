using Microsoft.AspNetCore.Authentication;

namespace PivotalServices.AspNet.Auth.Extensions.Authentication
{
    public interface ITicketIssuer
    {
        AuthenticationTicket Authenticate(string base64Token);
    }
}
