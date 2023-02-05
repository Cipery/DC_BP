using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace User.Test.Component.Utils;

public static class ServiceCollectionExtensions
{
    public static void ReplaceService<TOriginalService>(this IServiceCollection serviceCollection,
        object replacementServiceImplementation)
    {
        if (serviceCollection == null)
        {
            throw new ArgumentNullException(nameof(serviceCollection));
        }

        var originalServiceDescriptors = serviceCollection.Where(
                d => d.ServiceType ==
                     typeof(TOriginalService))
            .ToList();

        foreach (var serviceDescriptor in originalServiceDescriptors)
        {
            serviceCollection.Remove(serviceDescriptor);
        }

        serviceCollection.AddSingleton(typeof(TOriginalService), replacementServiceImplementation);
    }

    public static void RemoveDbContext<TDbContext>(this IServiceCollection serviceCollection)
        where TDbContext : DbContext
    {
        if (serviceCollection == null)
        {
            throw new ArgumentNullException(nameof(serviceCollection));
        }

        var context = serviceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TDbContext));
        if (context != null)
        {
            serviceCollection.Remove(context);
            var options = serviceCollection.Where(r => (r.ServiceType == typeof(DbContextOptions))
                                                       || (r.ServiceType.IsGenericType && r.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))).ToArray();
            foreach (var option in options)
            {
                serviceCollection.Remove(option);
            }
        }
    }
}