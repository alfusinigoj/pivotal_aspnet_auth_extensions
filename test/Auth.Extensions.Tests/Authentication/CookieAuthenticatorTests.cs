﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using PivotalServices.AspNet.Auth.Extensions.Authentication;
using PivotalServices.AspNet.Auth.Extensions.DataProtection;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using Xunit;

namespace PivotalServices.AspNet.Auth.Extensions.Tests.Authentication
{
    public class CookieAuthenticatorTests
    {
        IDataProtector dataProtector;
        Mock<ILogger<CookieAuthenticator>> logger;
        Mock<HttpServerUtilityBase> server;
        Mock<HttpResponseBase> response;
        Mock<HttpRequestBase> request;
        Mock<HttpSessionStateBase> session;
        Mock<HttpContextBase> context;
        Mock<HttpBrowserCapabilitiesBase> browser;
        HttpCookieCollection cookies;

        public CookieAuthenticatorTests()
        {
            dataProtector = new DataProtectorStub();
            logger = new Mock<ILogger<CookieAuthenticator>>();
            server = new Mock<HttpServerUtilityBase>(MockBehavior.Loose);
            response = new Mock<HttpResponseBase>(MockBehavior.Loose);
            request = new Mock<HttpRequestBase>(MockBehavior.Strict);
            session = new Mock<HttpSessionStateBase>();
            context = new Mock<HttpContextBase>();
            browser = new Mock<HttpBrowserCapabilitiesBase>();
            cookies = new HttpCookieCollection();
            SetHttpContext();
        }

        [Fact]
        public void Test_IsOfTypeICookieAuthenticator()
        {
            Assert.IsAssignableFrom<ICookieAuthenticator>(new CookieAuthenticator(dataProtector, logger.Object));
        }

        [Fact]
        public void Test_ReturnsNoResultIfBrowserDoesNotSupportCookies()
        {
            browser.SetupGet(b => b.Cookies).Returns(false);

            var authenticator = new CookieAuthenticator(dataProtector, logger.Object);

            Assert.False(authenticator.Authenticate(context.Object).Succeeded);
        }

        [Fact]
        public void Test_ReturnsSuccessIfValidCookieEsists()
        {
            var serializer = new TicketSerializer();
            var ticket = new AuthenticationTicket(
                            new ClaimsPrincipal(
                                new ClaimsIdentity(new[]
                                {
                                        new Claim(ClaimTypes.Name,"Foo User"),
                                }, AuthConstants.SPNEGO_DEFAULT_SCHEME)),
                            AuthConstants.SPNEGO_DEFAULT_SCHEME);

            var serializedTicket = serializer.Serialize(ticket);
            var protectedTicket = dataProtector.Protect(serializedTicket);
            var encodedTicket = Convert.ToBase64String(protectedTicket);

            var cookie = new HttpCookie(AuthConstants.AUTH_COOKIE_NM)
            {
                Expires = DateTime.Now.AddDays(CookieAuthenticator.COOKIE_TIMEOUT_IN_MINUTES),
                Value = encodedTicket
            };

            cookies.Set(cookie);

            browser.SetupGet(b => b.Cookies).Returns(true);

            var authenticator = new CookieAuthenticator(dataProtector, logger.Object);

            var result = authenticator.Authenticate(context.Object);

            Assert.True(result.Succeeded);
            Assert.Equal("Foo User", result.Principal.Identity.Name);

        }

        [Fact]
        public void Test_ReturnsFailureIf_InValidCookieEsistsOrIfCookieIsDamaged()
        {
            var serializer = new TicketSerializer();
            var ticket = new AuthenticationTicket(
                            new ClaimsPrincipal(
                                new ClaimsIdentity(new[]
                                {
                                        new Claim(ClaimTypes.Name,"Foo User"),
                                }, AuthConstants.SPNEGO_DEFAULT_SCHEME)),
                            AuthConstants.SPNEGO_DEFAULT_SCHEME);

            var serializedTicket = serializer.Serialize(ticket);
            var protectedTicket = dataProtector.Protect(serializedTicket);
            var encodedTicket = Convert.ToBase64String(protectedTicket);

            var cookie = new HttpCookie(AuthConstants.AUTH_COOKIE_NM)
            {
                Expires = DateTime.Now.AddDays(CookieAuthenticator.COOKIE_TIMEOUT_IN_MINUTES),
                Value = encodedTicket + "Corrupt"
            };

            cookies.Set(cookie);

            browser.SetupGet(b => b.Cookies).Returns(true);

            var authenticator = new CookieAuthenticator(dataProtector, logger.Object);

            var result = authenticator.Authenticate(context.Object);

            Assert.False(result.Succeeded);
            Assert.Equal($"Unable to extract cookie '{AuthConstants.AUTH_COOKIE_NM}', cookie might be damaged/modified", result.Failure.Message);
        }

