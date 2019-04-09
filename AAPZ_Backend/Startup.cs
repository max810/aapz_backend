using BLL.Services;
using BLL.SignalR;
using DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace BLL
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
            services.AddCors();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "ACCR CURL", Version = "v1" });
            });
            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            //services.AddSingleton<IConnectionManagerThreadSafe<string>, ConnectionManagerThreadSafe<string>>();
            services.AddIdentity<IdentityUser, IdentityRole>()
                 .AddEntityFrameworkStores<AAPZ_BackendContext>()
                 .AddDefaultTokenProviders();
            //services.AddSingleton<> // add BLL Utils
            services.AddDbContext<AAPZ_BackendContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Singleton);

            services.AddSingleton<StreamingLogic>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();
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

            //app.UseHttpsRedirection();
            app.UseCors(config =>
               config
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowAnyOrigin()
               .AllowCredentials()
               );

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CURL v1");
                c.RoutePrefix = string.Empty;
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<StreamHub>("/streamHub");
            });

            app.UseMvc();
        }
    }
}
