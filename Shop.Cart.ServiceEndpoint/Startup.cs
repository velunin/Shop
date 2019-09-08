using System;
using AutoMapper;
using MassInstance;
using MassInstance.Bus;
using MassInstance.RabbitMq;
using MassInstance.ServiceCollection;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shop.Cart.DataAccess;
using Shop.Shared.Services;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Shop.Cart.ServiceEndpoint
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<RabbitMqConfig>(Configuration.GetSection("RabbitMqConfig"));

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<CartDbContext>(
                    (provider, builder) =>
                        builder
                            .UseSqlServer(Configuration.GetConnectionString("ShopConnection"))
                            .UseInternalServiceProvider(provider),
                    ServiceLifetime.Transient);

            services.AddAutoMapper();
            services.AddCqrs();
            services.AddCommandAndQueryHandlers(
                AppDomain.CurrentDomain.GetAssemblies(),
                ServiceLifetime.Transient);

            RegisterServiceBus(services);

            services.AddSingleton<IHostedService, ServiceBusBackgroundService>();

            return services.BuildServiceProvider();
        }

        private void RegisterServiceBus(IServiceCollection services)
        {
            services.AddMassInstance();

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

                    busCfg.AddServiceClient(
                        host, 
                        "cart-callback-queue", 
                        configurator => configurator.AddService<OrderServiceMap>());
                }));

            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IServiceBus>());
            services.AddSingleton<IBusControl>(provider => provider.GetRequiredService<IServiceBus>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
