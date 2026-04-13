using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace WeatherAlertAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly IConfiguration _config;

        public WeatherController(IConfiguration config) => _config = config;

        // Old endpoint (kept for compatibility)
        // GET: /api/weather/alerts?city=Colombo
        [HttpGet("alerts")]
        public async Task<IActionResult> GetWeatherAlerts([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City is required.");

            var apiKey = _config["OpenWeatherMap:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return StatusCode(500, "Server is missing OpenWeatherMap API key.");

            try
            {
                var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
                var weatherResponse = await client.GetStringAsync(url);
                var weatherData = JObject.Parse(weatherResponse);

                string? alert = null;
                var weatherArray = weatherData["weather"] as JArray;
                if (weatherArray != null)
                {
                    foreach (var item in weatherArray)
                    {
                        var main = item?["main"]?.ToString();
                        if (main == "Thunderstorm")
                        {
                            alert = "Thunderstorm Warning!";
                            break;
                        }
                    }
                }

                double? temp = weatherData["main"]?["temp"]?.Value<double?>();

                return Ok(new { temp, alert });
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Error fetching weather data: " + e.Message);
            }
        }

        // NEW: rich details + 5-day forecast
        // GET: /api/weather/details?city=Colombo
        [HttpGet("details")]
        public async Task<IActionResult> GetWeatherDetails([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City is required.");

            var apiKey = _config["OpenWeatherMap:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return StatusCode(500, "Server is missing OpenWeatherMap API key.");

            try
            {
                // Current weather
                var currentUrl = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
                var currentResponse = await client.GetStringAsync(currentUrl);
                var current = JObject.Parse(currentResponse);

                // Forecast (3-hourly, next 5 days)
                var forecastUrl = $"http://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric";
                var forecastResponse = await client.GetStringAsync(forecastUrl);
                var forecast = JObject.Parse(forecastResponse);

                // Build "current" object
                var weather0 = current["weather"]?.FirstOrDefault();
                var sys = current["sys"];
                var coord = current["coord"];

                var currentDto = new
                {
                    city = current["name"]?.ToString(),
                    country = sys?["country"]?.ToString(),
                    coord = new
                    {
                        lat = coord?["lat"]?.Value<double?>(),
                        lon = coord?["lon"]?.Value<double?>()
                    },
                    main = weather0?["main"]?.ToString(),                    // e.g., Clear, Rain
                    description = ToTitle(weather0?["description"]?.ToString()),
                    icon = weather0?["icon"]?.ToString(),                    // e.g., 10d
                    temp = current["main"]?["temp"]?.Value<double?>(),
                    feels_like = current["main"]?["feels_like"]?.Value<double?>(),
                    humidity = current["main"]?["humidity"]?.Value<int?>(),
                    pressure = current["main"]?["pressure"]?.Value<int?>(),
                    wind_speed = current["wind"]?["speed"]?.Value<double?>(),
                    wind_deg = current["wind"]?["deg"]?.Value<int?>(),
                    cloudiness = current["clouds"]?["all"]?.Value<int?>(),   // %
                    visibility = current["visibility"]?.Value<int?>(),       // meters
                    sunrise = UnixToIso(sys?["sunrise"]?.Value<long?>(), current["timezone"]?.Value<int?>()),
                    sunset = UnixToIso(sys?["sunset"]?.Value<long?>(), current["timezone"]?.Value<int?>()),
                    soilMoisture = (double?)null // Placeholder: requires separate API/plan
                };

                // Basic alert heuristics
                var alerts = new List<string>();
                var wMain = currentDto.main ?? "";
                if (wMain.Equals("Thunderstorm", StringComparison.OrdinalIgnoreCase)) alerts.Add("Thunderstorm Warning");
                if (wMain.Equals("Rain", StringComparison.OrdinalIgnoreCase)) alerts.Add("Rain Advisory");
                if (wMain.Equals("Snow", StringComparison.OrdinalIgnoreCase)) alerts.Add("Snow Advisory");
                if ((currentDto.wind_speed ?? 0) >= 14) alerts.Add("Wind Advisory"); // ~> 50 km/h
                if ((currentDto.temp ?? 0) >= 37) alerts.Add("Heat Advisory");
                if ((currentDto.temp ?? 0) <= 0) alerts.Add("Freeze Advisory");

                // Aggregate 5-day daily forecast
                var list = forecast["list"] as JArray ?? new JArray();
                var groups = list
                    .Select(item =>
                    {
                        var dt = UnixToDateTime(item["dt"]?.Value<long?>(), forecast["city"]?["timezone"]?.Value<int?>());
                        return new
                        {
                            date = dt.Date,
                            temp_min = item["main"]?["temp_min"]?.Value<double?>(),
                            temp_max = item["main"]?["temp_max"]?.Value<double?>(),
                            main = item["weather"]?.FirstOrDefault()?["main"]?.ToString(),
                            description = item["weather"]?.FirstOrDefault()?["description"]?.ToString(),
                            icon = item["weather"]?.FirstOrDefault()?["icon"]?.ToString(),
                            pop = item["pop"]?.Value<double?>() // precipitation probability
                        };
                    })
                    .Where(x => x != null);

                var daily = groups
                    .GroupBy(g => g.date)
                    .Take(5)
                    .Select(g =>
                    {
                        var most = g
                            .GroupBy(x => x.main)
                            .OrderByDescending(grp => grp.Count())
                            .FirstOrDefault()?.FirstOrDefault();

                        return new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            min = g.Min(x => x.temp_min),
                            max = g.Max(x => x.temp_max),
                            main = most?.main,
                            description = ToTitle(most?.description),
                            icon = most?.icon,
                            pop = Math.Round((g.Average(x => x.pop ?? 0) * 100.0), 0) // %
                        };
                    })
                    .ToList();

                var payload = new
                {
                    current = currentDto,
                    alerts = alerts,
                    forecast = daily
                };

                return Ok(payload);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Error contacting weather service: " + e.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Unexpected server error: " + ex.Message);
            }
        }

        private static string ToTitle(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s ?? "";
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s!);
        }

        private static string? UnixToIso(long? unix, int? tzOffset)
        {
            var dt = UnixToDateTime(unix, tzOffset);
            return dt == DateTime.MinValue ? null : dt.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        private static DateTime UnixToDateTime(long? unix, int? tzOffset)
        {
            if (unix == null) return DateTime.MinValue;
            var dt = DateTimeOffset.FromUnixTimeSeconds(unix.Value);
            if (tzOffset != null) dt = dt.ToOffset(TimeSpan.FromSeconds(tzOffset.Value));
            return dt.DateTime;
        }
    }
}
