﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using AutoMapper;

using MassInstance;
using MassInstance.Bus;
using MassInstance.Client;
using MassInstance.Configuration.Client;
using MassInstance.RabbitMq;
using MassInstance.ServiceCollection;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;

using Shop.DataAccess.EF;
using Shop.Domain;

using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using RabbitMqConfig = Shop.DataAccess.Dto.RabbitMqConfig;

namespace Shop.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<RabbitMqConfig>(Configuration.GetSection("RabbitMqConfig"));

            services.AddEntityFrameworkSqlServer().AddDbContext<ShopDbContext>((provider, builder) =>
                builder.UseSqlServer(Configuration.GetConnectionString("ShopConnection"))
                    .UseInternalServiceProvider(provider), ServiceLifetime.Transient);

            services.AddAutoMapper();
            services.AddCqrs();
            services.AddCommandAndQueryHandlers(
                AppDomain.CurrentDomain.GetAssemblies(),
                ServiceLifetime.Transient);

            RegisterServiceBus(services);

            services.AddSingleton<IHostedService, ServiceBusBackgroundService>();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            { 
                options.Cookie.Name = ".Shop.Web.Session";
                options.IdleTimeout = TimeSpan.FromHours(4);
                options.Cookie.HttpOnly = true;
            });

            return services.BuildServiceProvider();
        }

        private void RegisterServiceBus(IServiceCollection services)
        {
            var rabbitMqConfig = Configuration.GetSection("RabbitMqConfig").Get<RabbitMqConfig>();

            services.AddMassInstance(ext => ext
                .ResultTypesFrom<CartServiceMap>()
                .ResultTypesFrom<OrderServiceMap>());

            services.AddSingleton(provider => Bus.Factory.CreateMassInstanceRabbitMqBus(
                provider.GetRequiredService<IMassInstanceConsumerFactory>(),
                cfg =>
                {
                    var rabbitHost = cfg.Host(new Uri(rabbitMqConfig.Uri), h =>
                    {
                        h.Username(rabbitMqConfig.User);
                        h.Password(rabbitMqConfig.Password);
                    });

                    cfg.AddServiceClient(
                        rabbitHost,
                        "frontend-web-callback",
                        configurator =>
                        {
                            configurator.AddService<CartServiceMap>();
                            configurator.AddService<OrderServiceMap>();
                        });
                }));

            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IServiceBus>());
            services.AddSingleton<IBusControl>(provider => provider.GetRequiredService<IServiceBus>());
            services.AddSingleton<IServiceClient>(provider => provider.GetRequiredService<IServiceBus>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
           
            app.UseStaticFiles();
            app.UseSession();
            app.Use(async (context, next) =>
            {
                context.Session.SetString("___forcreatesession___", @"¯\_(ツ)_/¯");
                await next.Invoke();
            });
       
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
