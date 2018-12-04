﻿using System;
using Automatonymous;
using AutoMapper;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.Saga;
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
using Shop.Domain.Commands.Order;
using Shop.Domain.Events;
using Shop.Infrastructure;
using Shop.Services.Common;
using Shop.Services.Order.Sagas;
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

            services.AddEntityFrameworkSqlServer().AddDbContext<ShopDbContext>((provider, builder) =>
                builder.UseSqlServer(Configuration.GetConnectionString("ShopConnection"))
                   .UseInternalServiceProvider(provider), ServiceLifetime.Transient);

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

            services.Scan(scan =>
                scan.FromAssemblies(typeof(OrderSaga).Assembly)
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(SagaStateMachine<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            AddServiceBus(services);

            services.AddSingleton<IFakeMailService, FakeMailService>();

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
            AddSagaRepository(services);

            services.AddServices(srvCfg =>
                {
                    srvCfg

                        .AddServiceEndpoint(ServicesQueues.OrderServiceSagaQueue,
                            consumeCfg => consumeCfg.AddSaga<OrderSagaContext>())

                        .AddServiceEndpoint(
                            ServicesQueues.OrderServiceEventsQueue,
                            consumeCfg => consumeCfg
                                .AddEventConsumer<OrderCreated>())

                        .AddServiceEndpoint(
                            ServicesQueues.OrderServiceCommandQueue,
                            consumeCfg => consumeCfg
                                .AddCommandConsumer<AddOrderContactsCommand>()
                                .AddCommandConsumer<CreateOrderCommand>(ExceptionResponseMappings
                                    .CreateOrderCommandMap),
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

        private void AddSagaRepository(IServiceCollection services)
        {
            services.AddTransient<ISagaRepository<OrderSagaContext>>(provider =>
                new EntityFrameworkSagaRepository<OrderSagaContext>(provider.GetRequiredService<ShopDbContext>,
                    optimistic: true));
        }
    }
}