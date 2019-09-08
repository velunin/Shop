using System;
using AutoMapper;
using MassInstance.ServiceCollection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Catalog.DataAccess;
using Shop.Catalog.DataAccess.QueryHandlers;

namespace Shop.Catalog.ServiceEndpoint
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

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<CatalogDbContext>(
                    (provider, builder) =>
                        builder
                            .UseSqlServer(Configuration.GetConnectionString("ShopConnection"))
                            .UseInternalServiceProvider(provider),
                    ServiceLifetime.Transient);

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddCqrs();
            services.AddCommandAndQueryHandlers(
                typeof(GetAllProductsHandler).Assembly,
                ServiceLifetime.Transient);

            return services.BuildServiceProvider();
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
