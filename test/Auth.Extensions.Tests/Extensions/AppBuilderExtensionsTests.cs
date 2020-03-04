using PivotalServices.AspNet.Bootstrap.Extensions;
using PivotalServices.AspNet.Bootstrap.Extensions.Testing;
using Xunit;

namespace PivotalServices.AspNet.Auth.Extensions.Tests.Extensions
{
    public class AppBuilderExtensionsTests
    {
        [Fact]
        public void Test_AddWindowsAuthDependenciesSuccessfully()
        {
            TestProxy.InMemoryConfigStoreProxy.Clear();
            TestProxy.ConfigureServicesDelegatesProxy.Clear();
            TestProxy.ConfigureAppConfigurationDelegatesProxy.Clear();
            AppBuilder.Instance.AddWindowsAuthentication();

            Assert.Equal(1, TestProxy.ConfigureServicesDelegatesProxy.Count);

            Assert.Contains(TestProxy.HandlersProxy, h => h.FullName == "PivotalServices.AspNet.Auth.Extensions.Handlers.WindowsAuthenticationHandler");

            Assert.Null(TestProxy.InMemoryConfigStoreProxy[AuthConstants.PRINCIPAL_PASSWORD_NM]);
            Assert.Equal("default", TestProxy.InMemoryConfigStoreProxy[AuthConstants.DATA_PROTECTION_KEY_NM]);
        }

        [Fact]
        public void Test_AddWindowsAuthDependenciesWithArgumentsSuccessfully()
        {
            TestProxy.InMemoryConfigStoreProxy.Clear();
            TestProxy.ConfigureServicesDelegatesProxy.Clear();
            TestProxy.ConfigureAppConfigurationDelegatesProxy.Clear();
            AppBuilder.Instance.AddWindowsAuthentication(principalPassword: "password", dataProtectionKey: "key");

            Assert.Equal(1, TestProxy.ConfigureServicesDelegatesProxy.Count);

            Assert.Contains(TestProxy.HandlersProxy, h => h.FullName == "PivotalServices.AspNet.Auth.Extensions.Handlers.WindowsAuthenticationHandler");

            Assert.Equal("password", TestProxy.InMemoryConfigStoreProxy[AuthConstants.PRINCIPAL_PASSWORD_NM]);
            Assert.Equal("key", TestProxy.InMemoryConfigStoreProxy[AuthConstants.DATA_PROTECTION_KEY_NM]);
        }
    }
}
