using ReminderBot.BL.StateHandlers;

namespace ReminderBot.BL.Interfaces
{
    public interface IBotHandlerManagerService
    {
        void AddOrUpdateHandler(long userId, BaseStateHandler handler);
        bool TryGetHandler(long userId, out BaseStateHandler handler);
    }
}
