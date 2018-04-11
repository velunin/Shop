using System;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NSaga.ServiceCollection;

using AutoMapper;

using Rds.CaraBus.RequestResponse.Extensions;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Decorators.Events;
using Rds.Cqrs.Events;
using Rds.Cqrs.Microsoft.DependencyInjection;
using Rds.Cqrs.Queries;

using RDS.CaraBus;
using RDS.CaraBus.RabbitMQ;

using Shop.DataAccess.EF;
using Shop.DataAccess.EF.NSaga;
using Shop.Infrastructure;
using Shop.Web.BackgroundServices;

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

            services.AddEntityFrameworkNpgsql().AddDbContext<ShopDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("ShopConnectionPostgre")));

            services.AddAutoMapper();
            services.AddRdsCqrs();

            AddCaraBus(services);

            services
                .AddNSagaComponents()
                .UseSagaRepository<NSagaEFRepository>();

            services.AddAppInfrastructure<WebFrontendServiceBusBootstrap>();

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

        private void AddCaraBus(IServiceCollection services)
        {
            services.AddSingleton<ICaraBus>(new RabbitMQCaraBus(new RabbitMQCaraBusOptions
            {
                ConnectionString = Configuration.GetConnectionString("RabbitMQ"),
                AutoStart = true
            }));
            services.AddCaraBusRequestClient();

            services.AddSingleton<EventDispatcher>();
            services.AddSingleton(factory =>
            {
                IEventDispatcher UnderlyingDispatcherAccessor() => factory.GetService<EventDispatcher>();
                return (Func<IEventDispatcher>) UnderlyingDispatcherAccessor;
            });
            services.Decorate<IEventDispatcher>((inner, provider) =>
                new CaraBusEventDispatcher(provider.GetRequiredService<ICaraBus>()));
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
