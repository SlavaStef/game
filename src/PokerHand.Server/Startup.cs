using System;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.BusinessLogic.Services;
using PokerHand.Common;
using PokerHand.Common.Entities;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;
using PokerHand.DataAccess.Repositories;
using PokerHand.Server.Helpers;
using PokerHand.Server.Hubs;

namespace PokerHand.Server
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("PostgreSQLConnection")));

            services.AddIdentity<Player, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "931980355852-kl59do3oc6vft9rp80n2et8qhvs0sjm9.apps.googleusercontent.com";
                    options.ClientSecret = "mfnNBGBHl7ZV21ZN8-qjAJae";
                    
                    options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                    options.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
                    options.SaveTokens = true;
                });
            
            services.AddSignalR();

            services.AddTransient<IGameProcessManager, GameProcessManager>();
            services.AddTransient<IPlayerService, PlayerService>();
            services.AddTransient<ITableService, TableService>();
            services.AddTransient<IChatService, ChatService>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            services.AddSingleton<TablesCollection>();
            services.AddSingleton<PlayersOnline>();
            
            services.AddAutoMapper(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            // using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            // {
            //     var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationContext>();
            //     context.Database.Migrate();
            // }
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/game");
                
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Is working");
                });
            });
        }
    }
}
