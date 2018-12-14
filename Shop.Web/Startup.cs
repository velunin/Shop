using System;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using AutoMapper;

using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;

using Microsoft.Extensions.Options;

using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;
using Rds.Cqrs.Microsoft.DependencyInjection;
using Rds.Cqrs.Queries;
using Shop.DataAccess.Dto;
using Shop.DataAccess.EF;
using Shop.Domain.Commands.Order;
using Shop.Infrastructure;
using Shop.Services.Common;
using Shop.Web.Models;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

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
            services.AddRdsCqrs();

            AddServiceBus(services);

            services.Scan(scan =>
                scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            services.Scan(scan =>
                scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(IResultingCommandHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            services.Scan(scan =>
                scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

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

        private void AddServiceBus(IServiceCollection services)
        {
            services.AddServiceClient(mapper =>
                mapper
                
                //Commands
                    //.Map<CreateOrderCommand>(ServicesQueues.OrderServiceCommandQueue)
                    //.Map<AddOrderContactsCommand>(ServicesQueues.OrderServiceCommandQueue)
                    //.Map<PayOrderCommand>(ServicesQueues.OrderServiceCommandQueue)

                    //Saga
                    .Map<CreateOrderCommand>(ServicesQueues.OrderServiceSagaQueue)
                    .Map<AddOrderContactsCommand>(ServicesQueues.OrderServiceSagaQueue)
                    .Map<PayOrderCommand>(ServicesQueues.OrderServiceSagaQueue)
            );
            services.AddMassTransit();

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var rabbitConfig = provider.GetService<IOptions<RabbitMqConfig>>().Value;

                cfg.Host(new Uri(rabbitConfig.Uri), h =>
                {
                    h.Username(rabbitConfig.User);
                    h.Password(rabbitConfig.Password);
                });
            }));
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
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
