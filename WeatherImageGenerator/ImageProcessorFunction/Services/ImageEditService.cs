using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using Microsoft.Extensions.Logging;
using System.IO;
using SixLabors.ImageSharp.Drawing.Processing;

namespace ImageProcessorFunction.Services
{
    public class ImageEditService
    {
        private readonly ILogger<ImageEditService> _logger;

        public ImageEditService(ILogger<ImageEditService> logger)
        {
            _logger = logger;
        }

        public Stream OverlayTextOnImage(Stream imageStream, (string text, (float x, float y) position, int fontSize, string colorHex)[] texts)
        {
            var memoryStream = new MemoryStream();

            try
            {
                // Load the image from the provided stream
                using (var image = Image.Load<Rgba32>(imageStream))
                {
                    foreach (var (text, (x, y), fontSize, colorHex) in texts)
                    {
                        // Load the font
                        var font = SystemFonts.CreateFont("Verdana", fontSize);
                        var color = Rgba32.ParseHex(colorHex);

                        // Draw text onto the image
                        image.Mutate(img => img.DrawText(text, font, color, new PointF(x, y)));
                    }

                    // Save the image to the memory stream
                    image.SaveAsPng(memoryStream);
                }

                // Reset the memory stream position to the beginning
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error overlaying text on image: {ex.Message}");
                throw;
            }
        }
    }
}