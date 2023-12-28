using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime.TimeZones;
using Xunit;

namespace ReminderBot.Tests.Snippets
{
    //https://stackoverflow.com/questions/19695439/get-the-default-timezone-for-a-country-via-cultureinfo
    //https://stackoverflow.com/questions/27708673/how-do-i-convert-iana-zone-datetime-to-utc-offset-using-nodatime

    public class NodaTimeLocationDetectionTest
    {
        [Fact]
        public void Test()
        {
            var countryName = "Russia";
            var longitude = 40.332206;

            var zones = TzdbDateTimeZoneSource.Default.ZoneLocations.Where(x => x.CountryName == countryName).AsQueryable();
            if (!double.IsNaN(longitude))
            {
                zones = zones.OrderBy(o => this.Distance(o.Latitude, longitude, o.Latitude, o.Longitude, DistanceUnit.Kilometer));
            }
            var bestZone = zones.FirstOrDefault();
            var dateTimeZone = TzdbDateTimeZoneSource.Default.ForId(bestZone.ZoneId);

            var newTime = DateTime.UtcNow.AddSeconds(dateTimeZone.MaxOffset.Seconds);

        }

        public enum DistanceUnit { StatuteMile, Kilometer, NauticalMile };

        private double Distance(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit)
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case DistanceUnit.Kilometer:
                    return dist * 1.609344;
                case DistanceUnit.NauticalMile:
                    return dist * 0.8684;
                default:
                case DistanceUnit.StatuteMile: //Miles
                    return dist;
            }
        }
    }
}
