using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Helper.Repository;
using AdvertisementService.Models.Common;
using AdvertisementService.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvertisementService
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
            services.AddControllers();
            services.AddMvc().AddNewtonsoftJson();

            services.AddDbContext<AdvertisementService.Models.DBModels.advertisementserviceContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.WithOrigins("http://localhost:56411"));
            });

            services.AddScoped<IAdvertisementsRepository, AdvertisementsRepository>();
            services.AddScoped<ICampaignsRepository, CampaignsRepository>();
            services.AddScoped<IMediasRepository, MediasRepository>();
            services.AddScoped<IIntervalsRepository, IntervalsRepository>();
            services.AddScoped<IIncludeAdvertisementsRepository, IncludeAdvertisementsRepository>();
            services.AddScoped<IIncludeQRCodeRepository, IncludeQRCodeRepository>();

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();
            
            var azureConfigSection = Configuration.GetSection("AzureStorageBlobConfig");
            services.Configure<AzureStorageBlobConfig>(azureConfigSection);
            var azureConfig = azureConfigSection.Get<AzureStorageBlobConfig>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors(options => options.WithOrigins("http://localhost:56411"));
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
