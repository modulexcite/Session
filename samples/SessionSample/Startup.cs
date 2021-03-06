﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Caching.Redis;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace SessionSample
{
    public class Startup
    {
        public Startup(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Verbose);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Adds a default in-memory implementation of IDistributedCache
            services.AddCaching();

            // Uncomment the following line to use the Redis implementation of IDistributedCache.
            // This will override any previously registered IDistributedCache service.
            //services.AddTransient<IDistributedCache, RedisCache>();

            services.AddSession();

            services.ConfigureSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromSeconds(30);
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSession();

            app.Map("/session", subApp =>
            {
                subApp.Run(async context =>
                {
                    int visits = 0;
                    visits = context.Session.GetInt32("visits") ?? 0;
                    context.Session.SetInt32("visits", ++visits);
                    await context.Response.WriteAsync("Counting: You have visited our page this many times: " + visits);
                });
            });

            app.Run(async context =>
            {
                int visits = 0;
                visits = context.Session.GetInt32("visits") ?? 0;
                await context.Response.WriteAsync("<html><body>");
                if (visits == 0)
                {
                    await context.Response.WriteAsync("Your session has not been established.<br>");
                    await context.Response.WriteAsync(DateTime.Now + "<br>");
                    await context.Response.WriteAsync("<a href=\"/session\">Establish session</a>.<br>");
                }
                else
                {
                    context.Session.SetInt32("visits", ++visits);
                    await context.Response.WriteAsync("Your session was located, you've visited the site this many times: " + visits);
                }
                await context.Response.WriteAsync("</body></html>");
            });
        }
    }
}