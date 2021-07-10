using DelusionalApi.Model.Bots;
using DelusionalApi.Service;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text.Json.Serialization;

namespace DelusionalApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            AppSetttings = configuration.Get<AppSetttings>();
        }

        public AppSetttings AppSetttings { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                 .AddJsonOptions(x =>
                 {
                     x.JsonSerializerOptions.WriteIndented = true;
                     x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                 });


            services.AddSwaggerGen(c =>
             {
                 c.UseInlineDefinitionsForEnums();

                 var filePath = Path.Combine(System.AppContext.BaseDirectory, "DelusionalApi.xml");
                 c.IncludeXmlComments(filePath);
             });

            services.AddScoped<IConceptGraphDb, ConceptGraphDb>();
            services.AddSingleton<IAssociationFormatter, AssociationFormatter>();
            services.AddSingleton(AppSetttings);
            services.AddSingleton<IDelusionDictionary, DelusionDictionary>();
            services.AddSingleton<ISpeechService, CustomVoiceService>();

            services.AddSingleton<BotScriptService>();


            services.AddHttpContextAccessor();


            var provider = services.BuildServiceProvider();

            GlobalConfiguration.Configuration.UseSQLiteStorage();
            services.AddHangfireServer();

            services.AddHangfire(c => c.UseSQLiteStorage());

            services.AddMemoryCache();
            
            var botScriptService = provider.GetRequiredService<BotScriptService>();

            var ren = new RenBot();

            RecurringJob.AddOrUpdate("botScriptGeneration", () => botScriptService.PrepareBotScripts(ren, 3), Cron.Minutely());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
                
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.Full);
                c.RoutePrefix = string.Empty;
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Scripts")),
                RequestPath = "/Scripts"
            });

            app.UseHangfireDashboard();

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