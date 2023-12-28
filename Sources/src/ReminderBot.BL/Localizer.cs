using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Localization;
using ReminderBot.BL.Interfaces;
using ReminderBot.Shared;
using ReminderBot.Shared.Resources;

namespace ReminderBot.BL
{
    public class Localizer : ILocalizer
    {
        private readonly IStringLocalizerFactory _stringLocalizerFactory;
        private readonly IStringLocalizer _stringLocalizer;

        public Localizer(IStringLocalizerFactory stringLocalizerFactory)
        {
            _stringLocalizerFactory = stringLocalizerFactory;

            _stringLocalizer = GetStringLocalizer();
        }

        private static readonly Dictionary<string, CultureInfo> _supportedCultures = new()
        {
            { SystemConstants.EnLoc, new CultureInfo(SystemConstants.EnLoc) },
            { SystemConstants.RuLoc, new CultureInfo(SystemConstants.RuLoc) },
            { SystemConstants.BeLoc, new CultureInfo(SystemConstants.BeLoc) },
            { SystemConstants.UkLoc, new CultureInfo(SystemConstants.UkLoc) }
        };

        private static readonly object _lock = new object();

        public string GetString(string key, string languageCode, params object[] args)
        {
            lock (_lock)
            {
                var currentCulture = CultureInfo.CurrentUICulture;

                try
                {
                    CultureInfo.CurrentUICulture = _supportedCultures.ContainsKey(languageCode)
                        ? _supportedCultures[languageCode]
                        : _supportedCultures[SystemConstants.EnLoc];

                    return _stringLocalizer.GetString(key, args);
                }
                finally
                {
                    CultureInfo.CurrentUICulture = currentCulture;
                }
            }
        }

        private IStringLocalizer GetStringLocalizer()
        {
            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName!);
            return _stringLocalizerFactory.Create("SharedResource", assemblyName.Name!);
        }
    }
}
