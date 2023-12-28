using System.Threading;
using System.Threading.Tasks;

namespace ReminderBot.BL.Interfaces
{
    public interface IBotService
    {
        void Init();
        ValueTask StartAsync(CancellationToken cancellationToken);
    }
}
