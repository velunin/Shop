using System;
using AutoMapper;
using MassInstance;
using MassInstance.Client;
using MassInstance.Extensions;
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

using Shop.DataAccess.Dto;
using Shop.DataAccess.EF;
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

            services.AddEntityFrameworkSqlServer().AddDbContext<ShopDbContext>((provider, builder) =>
                builder.UseSqlServer(Configuration.GetConnectionString("ShopConnection"))
                   .UseInternalServiceProvider(provider), ServiceLifetime.Transient);

            services.AddAutoMapper();

            services.AddCqrs();

            services.AddCommandAndQueryHandlers(
                AppDomain.CurrentDomain.GetAssemblies(), 
                ServiceLifetime.Transient);

            RegisterServiceBus(services);

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

        private void RegisterServiceBus(IServiceCollection services)
        {
            services.AddSagaStateMachines(
                GetType().Assembly,
                ServiceLifetime.Transient);

            services.AddSingleton<ISagaRepository<OrderSagaContext>>(provider =>
                new EntityFrameworkSagaRepository<OrderSagaContext>(
                    provider.GetRequiredService<ShopDbContext>,
                    optimistic: true));

            services.AddServices(srvCfg =>
            {
                srvCfg
                    .AddServiceEndpoint(
                        ServicesQueues.OrderServiceSagaQueue,
                        consumeCfg => consumeCfg.AddSaga<OrderSagaContext>());
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
