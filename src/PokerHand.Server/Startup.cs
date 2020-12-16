using System;
using AutoMapper;
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
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<Player, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();
            
            services.AddSignalR();

            services.AddScoped<IGameProcessManager, GameProcessManager>();
            services.AddScoped<IGameService, GameService>();
            
            services.AddSingleton<TablesCollection>();
            
            services.AddAutoMapper(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/game");
                
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("hello");
                });
            });
        }
    }
}
