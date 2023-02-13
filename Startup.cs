using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMethod;
using DinkToPdf;
using DinkToPdf.Contracts;
using GreenStop.API.CommonMethod;
using GreenStop.API.Data;
using GreenStop.API.Helpers;
using GreenStop.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GreenStop.API
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
            services.AddMemoryCache();
            services.AddCors(options=>{
                options.AddPolicy("EnableCORS",builder=>{
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build();//AllowCredentials().Build();
                });

            });
            services.AddScoped<TokenModel>();
            services.AddSingleton<ICommonMethods,CommonMethods>();
            services.AddSingleton<IJwtDecoder,JwtDecoder>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddDbContextPool<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddHttpClient();
            
            services.AddIdentity<ApplicationUser,IdentityRole>(options =>{
                options.Password.RequireDigit=true;
                options.Password.RequiredLength=6;
                options.Password.RequireNonAlphanumeric=true;
                options.Password.RequireLowercase=true;
                options.Password.RequireUppercase=true;
                options.User.RequireUniqueEmail=false;

                //Lockout settings
                options.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts=5;
                options.Lockout.AllowedForNewUsers=true;

            }

            ).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            // Configure strongly typed settings objects
            var appSettingsSection=Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            

            var appSettings=appSettingsSection.Get<AppSettings>();
            var key=Encoding.ASCII.GetBytes(appSettings.Secret);
            
            // services.AddHttpClient("notification",c=>{
            //     c.BaseAddress=new Uri(appSettings.NotificationServer);
            // });
            // Authenticaton Middelware
            services.AddAuthentication(o=>{
                o.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme=JwtBearerDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;  
            }).AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,options=>{
                options.TokenValidationParameters=new TokenValidationParameters{
                    ValidateIssuerSigningKey=true,
                    ValidateIssuer=true,
                    ValidateAudience=true,
                   // RequireExpirationTime=false,
                    ValidIssuer=appSettings.Site,
                    ValidAudience=appSettings.Audience,
                    IssuerSigningKey=new SymmetricSecurityKey(key),
                    ClockSkew=TimeSpan.Zero
                };
            });

            services.AddAuthorization(options=>{
                options.AddPolicy("RequireCustomerLoggedIn",policy=>policy.RequireRole("ITAdmin","Customer","CustomerService"));
                options.AddPolicy("RequireLoggedIn",policy=> policy.RequireRole("ITAdmin","Customer","OperationManager","Accounts","DeliveryManager","Kitchen","DeliveryAgents","CustomerService").RequireAuthenticatedUser());//.RequireClaim("abc"));
                options.AddPolicy("RequireAdministratorRole",policy=>policy.RequireRole("ITAdmin").RequireAuthenticatedUser());    
            });

            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("EnableCORS");

        //    app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
