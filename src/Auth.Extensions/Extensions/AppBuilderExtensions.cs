﻿using Kerberos.NET;
using Kerberos.NET.Crypto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PivotalServices.AspNet.Auth.Extensions.Authentication;
using PivotalServices.AspNet.Auth.Extensions.DataProtection;
using PivotalServices.AspNet.Auth.Extensions.Handlers;
using PivotalServices.AspNet.Bootstrap.Extensions;
using PivotalServices.AspNet.Bootstrap.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PivotalServices.AspNet.Auth.Extensions
{
    public class AuthConstants
    {
        public const string SPNEGO_DEFAULT_SCHEME = "Negotiate";
        public const string AUTH_COOKIE_NM = "AUTH";

        public const string DATA_PROTECTION_KEY_NM = "DATA_PROTECTION_KEY";

        public const string PRINCIPAL_PASSWORD_NM = "PRINCIPAL_PASSWORD";

        public const string WHITELIST_PATHS_CSV_NM = "WHITELIST_PATH_CSV";
    }

    public static class AppBuilderExtensions
    {
        public static AppBuilder AddWindowsAuthentication(this AppBuilder instance, string principalPassword = null, string dataProtectionKey = null)
        {
            var inMemoryConfigStore = ReflectionHelper
                .GetNonPublicInstancePropertyValue<Dictionary<string, string>>(instance, "InMemoryConfigStore");

            inMemoryConfigStore[AuthConstants.PRINCIPAL_PASSWORD_NM] = principalPassword 
                ?? Environment.GetEnvironmentVariable(AuthConstants.PRINCIPAL_PASSWORD_NM);

            inMemoryConfigStore[AuthConstants.DATA_PROTECTION_KEY_NM] = dataProtectionKey 
                ?? Environment.GetEnvironmentVariable(AuthConstants.DATA_PROTECTION_KEY_NM) 
                ?? "default";

            var handlers = ReflectionHelper
                .GetNonPublicInstanceFieldValue<List<Type>>(instance, "Handlers");

            handlers.Add(typeof(WindowsAuthenticationHandler));

            ReflectionHelper
                 .GetNonPublicInstanceFieldValue<List<Action<HostBuilderContext, IServiceCollection>>>(instance, "ConfigureServicesDelegates")
                 .Add((builderContext, services) =>
                 {
                     services.AddSingleton<IDataProtector, MachineKeyDataProtector>();
                     services.AddSingleton((provider) => GetAuthenticator(provider));

                     services.AddSingleton<ITicketIssuer, KerberosTicketIssuer>();

                     services.AddSingleton<ISpnegoAuthenticator, SpnegoAuthenticator>();
                     services.AddSingleton<ICookieAuthenticator, CookieAuthenticator>();
                 });

            return instance;
        }

        private static KerberosAuthenticator GetAuthenticator(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            return new KerberosAuthenticator(new KerberosValidator(new KerberosKey(configuration[AuthConstants.PRINCIPAL_PASSWORD_NM])))
            {
                UserNameFormat = UserNameFormat.DownLevelLogonName
            };
        }
    }
}
