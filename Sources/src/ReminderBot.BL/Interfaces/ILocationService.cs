using System.Threading.Tasks;
using ReminderBot.Models.Models;

namespace ReminderBot.BL.Interfaces
{
    public interface ILocationService
    {
        Task<GeonamesTimeZone?> GetTimeZoneAsync(double lat, double lng);
        Task<UserTimeZone?> GetUserTimeZoneAsync(double lat, double lng);
        UserTimeZone? GetUserTimeZone(double lat, double lng);
    }
}
