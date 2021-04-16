using System;
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

            services.AddIdentity<Player, IdentityRole<Guid>>(options =>
                {
                    options.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                        "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
                        "ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿ" +
                        "0123456789 \\-._@'\"";
                })
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            services.AddSignalR()
                .AddMessagePackProtocol();

            services.AddControllers();
            
            services.AddSwaggerGen();

            services.AddTransient<IPlayerService, PlayerService>();
            services.AddTransient<ITableService, TableService>();
            services.AddTransient<IBotService, BotService>();
            services.AddTransient<ICardEvaluationService, CardEvaluationService>();
            services.AddTransient<IMediaService, MediaService>();
            services.AddTransient<IDeckService, DeckService>();
            services.AddTransient<IGameProcessService, GameProcessService>();
            services.AddTransient<ILoginService, LoginService>();
            services.AddTransient<IPresentService, PresentService>();
            services.AddTransient<IMoneyBoxService, MoneyBoxService>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            services.AddSingleton<ITablesOnline, TablesOnline>();
            services.AddSingleton<IPlayersOnline, PlayersOnline>();
            
            services.AddAutoMapper(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/game");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Is working");
                });
            });
        }
    }
}
