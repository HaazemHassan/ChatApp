using ChatApi.Bases.DataSeeding;
using ChatApi.Core;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Middlewares;
using ChatApi.Extentions;
using ChatApi.Hubs;
using ChatApi.Infrastructure;
using ChatApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

namespace ChatApi {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.InfrastrctureServicesRegisteration(builder.Configuration)
                            .CoreServicesRegisteration(builder.Configuration)
                            .RegisterServicesDependcies(builder.Configuration)
                            .ConfigureServices(builder.Configuration);




            var myOrigin = "AngularChatApp";
            builder.Services.AddCors(options => {
                options.AddPolicy(name: myOrigin, policy => {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials() //this line is important for SignalR
                          .WithOrigins("http://localhost:4200");
                });
            });

            builder.Services.AddSignalR();

            var app = builder.Build();

            app.MapHub<ChatHub>("/chatHub");


            #region Seed Data
            using (var scope = app.Services.CreateScope()) {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                await ApplicationRoleSeeder.SeedAsync(roleManager);
                await ApplicationUserSeeder.SeedAsync(userManager);
            }
            #endregion

            if (app.Environment.IsDevelopment()) {
                // Temporary workaround: Downgrade Swagger to OpenAPI 2.0
                // Reason: Package -> Swashbuckle.AspNetCore.Annotations (v8.1.1) is not fully compatible with OpenAPI 3.x
                app.UseSwagger(c => {
                    c.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
                });
                app.UseSwaggerUI();
            }
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();
            app.UseCors(myOrigin);



            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}