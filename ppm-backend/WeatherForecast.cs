using Microsoft.AspNetCore.Mvc;

namespace production_process_management_app.Server
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
}
