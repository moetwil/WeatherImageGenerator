namespace ImageProcessorFunction.DTOs;

using System;

public class ImageInfoDTO
{
    public Guid JobId { get; set; }
    public StationDTO Station { get; set; }
    public string ImageUrl { get; set; }
}

public class StationDTO
{
    public int StationId { get; set; }
    public string StationName { get; set; }
    public string Regio { get; set; }
    public DateTime Timestamp { get; set; }
    public string WeatherDescription { get; set; }
    public string IconUrl { get; set; }
    public double Temperature { get; set; }
    public double GroundTemperature { get; set; }
    public double FeelTemperature { get; set; }
    public int Humidity { get; set; }
    public double Precipitation { get; set; }
    public int SunPower { get; set; }
    public double RainFallLast24Hour { get; set; }
    public double RainFallLastHour { get; set; }
    public int WindDirectionDegrees { get; set; }
}