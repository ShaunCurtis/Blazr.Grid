namespace Blazr.Grid.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
        private List<WeatherForecast>? _forecasts;

        public async Task<IEnumerable<WeatherForecast>> GetForecastsAsync()
        {
            if (_forecasts is null)
                _forecasts = GetForecasts();
            await Task.Delay(100);
            return _forecasts;
        }


        private List<WeatherForecast> GetForecasts()
        {
            return Enumerable.Range(1, 20).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now).AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToList();
        }
    }
}