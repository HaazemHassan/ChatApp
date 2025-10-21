using ChatApi.Core.Bases.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ChatApi.Extentions {
    public static class RegisterServices {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration) {
            AuthenticationServiceConfiguations(services, configuration);
            SwaggerServiceConfiguations(services, configuration);
            //EmailServiceConfiguations(services, configuration);
            AutorizationServiceConfiguations(services, configuration);
            return services;
        }

        private static IServiceCollection AuthenticationServiceConfiguations(this IServiceCollection services, IConfiguration configuration) {
            //JWT Authentication
            var jwtSettings = new JwtSettings();
            configuration.GetSection(nameof(jwtSettings)).Bind(jwtSettings);
            services.AddSingleton(jwtSettings);

            //Google Authentication
            var googleAuthSettings = new GoogleAuthSettings();
            configuration.GetSection("GoogleAuth").Bind(googleAuthSettings);
            services.AddSingleton(googleAuthSettings);

            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(x => {
               x.RequireHttpsMetadata = false;
               x.SaveToken = true;
               x.TokenValidationParameters = new TokenValidationParameters {
                   ValidateIssuer = jwtSettings.ValidateIssuer,
                   ValidIssuers = new[] { jwtSettings.Issuer },
                   ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                   ValidAudience = jwtSettings.Audience,
                   ValidateAudience = jwtSettings.ValidateAudience,
                   ValidateLifetime = jwtSettings.ValidateLifeTime,
               };


               x.Events = new JwtBearerEvents {
                   OnMessageReceived = context => {
                       var accessToken = context.Request.Query["access_token"];
                       var path = context.HttpContext.Request.Path;

                       if (!string.IsNullOrEmpty(accessToken) &&
                           path.StartsWithSegments("/chatHub")) {
                           context.Token = accessToken;
                       }

                       return Task.CompletedTask;
                   }
               };
           }



           );



            return services;
        }

        private static IServiceCollection SwaggerServiceConfiguations(this IServiceCollection services, IConfiguration configuration) {
            // Swagger Configuration
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Chat API",
                    Version = "v1"
                });

                options.EnableAnnotations();

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        //private static IServiceCollection EmailServiceConfiguations(this IServiceCollection services, IConfiguration configuration) {
        //    var emailSettings = new EmailSettings();
        //    configuration.GetSection("emailSettings").Bind(emailSettings);
        //    services.AddSingleton(emailSettings);
        //    return services;
        //}

        private static IServiceCollection AutorizationServiceConfiguations(this IServiceCollection services, IConfiguration configuration) {
            services.AddAuthorization(options => {
                options.AddPolicy("ResetPasswordPolicy", policy => {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("purpose", "reset-password");
                });
            });
            return services;
        }
    }
}
