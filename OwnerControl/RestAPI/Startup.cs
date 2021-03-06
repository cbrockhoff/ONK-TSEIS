﻿using System;
using Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OwnerControl.Persistence;
using RestAPI.Helpers;
using Shared.Contracts.Events;
using Shared.Messaging;
using StructureMap;

namespace OwnerControl.RestAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            var provider =  ConfigureIoC(services);
            SetupEventListeners(provider);
            return provider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseLoggingMiddleware();
            app.UseMvc();
        }

        private IServiceProvider ConfigureIoC(IServiceCollection serviceCollection)
        {
            var service = "OwnerControl.RestAPI";
            var container = new Container();

            container.Configure(c =>
            {
                c.Scan(a =>
                {
                    a.AssemblyContainingType<Startup>();
                    a.WithDefaultConventions();
                });

                c.AddRegistry(new MessagingRegistry(service));
                c.AddRegistry(new PersistenceRegistry(Configuration.GetConnectionString("OwnerControl")));
                c.AddRegistry(new LoggingRegistry(service, Configuration.GetConnectionString("Logging")));
                c.Populate(serviceCollection);
            });

            return container.GetInstance<IServiceProvider>();
        }

        private void SetupEventListeners(IServiceProvider provider)
        {
            var bus = provider.GetService<IMessageBus>();
            var repo = provider.GetService<IStocksRepository>();
            bus.Subscribe<StockTradeHappenedEventDto>(async e =>
            {
                await repo.Write(e.BuyerId, e.Stock, e.Amount);
                await repo.Delete(e.SellerId, e.Stock, e.Amount);
            });
        }
    }
}