        [Fact]
        public void Test_ReturnsNoResultOrNotSuccessIf_CookieDoesNotEsist()
        {
            browser.SetupGet(b => b.Cookies).Returns(true);

            var authenticator = new CookieAuthenticator(dataProtector, logger.Object);

            var result = authenticator.Authenticate(context.Object);

            Assert.False(result.Succeeded);
        }

        [Fact]
        public void Test_SignIn_DoesNotAddCookie_IfAuthResultIsNotSuccess()
        {
            var authenticator = new CookieAuthenticator(dataProtector, logger.Object);

            authenticator.SignIn(AuthenticateResult.NoResult(), context.Object);

            var authCookie = context.Object.Response.Cookies.Get(AuthConstants.AUTH_COOKIE_NM);
            Assert.Null(authCookie);
        }

        [Fact]
        public void Test_SignIn_AddsCookie_IfAuthResultIsSuccess()
        {
            var serializer = new TicketSerializer();
            var ticket = new AuthenticationTicket(
                            new ClaimsPrincipal(
                                new ClaimsIdentity(new[]
                                {
                                        new Claim(ClaimTypes.Name,"Foo User"),
                                }, AuthConstants.SPNEGO_DEFAULT_SCHEME)),
                            AuthConstants.SPNEGO_DEFAULT_SCHEME);

            var serializedTicket = serializer.Serialize(ticket);
            var protectedTicket = dataProtector.Protect(serializedTicket);
            var encodedTicket = Convert.ToBase64String(protectedTicket);

            var cookie = new HttpCookie(AuthConstants.AUTH_COOKIE_NM)
            {
                Expires = DateTime.Now.AddDays(CookieAuthenticator.COOKIE_TIMEOUT_IN_MINUTES),
                Value = encodedTicket
            };

            var authenticator = new CookieAuthenticator(dataProtector, logger.Object);

            authenticator.SignIn(AuthenticateResult.Success(ticket), context.Object);

            response.Verify(r => r.AppendCookie(It.Is<HttpCookie>(c => Convert.ToBase64String(dataProtector.UnProtect(Convert.FromBase64String(c.Value))) == Convert.ToBase64String(dataProtector.UnProtect(Convert.FromBase64String(encodedTicket)))
                                                                    && c.Expires.Date.Minute == DateTime.Now.AddMinutes(CookieAuthenticator.COOKIE_TIMEOUT_IN_MINUTES).Date.Minute)), Times.Once);
        }

        private void SetHttpContext()
        {
            request.Setup(r => r.UserHostAddress).Returns("127.0.0.1");
            session.Setup(s => s.SessionID).Returns(Guid.NewGuid().ToString());
            context.SetupGet(c => c.Request).Returns(request.Object);
            context.SetupGet(c => c.Response).Returns(response.Object);
            context.SetupGet(c => c.Server).Returns(server.Object);
            context.SetupGet(c => c.Session).Returns(session.Object);
            request.SetupGet(r => r.Cookies).Returns(cookies);
            response.SetupGet(r => r.Cookies).Returns(cookies);
            request.SetupGet(r => r.Browser).Returns(browser.Object);
            request.SetupGet(r => r.Url).Returns(new Uri("http://localhost"));
        }
    }

    internal class DataProtectorStub: IDataProtector
    {
        public byte[] Protect(byte[] unsecuredData)
        {
            return ProtectedData.Protect(unsecuredData, null, DataProtectionScope.LocalMachine);
        }

        public byte[] UnProtect(byte[] securedData)
        {
            return ProtectedData.Unprotect(securedData, null, DataProtectionScope.LocalMachine);
        }
    }
}
