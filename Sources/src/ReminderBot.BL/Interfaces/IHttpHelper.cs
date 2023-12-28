using System.Threading;
using System.Threading.Tasks;

namespace ReminderBot.BL.Interfaces
{
    public interface IHttpHelper
    {
        Task<T> GetAsync<T>(string query, string? externalAccessToken = null, CancellationToken token = default);
        Task<T> PostAsync<T>(string query, object @object, string? externalAccessToken = null, CancellationToken token = default);
    }
}
