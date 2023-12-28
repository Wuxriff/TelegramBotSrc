using ReminderBot.BL.StateHandlers;

namespace ReminderBot.BL
{
    public delegate BaseStateHandler HandlerResolver(string? command);
}
