using Chic.Abstractions;
using Chic.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Chic.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddChic(this IServiceCollection services)
        {
            // Core (Don't do anything here if Provisioner is moving)
            services.AddSingleton<IModelMetadataProvider, DefaultModelMetadataProvider>();

            // Typed
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }
    }
}
