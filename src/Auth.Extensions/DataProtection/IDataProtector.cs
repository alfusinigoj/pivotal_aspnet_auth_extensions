namespace PivotalServices.AspNet.Auth.Extensions.DataProtection
{
    public interface IDataProtector
    {
        byte[] Protect(byte[] unsecuredData);
        byte[] UnProtect(byte[] securedData);
    }
}
