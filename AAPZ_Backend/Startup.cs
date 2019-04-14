using BLL.Services;
using BLL.SignalR;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Threading.Tasks;

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
            services.AddIdentity<User, IdentityRole>()
                 .AddEntityFrameworkStores<AAPZ_BackendContext>()
                 .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            });
            //services.AddSingleton<> // add BLL Utils
            services.AddDbContext<AAPZ_BackendContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Singleton);

            services.AddSingleton<StreamingLogic>();
            services.AddSingleton<Statistics>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
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
            CreateUserRoles(services).Wait();
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<User>>();

            IdentityResult roleResult;

            var roleCheck = await RoleManager.RoleExistsAsync("Manager");
            if (!roleCheck)
            {
                roleResult = await RoleManager.CreateAsync(new IdentityRole("Manager"));
            }

            roleCheck = await RoleManager.RoleExistsAsync("Driver");
            if (!roleCheck)
            {
                roleResult = await RoleManager.CreateAsync(new IdentityRole("Driver"));
            }
            //Assign Admin role to the main User here we have given our newly registered 
            //login id for Admin management
            //User user = await UserManager.FindByEmailAsync("syedshanumcain@gmail.com");
            //var User = new User();
            //await UserManager.AddToRoleAsync(user, "Admin");
        }
    }
}
