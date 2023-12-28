namespace ReminderBot.BL.Interfaces
{
    public interface ILocalizer
    {
        string GetString(string key, string languageCode, params object[] args);
    }
}
