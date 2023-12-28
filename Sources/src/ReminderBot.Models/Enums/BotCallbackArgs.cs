namespace ReminderBot.Models.Enums
{
    public enum BotCallbackArgs
    {
        None,

        //SettingsHandler
        SettingsHandler_Language,
        SettingsHandler_Location,
        SettingsHandler_AutoDelete,
        SettingsHandler_Postpone,
        SettingsHandler_DateFormat,
        SettingsHandler_DateFormatValue,

        //StopHandler
        StopHandler_Stop,
        StopHandler_Stop_Erase,

        //ViewHandler
        ViewHandler_ViewActive,
        ViewHandler_ExportAll,
        ViewHandler_First,
        ViewHandler_Prev,
        ViewHandler_GoTo,
        ViewHandler_Next,
        ViewHandler_Last,

        //EditHandler
        EditHandler_View,
        EditHandler_Edit,
        EditHandler_Delete,

        //CreateHandler
        CreateHandler_Date_Correct,
        CreateHandler_Date_Incorrect,

        //ReminderHandler
        ReminderHandler_Postpone,
        ReminderHandler_Confirm
    }
}
