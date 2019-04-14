using System;

using AutoMapper;
using MassInstance;
using MassInstance.RabbitMq;
using MassInstance.ServiceCollection;
using MassTransit;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Shop.DataAccess.EF;
using Shop.Domain;
using Shop.Domain.ErrorCodes;
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
            services.Configure<DataAccess.Dto.RabbitMqConfig>(Configuration.GetSection("RabbitMqConfig"));

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
            services.AddMassInstance(ext => ext.From<CartServiceMap>());

            services.AddSingleton(provider => Bus.Factory.CreateMassInstanceRabbitMqBus(
                provider.GetRequiredService<IMassInstanceConsumerFactory>(),
                busCfg =>
                {
                    var rabbitConfig = provider.GetService<IOptions<DataAccess.Dto.RabbitMqConfig>>().Value;

                    var host = busCfg.Host(new Uri(rabbitConfig.Uri), h =>
                    {
                        h.Username(rabbitConfig.User);
                        h.Password(rabbitConfig.Password);
                    });

                    busCfg.AddServiceHost<CartServiceMap>(
                        host,
                        srvCfg =>
                        {
                            srvCfg.Configure(cartServiceMap => cartServiceMap.CartServiceCommands,
                                queueCfg =>
                                {
                                    queueCfg.Configure(
                                        x => x.AddOrUpdateProductInCart, commandCfg =>
                                        {
                                            //Custom mapping exception to error codes for command
                                        });

                                    queueCfg.Configure(
                                        x => x.DeleteProductFromCart, commandCfg =>
                                        {
                                            //Custom mapping exception to error codes for command
                                        });

                                    queueCfg.ConfigureCommandExceptionHandling = opt =>
                                    {
                                        //Custom mapping exception to error codes for queue
                                    };
                                });

                            srvCfg.ConfigureCommandExceptionHandling = exceptionCfg =>
                            {
                                //Custom mapping exception to error codes for service
                                exceptionCfg.SetDefaultExceptionResponse(
                                    (int) CartErrorCodes.UnknownError,
                                    "Unknown cart error");
                            };
                        });
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
