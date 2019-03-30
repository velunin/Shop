using System;

using AutoMapper;
using MassInstance;
using MassInstance.Client;
using MassInstance.Extensions;
using MassInstance.ServiceCollection;
using MassTransit;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Shop.DataAccess.Dto;
using Shop.DataAccess.EF;
using Shop.Domain.Commands.Cart;
using Shop.Services.Common;
using Shop.Services.Common.ErrorCodes;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Shop.Services.Cart
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

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<ShopDbContext>(
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
            services.AddServices(srvCfg =>
            {
                srvCfg
                    .AddServiceEndpoint(
                        ServicesQueues.CartServiceCommandsQueue,
                        consumeCfg => consumeCfg
                            .AddCommandConsumer<AddOrUpdateProductInCart>()
                            .AddCommandConsumer<DeleteProductFromCart>(),
                        x => x.SetDefaultExceptionResponse(
                            (int)CartErrorCodes.UnknownError, 
                            "Unknown cart error"));
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
