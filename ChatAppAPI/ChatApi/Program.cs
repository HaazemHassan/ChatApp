using ChatApi.Bases.DataSeeding;
using ChatApi.Core;
using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Helpers;
using ChatApi.Core.Middlewares;
using ChatApi.Extentions;
using ChatApi.Hubs;
using ChatApi.Infrastructure;
using ChatApi.Infrastructure.Data;
using ChatApi.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Net;
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

            // Configure Forwarded Headers for Proxy/Docker/nginx
            builder.Services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });



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
                options.AddPolicy("defaultLimiter", httpContext => {
                    var user = httpContext.User?.Identity?.Name;
                    string partitionKey;
                    if (!string.IsNullOrEmpty(user)) {
                        partitionKey = user;
                    }
                    else {
                        partitionKey = HttpContextHelper.GetClientIpAddress(httpContext);
                    }

                    return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, key => new SlidingWindowRateLimiterOptions {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 90,
                        QueueLimit = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        SegmentsPerWindow = 4
                    });
                });

                options.AddPolicy("loginLimiter", httpContext => {
                    var partitionKey = HttpContextHelper.GetClientIpAddress(httpContext);

                    return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, key => new SlidingWindowRateLimiterOptions {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 5,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        SegmentsPerWindow = 4
                    });
                });

                options.OnRejected = async (context, token) => {
                    if (context.HttpContext.Response.HasStarted) {
                        return;
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    // Add Retry-After header
                    int retryAfterSeconds;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)) {
                        retryAfterSeconds = (int)retryAfter.TotalSeconds;
                    }
                    else {
                        var endpoint = context.HttpContext.GetEndpoint();
                        var rateLimiterAttribute = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();
                        var policyName = rateLimiterAttribute?.PolicyName;

                        retryAfterSeconds = policyName switch {
                            "loginLimiter" => 60,  // 1 minute
                            "defaultLimiter" => 60, // 1 minute
                            _ => 60
                        };
                    }
                    context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();


                    // Generic message - avoid information disclosure
                    var response = new Response<string> {
                        StatusCode = HttpStatusCode.TooManyRequests,
                        Message = "Too many requests. Please try again later.",
                        Succeeded = false
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: token);
                };

                // Global rejection behavior
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
            #endregion

            var app = builder.Build();

            app.MapHub<ChatHub>("/chatHub");


            #region Initialize Database
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

            app.UseErrorHandling();
            app.UseForwardedHeaders();   // Use Forwarded Headers (must be early in pipeline)
            app.UseSecurityHeaders();
            app.UseHttpsRedirection();
            app.UseCors(myOrigin);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();    //must be after UseAuthentication and UseAuthorization be cause we are using user identity name in rate limiting policy                                
            app.MapControllers();

            app.Run();
        }
    }
}