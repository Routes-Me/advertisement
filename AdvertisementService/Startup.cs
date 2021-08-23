using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Helper.Repository;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Internal.Repository;
using AdvertisementService.Internal.Abstraction;
using AdvertisementService.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisementService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc().AddNewtonsoftJson();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddDbContext<AdvertisementContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddScoped<IAdvertisementsRepository, AdvertisementsRepository>();
            services.AddScoped<IAdvertisementsReportRepository, AdvertisementsReportRepository>();
            services.AddScoped<ICampaignsRepository, CampaignsRepository>();
            services.AddScoped<IMediasRepository, MediasRepository>();
            services.AddScoped<IIntervalsRepository, IntervalsRepository>();
            services.AddScoped<IIncludeAdvertisementsRepository, IncludeAdvertisementsRepository>();
            services.AddScoped<IVideoConversionRepository, VideoConversionRepository>();
            services.AddScoped<ICommonFunctions, CommonFunctions>();

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            var dependenciessSection = Configuration.GetSection("Dependencies");
            services.Configure<Dependencies>(dependenciessSection);

            var azureConfigSection = Configuration.GetSection("AzureStorageBlobConfig");
            services.Configure<AzureStorageBlobConfig>(azureConfigSection);
            var azureConfig = azureConfigSection.Get<AzureStorageBlobConfig>();

            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });
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
            // Add Static Files Middleware  
            app.UseStaticFiles();
            app.UseAuthorization();
            app.UseCors("MyPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
