using FluentValidation;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MindMapGenerator.Core.Dtos.AuthenticationDto;
using MindMapGenerator.Core.Dtos.ExternalDto;
using MindMapGenerator.Core.HttpClients;
using MindMapGenerator.Core.MappingProfile;
using MindMapGenerator.Core.ServiceContracts;
using MindMapGenerator.Core.Services;

namespace MindMapGenerator.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCore(this IServiceCollection services , IConfiguration configuration)
        {
            services.AddValidatorsFromAssemblyContaining<LoginDTO>();
            services.AddAutoMapper(typeof(DiagramMappingProfile).Assembly);
            services.AddScoped<IAuthenticationServices, AuthenticationServices>();
            services.AddScoped<IDiagramService, DiagramService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.Configure<DeepSeekSettings>(configuration.GetSection("DeepSeek"));
            services.AddHttpClient<IDeepSeekService, DeepSeekService>();

            return services;
        }
    }
}
