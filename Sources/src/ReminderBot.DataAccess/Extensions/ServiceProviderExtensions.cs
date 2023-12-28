using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.Shared;

namespace ReminderBot.DataAccess.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void AddDbContexts(this IServiceCollection services)
        {
            services.AddDbContextFactory<DataContext>(options => options.UseSqlite(SystemConstants.DatabaseName), ServiceLifetime.Transient);
        }
    }
}
