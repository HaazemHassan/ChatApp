using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApi.Services {
    public static class ServicesDependencyRegisteration {
        public static IServiceCollection RegisterServicesDependcies(this IServiceCollection services, IConfiguration configuration) {
            services.AddTransient<IApplicationUserService, ApplicationUserService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            
            // Chat services
            services.AddTransient<IMessagesService, MessagesService>();
            services.AddTransient<IConversationsService, ConversationsService>();
            services.AddTransient<IConnectionService, ConnectionService>();
            
            // Current user service
            services.AddTransient<ICurrentUserService, CurrentUserService>();
            
            return services;
        }
    }
}
