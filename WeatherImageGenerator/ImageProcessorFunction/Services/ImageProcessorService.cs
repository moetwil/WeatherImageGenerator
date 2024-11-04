using System.Text.Json;
using ImageProcessorFunction.DTOs;
using Microsoft.Extensions.Logging;

namespace ImageProcessorFunction.Services;

public class ImageProcessorService
{
    private readonly ILogger<ImageProcessorService> _logger;
    private readonly ImageEditService _imageEditService;
    private readonly BlobStorageService _blobStorageService;
    private readonly HttpClient _httpClient;

    public ImageProcessorService(ILogger<ImageProcessorService> logger, ImageEditService imageEditService,
        BlobStorageService blobStorageService, HttpClient httpClient)
    {
        _logger = logger;
        _imageEditService = imageEditService;
        _blobStorageService = blobStorageService;
        _httpClient = httpClient;
    }

    public async Task ProcessImageAsync(string message)
    {
        var imageInfo = MapMessageToImageInfo(message);
        var imageStream = await FetchImageStreamAsync(imageInfo.ImageUrl);
        var texts = CreateOverlayTexts(imageInfo.Station);

        using var editedImageStream = _imageEditService.OverlayTextOnImage(imageStream, texts);

        await _blobStorageService.SaveImageAsync(editedImageStream, imageInfo.JobId, imageInfo.Station.StationName);
    }

    private (string text, (float x, float y) position, int fontSize, string colorHex)[] CreateOverlayTexts(
        StationDTO station)
    {
        return new[]
        {
            (station.StationName, (10f, 10f), 20, "#FFFFFF"),
            ($"Temperature: {station.Temperature}°C", (10f, 40f), 20, "#FFFFFF"),
            ($"Humidity: {station.Humidity}%", (10f, 70f), 20, "#FFFFFF"),
            ($"Wind: {station.WindDirectionDegrees}°", (10f, 100f), 20, "#FFFFFF"),
            ($"Precipitation: {station.Precipitation}mm", (10f, 130f), 20, "#FFFFFF"),
            ($"Sun power: {station.SunPower}", (10f, 160f), 20, "#FFFFFF"),
            ($"Rainfall last 24h: {station.RainFallLast24Hour}mm", (10f, 190f), 20, "#FFFFFF"),
            ($"Rainfall last hour: {station.RainFallLastHour}mm", (10f, 220f), 20, "#FFFFFF")
        };
    }

    private async Task<Stream> FetchImageStreamAsync(string imageUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError($"HTTP request error while fetching image: {httpEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while fetching image: {ex.Message}");
            return null;
        }
    }

    private ImageInfoDTO MapMessageToImageInfo(string message)
    {
        try
        {
            var imageInfo = JsonSerializer.Deserialize<ImageInfoDTO>(message);
            return imageInfo;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError($"JSON deserialization error: {jsonEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}");
            throw;
        }
    }
}