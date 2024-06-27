using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans.Runtime;
using SirPixAlot.Core.DataTransferObjects;
using SirPixAlot.Core.GrainInterfaces;
using SirPixAlot.Core.Models;

namespace SirPixAlot.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CanvasController(IGrainFactory grainFactory) : ControllerBase
    {
        const double CANVAS_SIZE = 100;

        [HttpGet("pixel")]
        public async Task<IActionResult> Get(long x, long y)
        {
            var dto = await GetPixelDto(grainFactory, x, y);
            return Ok(dto);
        }

        private static async Task<PixelDto> GetPixelDto(IGrainFactory grainFactory, long x, long y)
        {
            var canvas = grainFactory.GetGrain<ICanvasGrain>(GetCanvasIdFromPixel(x, y));
            var pixel = await canvas.GetPixel(x, y);
            return new PixelDto
            {
                X = x,
                Y = y,
                Color = pixel?.Color ?? "#FFFFFF"
            };
        }

        private static string GetCanvasIdFromPixel(long x, long y)
        {

            long topleftX = (long)(Math.Floor(x / CANVAS_SIZE) * CANVAS_SIZE);
            long topleftY = (long)(Math.Ceiling(y / CANVAS_SIZE) * CANVAS_SIZE);

            long bottomRightX = (long)(Math.Floor(x / CANVAS_SIZE) * CANVAS_SIZE) + (long)CANVAS_SIZE - 1;
            long bottomRightY = (long)(Math.Ceiling(y / CANVAS_SIZE) * CANVAS_SIZE) - (long)CANVAS_SIZE + 1;

            return GenerateCanvasId(topleftX, topleftY, bottomRightX, bottomRightY);
        }

        private static string GenerateCanvasId(long topleftX, long topleftY, long bottomRightX, long bottomRightY)
        {
            return $"{topleftX}:{topleftY},{bottomRightX}:{bottomRightY}";
        }

        [HttpGet]
        public async Task<IActionResult> Get(long topLeftX, long topLeftY, long bottomRightX, long bottomRightY)
        {
            long normalizeTopLeftX = (long)(Math.Floor(topLeftX / CANVAS_SIZE) * CANVAS_SIZE);
            long normalizeTopLeftY = (long)(Math.Ceiling(topLeftY / CANVAS_SIZE) * CANVAS_SIZE);

            long normalizeBottomRightX = (long)(Math.Floor(bottomRightX / CANVAS_SIZE) * CANVAS_SIZE);
            long normalizeBottomRightY = (long)(Math.Ceiling(bottomRightY / CANVAS_SIZE) * CANVAS_SIZE);

            var pixels = new List<Pixel>();

            for (long x = normalizeTopLeftX; x <= normalizeBottomRightX; x+= (long)CANVAS_SIZE)
            {
                for (long y = normalizeBottomRightY; y <= normalizeTopLeftY; y+=(long)CANVAS_SIZE)
                {
                    long bottomX = x + (long)CANVAS_SIZE - 1;
                    long bottomY = y - (long)CANVAS_SIZE + 1;
                    var canvasId = GenerateCanvasId(x, y, bottomX, bottomY);
                    var canvas = grainFactory.GetGrain<ICanvasGrain>(canvasId);
                    pixels.AddRange(await canvas.GetPixels());
                }
            }


            return Ok(pixels.Select(p => new PixelDto() {
                X = p.X,
                Y = p.Y,
                Color = p.Color,
            }).ToList());
        }

        
        [HttpPut]
        public async Task<IActionResult> Put(PixelDto pixel)
        {
            var pixelGrain = grainFactory.GetGrain<ICanvasGrain>(GetCanvasIdFromPixel(pixel.X, pixel.Y));
            bool success = await pixelGrain.ChangePixelColor(pixel.X, pixel.Y, pixel.Color);
            var savedPixel = await pixelGrain.GetPixel(pixel.X, pixel.Y);
            PixelDto pixelDto = new PixelDto
            {
                X = pixel.X,
                Y = pixel.Y,
                Color = savedPixel.Color,
            };

            if (!success)
            {
                return BadRequest(pixelDto);
            }
            else
            {
                return Ok(pixelDto);
            }
            
        }
    }
}
