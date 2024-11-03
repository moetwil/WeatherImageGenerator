namespace JobProcessorFunction.DTOs;

public class WeatherDataDTO
{
    public BuienradarDTO Buienradar { get; set; }
    public ActualDTO Actual { get; set; }
}

public class BuienradarDTO
{
    public string Copyright { get; set; }
    public string Terms { get; set; }
}

public class ActualDTO
{
    public string ActualRadarUrl { get; set; }
    public string Sunrise { get; set; }
    public string Sunset { get; set; }
    public List<StationMeasurementDTO> StationMeasurements { get; set; }
}

public class StationMeasurementDTO
{
    public int StationId { get; set; }
    public string StationName { get; set; }
    public string Regio { get; set; }
    public string Timestamp { get; set; }
    public string WeatherDescription { get; set; }
    public double Temperature { get; set; }
    public double GroundTemperature { get; set; }
    public double FeelTemperature { get; set; }
    public double Humidity { get; set; }
    public double Precipitation { get; set; }
    public double SunPower { get; set; }
    public double RainFallLast24Hour { get; set; }
    public double RainFallLastHour { get; set; }
    public double WindDirectionDegrees { get; set; }
}