using ChatApi.Bases.DataSeeding;
using ChatApi.Core;
using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Middlewares;
using ChatApi.Extentions;
using ChatApi.Hubs;
using ChatApi.Infrastructure;
using ChatApi.Infrastructure.Data;
using ChatApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

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
                          .WithOrigins(
                              "http://localhost:4200",      // Development (Angular CLI - ng serve)
                              "http://localhost:5000",      // Docker/Production (nginx)
                              "https://localhost:44318"     // Development (HTTPS)
                          );
                });
            });

            builder.Services.AddSignalR();

            #region RATE LIMITING CONFIGURATIONS
            builder.Services.AddRateLimiter(options => {
                options.AddSlidingWindowLimiter("defaultLimiter", opt => {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 100;
                    opt.QueueLimit = 10;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.SegmentsPerWindow = 4;
                });

                options.AddSlidingWindowLimiter("loginLimiter", opt => {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 5;
                    opt.QueueLimit = 0;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.SegmentsPerWindow = 4;
                });

                options.OnRejected = async (context, token) => {
                    var endpoint = context.HttpContext.GetEndpoint();
                    var rateLimiterAttribute = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();
                    var policyName = rateLimiterAttribute?.PolicyName;

                    context.HttpContext.Response.StatusCode = 429;

                    switch (policyName) {
                        case "loginLimiter":
                        await context.HttpContext.Response.WriteAsync(
                            "Too many login attempts. Please try again later.", token);
                        break;


                        default:
                        await context.HttpContext.Response.WriteAsync(
                            "Too many requests. Please try again later.", token);
                        break;
                    }
                };

            });
            #endregion

            var app = builder.Build();

            app.MapHub<ChatHub>("/chatHub");


            #region Configure Data
            using (var scope = app.Services.CreateScope()) {

                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Ensure database is created and apply all pending migrations
                try {
                    Console.WriteLine("Checking database connection...");
                    await context.Database.EnsureCreatedAsync(); // Create DB if not exists
                    Console.WriteLine("Applying migrations...");
                    await context.Database.MigrateAsync(); // Apply migrations
                    Console.WriteLine("Database ready!");
                }
                catch (SqlException ex) when (ex.Number == 2714) // Object already exists
                {
                    Console.WriteLine("Tables already exist, skipping migration");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Database migration error: {ex.Message}");
                    throw;
                }

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                var userConnectionRepository = scope.ServiceProvider.GetRequiredService<IUserConnectionRepository>();

                await ApplicationRoleSeeder.SeedAsync(roleManager);
                await ApplicationUserSeeder.SeedAsync(userManager);
                await UserConnectionsRemover.RemoveAsync(userConnectionRepository);
                await UserOnlineStatusResetter.ResetAsync(userManager);
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

            app.UseRateLimiter();
            app.MapControllers();

            app.Run();
        }
    }
}