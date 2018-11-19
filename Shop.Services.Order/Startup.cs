using System;

using AutoMapper;

using MassTransit;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Rds.Cqrs.Commands;
using Rds.Cqrs.Microsoft.DependencyInjection;
using Rds.Cqrs.Queries;
using Shop.DataAccess.Dto;
using Shop.DataAccess.EF;
using Shop.Domain.Commands.Cart;
using Shop.Domain.Commands.Order;
using Shop.Domain.Events;
using Shop.Infrastructure;
using Shop.Services.Common;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Shop.Services.Order
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<RabbitMqConfig>(Configuration.GetSection("RabbitMqConfig"));

            services.AddEntityFrameworkNpgsql().AddDbContext<ShopDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("ShopConnectionPostgre")));

            services.AddAutoMapper();

            services.AddRdsCqrs();

            services.Scan(scan =>
                scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            services.Scan(scan =>
                scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            AddServiceBus(services);

            services.AddSingleton<IHostedService, ServiceBusBackgroundService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private void AddServiceBus(IServiceCollection services)
        {
            services.AddServices(srvCfg =>
            {
                srvCfg

                    .AddServiceEndpoint(
                        ServicesQueues.OrderServiceEventsQueue,
                        consumeCfg => consumeCfg
                            .AddEventConsumer<OrderCreated>())

                    .AddServiceEndpoint(
                        ServicesQueues.OrderServiceCommandQueue,
                        consumeCfg => consumeCfg
                            .AddCommandConsumer<AddOrderContactsCommand>()
                            .AddCommandConsumer<CreateOrderCommand>(ExceptionResponseMappings.CreateOrderCommandMap),

                        ExceptionResponseMappings.DefaultOrderServiceMap);
            });

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var rabbitConfig = provider.GetService<IOptions<RabbitMqConfig>>().Value;

                var host = cfg.Host(new Uri(rabbitConfig.Uri), h =>
                {
                    h.Username(rabbitConfig.User);
                    h.Password(rabbitConfig.Password);
                });
                
                cfg.LoadServices(provider, host);
            }));

            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
        }
    }
}
