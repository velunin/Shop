using System;
using System.Linq;
using Automatonymous;
using AutoMapper;
using MassInstance;
using MassInstance.Client;
using MassInstance.RabbitMq;
using MassInstance.ServiceCollection;
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
using Shop.Domain;
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

            services.AddSingleton<IFakeMailService, FakeMailService>();

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
                    provider.GetRequiredService<ShopDbContext>,
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

                    busCfg.AddService<OrderServiceMap>(host, srvCfg =>
                    {
                        srvCfg.Configure(
                            serviceMap => serviceMap.OrderServiceSaga, 
                            queueCfg => queueCfg.ConfigureSaga<OrderSagaContext>());
                    });
                }));

            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
        }
    }
}
