using AintBnB.BusinessLogic.Imp;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Imp;
using AintBnB.Repository.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AintBnB.BlazorWASM.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var conString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DatabaseContext>(
                options => options.UseSqlServer(conString));

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IAccommodationService, AccommodationService>();
            services.AddTransient<IBookingService, BookingService>();
            services.AddTransient<IDeletionService, DeletionService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IImageService, ImageService>();

            services.AddAuthentication("Cookies")
                 .AddCookie(options =>
                 {
                     options.Cookie.Name = "myCoockie";
                 });

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "X-CSRF-TOKEN-COOKIE";
            });

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.Use(next => context =>
            {
                var antiforgery = context.RequestServices.GetService<IAntiforgery>();
                if (context.Request.Method.Equals("Get", StringComparison.OrdinalIgnoreCase))
                {
                    var tokens = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
                }
                return next(context);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
