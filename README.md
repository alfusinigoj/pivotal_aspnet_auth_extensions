### Package helps in enabling windows authentication (using kerberos) in any cloud platform (when containers are not domain joined)

Build | PivotalServices.AspNet.Auth.Extensions |
--- | --- |
[![Build Status](https://dev.azure.com/ajaganathan-home/pivotal_aspnet_auth_extensions/_apis/build/status/alfusinigoj.pivotal_aspnet_auth_extensions?branchName=master)](https://dev.azure.com/ajaganathan-home/pivotal_aspnet_auth_extensions/_build/latest?definitionId=6&branchName=master) | [![NuGet](https://img.shields.io/nuget/v/PivotalServices.AspNet.Auth.Extensions.svg?style=flat-square)](http://www.nuget.org/packages/PivotalServices.AspNet.Auth.Extensions) 

### Quick Links
- [Supported ASP.NET apps](https://github.com/alfusinigoj/pivotal_aspnet_auth_extensions#supported-aspnet-apps)
- [Salient features](https://github.com/alfusinigoj/pivotal_aspnet_auth_extensions#salient-features)
- [Prerequisites](https://github.com/alfusinigoj/pivotal_aspnet_auth_extensions#prerequisites)
- [Enable Windows Auth](https://github.com/alfusinigoj/pivotal_aspnet_auth_extensions/#enable-windows-authentication)
- [Sample Implementations](https://github.com/alfusinigoj/pivotal_aspnet_auth_extensions/tree/master/samples) 

### Supported ASP.NET apps
- WebAPI
- MVC
- WebForms
- Other types like (.asmx, .ashx)
- All the above with Unity
- All the above with Autofac

### Salient features
- Supports Windows Authentication using Kerberos for ASP.NET Web applications (except WCF)
- Real samples are available [here](https://github.com/alfusinigoj/pivotal_aspnet_auth_extensions/tree/master/samples) 

### Packages
- Extensions Package - [PivotalServices.AspNet.Auth.Extensions](https://www.nuget.org/packages/PivotalServices.AspNet.Auth.Extensions)
 
### Prerequisites
- Make sure your application is upgraded to ASP.NET framework 4.6.2 or above

### Enable Windows Authentication
- Uses Kerberos based authentication
- Supports all ASP.NET application types except WCF
- Install package [PivotalServices.AspNet.Auth.Extensions](https://www.nuget.org/packages/PivotalServices.AspNet.Auth.Extensions)
- In `Global.asax.cs`, add code as below under `Application_Start`

```c#
    using PivotalServices.AspNet.Bootstrap.Extensions
    using PivotalServices.AspNet.Auth.Extensions;

    protected void Application_Start()
    {
        AppBuilder.Instance
                .AddWindowsAuthentication()
                .Build()
                .Start();
    }
``` 
- `AddWindowsAuthentication()` have optional parameters
	- `principalPassword` can be used incase of pushing the secret from any external sources like vault
	- `dataProtectionKey` default value is `default`

- Once you setup the service account and SPN (as mentioned [below](https://github.com/alfusinigoj/pivotal_aspnet_auth_extensions/#create-spn-service-principal-name)), you need to set `PRINCIPAL_PASSWORD` with the service account's password.
- `PRINCIPAL_PASSWORD` can be set via environment variable or any other configuration sources (IConfiguration) you are using for the application (e.g. config server, yaml, json, etc.) as below.
- To whitelist any specific `path` like `actuator/health` or `actuator/info`, you need to set `WHITELIST_PATH_CSV` with the service account's password.
- `WHITELIST_PATH_CSV` can be set via environment variable or any other configuration sources (IConfiguration) you are using for the application (e.g. config server, yaml, json, etc.) as below.
- By default `/cloudfoundryapplication,/cloudfoundryapplication/,/actuator,/actuator/` are whitelisted

```c#
    using PivotalServices.AspNet.Bootstrap.Extensions
	using PivotalServices.AspNet.Auth.Extensions;

    protected void Application_Start()
    {
		AppBuilder
			.Instance
			.AddWindowsAuthentication()
			.ConfigureAppConfiguration((hostBuilder, configBuilder) =>
			{
				configBuilder.AddEnvironmentVariables();
				//Add additional configurations here
			})
			.Build()
			.Start();
	}
```

- If not exists already, add the `machineKey` section to `web.config` as below. You can generate a new one from [Developer Fusion](https://www.developerfusion.com/tools/generatemachinekey). This is for data protection purposes.

```
    <machineKey validationKey="B2FFA07BEA941CBFD2F2450A5BE4D8F6ABFFE624F3DBB35BC589D34C5647F65235634AEC71B5C1E2453BE8D466B6818A9438AC2FFE0C09024052FFF27C85EB3C" 
            decryptionKey="4AFFE5CFAE4F97BFAE7736E5A6B85E921EF209FA84F4BC665993E72393B080DC" validation="SHA1" decryption="AES" />
```

**NOTE: The skeleton of the machine key section will be added while installing the package**

- Add the application's url to trusted sites. If your application's url is `http://foo.bar`, add `http://foo.bar` into trusted sites.

**NOTE: Make sure you are browsing the application from a domain joined computer (same domain where the SPN is created)**

### Create SPN (Service Principal Name)

**NOTE: This is mandate for front end browser applications, but for services it is not required. In other words, if you want to access your application via browser, you need to have the SPN created, as mentioned below.**

Identify the service account for which the application should be running under (imagine as your application running in IIS on an APP POOL, under a service account). If your application's url is `http://foo.bar`, then you have to create a SPN for the service account as `http/foo.bar`

Command Syntax(using the above sample SPN):

```text
SetSpn -S http/foo.bar <domain\service_account_name>
```
To check to see which SPNs are currently registered with your service account, run the following command:

```text
SetSpn -L <domain\service_account_name>
```

**NOTE: You should have elevated privileges to execute the above command. Eventually this should be executed part of the deployment pipeline, for each application**

### Ongoing development packages in MyGet

Feed | PivotalServices.AspNet.Auth.Extensions |
--- | --- |
[V3](https://www.myget.org/F/ajaganathan/api/v3/index.json) | [![MyGet](https://img.shields.io/myget/ajaganathan/v/PivotalServices.AspNet.Auth.Extensions.svg?style=flat-square)](https://www.myget.org/feed/ajaganathan/package/nuget/PivotalServices.AspNet.Auth.Extensions) |

### Issues
- Kindly raise any issues under [GitHub Issues](https://github.com/alfusinigoj/pivotal_aspnet_auth_extensions/issues)

### Contributions are welcome!
