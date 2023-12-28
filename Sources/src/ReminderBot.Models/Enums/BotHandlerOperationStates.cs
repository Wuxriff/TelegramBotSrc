namespace ReminderBot.Models.Enums
{
    public enum BotHandlerOperationStates
    {
        None,

        //SettingsHandler
        Settings_Waiting,
        Settings_Language_Waiting,
        Settings_Location_Waiting,
        Settings_AutoDelete_Waiting,
        Settings_Postpone_Waiting,
        Settings_DateFormat_Waiting,

        //ManageHandler
        Manage_Waiting,
        Manage_View_Waiting,

        //StopHandler
        Stop_Waiting,

        //CreateHandler
        Create_Waiting_Date,
        Create_Waiting_Date_Confirm,
        Create_Waiting_Text,

        //EditHandler
        Edit_View,

        //AdminHandler
        Admin_Waiting
    }
}
