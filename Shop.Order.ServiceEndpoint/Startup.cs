using System;
using AutoMapper;
using MassInstance;
using MassInstance.Bus;
using MassInstance.Configuration;
using MassInstance.RabbitMq;
using MassInstance.ServiceCollection;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.Saga;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shop.Order.DataAccess;
using Shop.Order.DataAccess.CommandHandlers;
using Shop.Order.DataAccess.Dto;
using Shop.Shared.Services;
using Shop.Shared.Services.ErrorCodes;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Shop.Order.ServiceEndpoint
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1); 

            services.Configure<RabbitMqConfig>(Configuration.GetSection("RabbitMqConfig"));

            services.AddEntityFrameworkSqlServer().AddDbContext<OrderDbContext>((provider, builder) =>
                builder.UseSqlServer(Configuration.GetConnectionString("ShopConnection"))
                   .UseInternalServiceProvider(provider), ServiceLifetime.Transient);

            services.AddAutoMapper();

            services.AddCqrs();

            services.AddCommandAndQueryHandlers(
                typeof(CreateOrderHandler).Assembly, 
                ServiceLifetime.Transient);

            RegisterServiceBus(services);

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

        private void RegisterServiceBus(IServiceCollection services)
        {
            services.AddMassInstance();

            services.AddSagaStateMachines(
                GetType().Assembly,
                ServiceLifetime.Transient);

            services.AddSingleton<ISagaRepository<OrderSagaContext>>(provider =>
                new EntityFrameworkSagaRepository<OrderSagaContext>(
                    provider.GetRequiredService<OrderDbContext>,
                    optimistic: true));

            services.AddSingleton(provider => Bus.Factory.CreateMassInstanceRabbitMqBus(
                provider.GetRequiredService<IMassInstanceConsumerFactory>(),
                busCfg =>
                {
                    var rabbitConfig = provider.GetService<IOptions<RabbitMqConfig>>().Value;

                    var host = busCfg.Host(new Uri(rabbitConfig.Uri), h =>
                    {
                        h.Username(rabbitConfig.User);
                        h.Password(rabbitConfig.Password);
                    });

                    busCfg.AddServiceHost<OrderServiceMap>(host, srvCfg =>
                    {
                        var orderCommands = srvCfg.SelectQueue(s => s.OrderCommands);

                        orderCommands.ConfigureCommandExceptionHandling = options =>
                            options.SetDefaultExceptionResponse(
                                (int) OrderErrorCodes.UnknownError, "Unknown error");

                        orderCommands
                            .SelectCommand(c => c.CreateOrderCommand)
                            .SetExceptionHandling(h => h.Map<InvalidOperationException>(
                                (int) OrderErrorCodes.AlreadySold, "Already sold"));
                    });
                }));

            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IServiceBus>());
            services.AddSingleton<IBusControl>(provider => provider.GetRequiredService<IServiceBus>());
        }
    }
}
