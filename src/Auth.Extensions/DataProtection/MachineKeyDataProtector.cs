using Microsoft.Extensions.Configuration;
using System;
using System.Web.Security;

namespace PivotalServices.AspNet.Auth.Extensions.DataProtection
{
    public class MachineKeyDataProtector : IDataProtector
    {
        private readonly IConfiguration configuration;

        public MachineKeyDataProtector(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public byte[] Protect(byte[] unsecuredData)
        {
            return MachineKey.Protect(unsecuredData, configuration[AuthConstants.DATA_PROTECTION_KEY_NM]);
        }

        public byte[] UnProtect(byte[] securedData)
        {
            return MachineKey.Unprotect(securedData, configuration[AuthConstants.DATA_PROTECTION_KEY_NM]);
        }
    }
}
