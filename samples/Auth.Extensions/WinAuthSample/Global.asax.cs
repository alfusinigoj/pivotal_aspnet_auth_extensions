﻿using PivotalServices.AspNet.Auth.Extensions;
using PivotalServices.AspNet.Bootstrap.Extensions;
using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

namespace WinAuthSample
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AppBuilder.Instance
                .AddWindowsAuthentication()
                .Build()
                .Start();
        }

        protected void Application_Stop()
        {
            AppBuilder.Instance.Stop();
        }
    }
}