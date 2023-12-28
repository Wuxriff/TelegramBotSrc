using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GeoTimeZone;
using Microsoft.Extensions.Logging;
using NodaTime.TimeZones;
using ReminderBot.BL.Interfaces;
using ReminderBot.Models.Models;
using ReminderBot.Shared;
using TimeZoneConverter;

namespace ReminderBot.BL
{
    public class LocationService : ILocationService
    {
        private readonly IHttpHelper _httpHelper;
        private readonly IBotConfigurationService _botConfigurationService;
        private readonly ILogger<LocationService> _logger;

        public LocationService(IHttpHelper httpHelper, IBotConfigurationService botConfigurationService, ILogger<LocationService> logger)
        {
            _httpHelper = httpHelper;
            _botConfigurationService = botConfigurationService;
            _logger = logger;
        }

        public async Task<GeonamesTimeZone?> GetTimeZoneAsync(double lat, double lng)
        {
            try
            {
                var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

                var query = string.Format(ExternalApiConstants.GeoNamesTimeZone, lat.ToString(nfi), lng.ToString(nfi), _botConfigurationService.GetGeonamesToken());

                var result = await _httpHelper.GetAsync<GeonamesTimeZone>(query);

                if (string.IsNullOrWhiteSpace(result.TimezoneId))
                {
                    return null;
                }

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError("GetTimeZoneAsync failed: {0}", ex);
                return null;
            }
        }

        public async Task<UserTimeZone?> GetUserTimeZoneAsync(double lat, double lng)
        {
            try
            {
                var result = await GetTimeZoneAsync(lat, lng);

                if (result == null)
                {
                    return null;
                }

                var tzi = TimeZoneInfo.FindSystemTimeZoneById(result.TimezoneId!);

                return new UserTimeZone { TimeZoneInfo = tzi, TimeZoneId = result.TimezoneId!, CountryName = result.CountryName, CountryCode = result.CountryCode };
            }
            catch (Exception ex)
            {

                _logger.LogError("GetUserTimeZoneAsync failed: {0}", ex);
                return null;
            }
        }

        public UserTimeZone? GetUserTimeZone(double lat, double lng)
        {
            try
            {
                var result = TimeZoneLookup.GetTimeZone(lat, lng);
                var tzi = TZConvert.GetTimeZoneInfo(result.Result);
                var tzdbSource = TzdbDateTimeZoneSource.Default;
                var zoneLocation = tzdbSource.ZoneLocations?.FirstOrDefault(x => x.ZoneId == result.Result);

                return new UserTimeZone { TimeZoneInfo = tzi, TimeZoneId = result.Result, CountryName = zoneLocation?.CountryName, CountryCode = zoneLocation?.CountryCode };
            }
            catch (Exception ex)
            {
                _logger.LogError("GetUserTimeZone failed: {0}", ex);
                return null;
            }
        }
    }
}
