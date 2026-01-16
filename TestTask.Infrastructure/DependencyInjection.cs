using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestTask.Infrastructure.Data;

namespace TestTask.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDbInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(connectionString,
                    builder => builder.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
        );

        return services;
    }
}