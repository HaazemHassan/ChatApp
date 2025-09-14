using ChatApi.Core.Bases;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ChatApi.Core {
    public static class CoreDependencyRegisteration {
        public static IServiceCollection CoreServicesRegisteration(this IServiceCollection services, IConfiguration configuration) {

            // Replace this line:
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // With this line:
            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
            //services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            //fluent validaion services
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
