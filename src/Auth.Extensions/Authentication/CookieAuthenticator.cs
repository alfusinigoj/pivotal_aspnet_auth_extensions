﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using PivotalServices.AspNet.Auth.Extensions.DataProtection;
using System;
using System.Web;

namespace PivotalServices.AspNet.Auth.Extensions.Authentication
{
    public class CookieAuthenticator : ICookieAuthenticator
    {
        private readonly IDataProtector dataProtector;
        private readonly ILogger<CookieAuthenticator> logger;
        private readonly TicketSerializer serializer;
        public const int COOKIE_TIMEOUT_IN_MINUTES = 20;

        public CookieAuthenticator(IDataProtector dataProtector, ILogger<CookieAuthenticator> logger)
        {
            this.dataProtector = dataProtector ?? throw new ArgumentNullException(nameof(dataProtector));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            serializer = new TicketSerializer();
        }

        public AuthenticateResult Authenticate(HttpContextBase contextBase)
        {
            if (!contextBase.Request.Browser.Cookies)
            {
                logger.LogWarning("This browser doesnot support cookies, so cookie based authentication is disabled");
                return AuthenticateResult.NoResult();
            }

            var authCookie = contextBase.Request.Cookies.Get(AuthConstants.AUTH_COOKIE_NM);

            if (authCookie != null)
            {
                try
                {
                    var unprotectedCookieBytes = dataProtector.UnProtect(Convert.FromBase64String(authCookie.Value));
                    var ticket = serializer.Deserialize(unprotectedCookieBytes);
                    logger.LogDebug("Cookie authentication succeeded");
                    return AuthenticateResult.Success(ticket);
                }
                catch (Exception)
                {
                    return AuthenticateResult.Fail($"Unable to extract cookie '{AuthConstants.AUTH_COOKIE_NM}', cookie might be damaged/modified");
                }
                
            }

            logger.LogDebug("Cookie authentication failed");
            return AuthenticateResult.NoResult();
        }

        public void SignIn(AuthenticateResult authResult, HttpContextBase contextBase)
        {
            if (authResult.Succeeded)
            {
                var protectedTicket = dataProtector.Protect(serializer.Serialize(authResult.Ticket));
                var encodedProtectedTicket = Convert.ToBase64String(protectedTicket);

                var cookie = new HttpCookie(AuthConstants.AUTH_COOKIE_NM)
                {
                    Expires = DateTime.Now.AddMinutes(COOKIE_TIMEOUT_IN_MINUTES),
                    Secure = contextBase.Request.Url.Scheme == "https",
                    HttpOnly = true,
                    Value = encodedProtectedTicket
                };

                contextBase.Response.AppendCookie(cookie);
                logger.LogDebug($"Auth Cookie '{AuthConstants.AUTH_COOKIE_NM}' added");
            }
        }
    }
}