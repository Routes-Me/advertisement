using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Helper.Repository;
using AdvertisementService.Models.Common;
using AdvertisementService.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            services.AddDbContext<AdvertisementService.Models.DBModels.advertisementserviceContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IAdvertisementsRepository, AdvertisementsRepository>();
            services.AddScoped<ICampaignsRepository, CampaignsRepository>();
            services.AddScoped<IMediasRepository, MediasRepository>();
            services.AddScoped<IIntervalsRepository, IntervalsRepository>();
            services.AddScoped<IIncludeAdvertisementsRepository, IncludeAdvertisementsRepository>();

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            var dependenciessSection = Configuration.GetSection("Dependencies");
            services.Configure<Dependencies>(dependenciessSection);

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
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
