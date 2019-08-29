using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolicyBasedAuthorization.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolicyBasedAuthorization.AuthorizationRequirement;
using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuthorization.Handlers;

namespace PolicyBasedAuthorization
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Inject Application Options
            services.Configure<ApplicationOptions>(Configuration.GetSection("ApplicationOptions"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var applicationOptions = Configuration
                .GetSection("ApplicationOptions")
                .Get<ApplicationOptions>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RestrictIP", policy =>
                    policy.Requirements.Add(new IPRequirement(applicationOptions)));

                options.AddPolicy("DomainValidator", policy =>
                    policy.Requirements.Add(new InternalUserRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, IPAddressHandler>();
            services.AddSingleton<IAuthorizationHandler, EmailDomainHandler>();
            services.AddSingleton<IAuthorizationHandler, ExcludeContractorHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            CreateRoles(serviceProvider).Wait();
        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            //initializing custom roles   
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string[] roleNames = { "Admin", "User", "Contractor" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    //create the roles and seed them to the database: Question 1  
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            IdentityUser user = await UserManager.FindByEmailAsync("admin@blogofpi.com");

            if (user == null)
            {
                user = new IdentityUser()
                {
                    UserName = "admin@blogofpi.com",
                    Email = "admin@blogofpi.com",
                };
                await UserManager.CreateAsync(user, "Test@123");
            }
            await UserManager.AddToRoleAsync(user, "Admin");


            IdentityUser user1 = await UserManager.FindByEmailAsync("jane.doe@blogofpi.com");

            if (user1 == null)
            {
                user1 = new IdentityUser()
                {
                    UserName = "jane.doe@blogofpi.com",
                    Email = "jane.doe@blogofpi.com",
                };
                await UserManager.CreateAsync(user1, "Test@123");
            }
            await UserManager.AddToRoleAsync(user1, "User");

            IdentityUser user2 = await UserManager.FindByEmailAsync("testuser@gmail.com");

            if (user2 == null)
            {
                user2 = new IdentityUser()
                {
                    UserName = "testuser@gmail.com",
                    Email = "testuser@gmail.com",
                };
                await UserManager.CreateAsync(user2, "Test@123");
            }
            await UserManager.AddToRoleAsync(user2, "Contractor");

            IdentityUser user3 = await UserManager.FindByEmailAsync("retainerUser@gmail.com");

            if (user3 == null)
            {
                user3 = new IdentityUser()
                {
                    UserName = "retainerUser@gmail.com",
                    Email = "retainerUser@gmail.com",
                };
                await UserManager.CreateAsync(user3, "Test@123");
            }
            await UserManager.AddToRoleAsync(user3, "User");

        }
    }
}
