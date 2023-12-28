using System.Text;
using System.Text.Json;

namespace ReminderBot.Shared.Helpers
{
    public static class JsonSettings
    {
        public static JsonSerializerOptions Default { get; set; }

        static JsonSettings() => Default = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public static class JsonConvertHelper
    {
        public static string GetJson(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static byte[] GetJsonBytes(object obj)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
        }

        public static T? GetObject<T>(string str)
        {
            return JsonSerializer.Deserialize<T>(str, JsonSettings.Default);
        }

        public static T? GetObject<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes), JsonSettings.Default);
        }
    }
}
