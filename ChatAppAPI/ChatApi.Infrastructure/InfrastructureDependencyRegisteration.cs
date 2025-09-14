using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Infrastructure.Data;
using ChatApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApi.Infrastructure {
    public static class InfrastructureDependencyRegisteration {

        public static IServiceCollection InfrastrctureServicesRegisteration(this IServiceCollection services, IConfiguration configuration) {

            DbContextServiceConfiguations(services, configuration);
            RepositoryServiceConfiguations(services);
            IdentityServiceConfiguations(services);
            return services;

        }


        private static IServiceCollection DbContextServiceConfiguations(IServiceCollection services, IConfiguration configuration) {
            services.AddDbContext<AppDbContext>(options => {
                options.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);
            });

            return services;
        }


        private static IServiceCollection IdentityServiceConfiguations(IServiceCollection services) {

            services.AddIdentity<ApplicationUser, ApplicationRole>(option => {
                // Password settings.
                option.Password.RequireDigit = false;
                option.Password.RequireLowercase = false;
                option.Password.RequireNonAlphanumeric = false;
                option.Password.RequireUppercase = false;
                option.Password.RequiredLength = 3;
                option.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                option.Lockout.MaxFailedAccessAttempts = 5;
                option.Lockout.AllowedForNewUsers = true;

                // User settings.
                option.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                option.User.RequireUniqueEmail = true;
                option.SignIn.RequireConfirmedEmail = false;


            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            return services;
        }


        private static IServiceCollection RepositoryServiceConfiguations(this IServiceCollection services) {
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();
            
            // Chat repositories
            services.AddTransient<IConversationRepository, ConversationRepository>();
            services.AddTransient<IMessageRepository, MessageRepository>();
            services.AddTransient<IConversationParticipantRepository, ConversationParticipantRepository>();
            services.AddTransient<IUserConnectionRepository, UserConnectionRepository>();
            services.AddTransient<IConnectionGroupRepository, ConnectionGroupRepository>();
            services.AddTransient<ITypingIndicatorRepository, TypingIndicatorRepository>();
            services.AddTransient<IMessageDeliveryRepository, MessageDeliveryRepository>();

            return services;
        }
    }
}
