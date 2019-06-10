using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.ApiKey.Providers;
using AspNet.Security.ApiKey.Providers.Events;
using AspNet.Security.ApiKey.Providers.Extensions;
using Bunq.Sdk.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace bunqShortcuts
{
    public class Startup
    {
        private const string API_KEY = "<< Your bunq API Key >>";
        private const string DEVICE_DESCRIPTION = "bunqShortcuts";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Init bunq API
            try {
                var apiContext = ApiContext.Restore();
                apiContext.Save();
            } catch (Bunq.Sdk.Exception.BunqException) {
                var apiContext = ApiContext.Create(ApiEnvironmentType.PRODUCTION, API_KEY, DEVICE_DESCRIPTION);
                apiContext.Save();
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = ApiKeyDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = ApiKeyDefaults.AuthenticationScheme;
            })
                .AddApiKey(options => {
                    options.Header = "Authorization";
                    options.HeaderKey = "ApiKey";
                    options.Events = new ApiKeyEvents {
                        OnAuthenticationFailed = context => {
                            context.Fail(context.Exception);
                            return Task.CompletedTask;
                        },
                        OnApiKeyValidated = context => {
                            if (context.ApiKey == "Super-secret-key") {
                                var identity = new ClaimsIdentity(new[] {
                                    new Claim(ClaimTypes.Name, "W. Janson")
                                });
                                context.Principal.AddIdentity(identity);
                                context.Success();
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
