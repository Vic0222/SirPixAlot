using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans.Runtime;
using SirPixAlot.Core.DataTransferObjects;
using SirPixAlot.Core.GrainInterfaces;

namespace SirPixAlot.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CanvasController(IGrainFactory grainFactory) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(long x, long y)
        {
            var dto = await GetPixelDto(grainFactory, x, y);
            return Ok(dto);
        }

        private static async Task<PixelDto> GetPixelDto(IGrainFactory grainFactory, long x, long y)
        {
            var pixel = grainFactory.GetGrain<IPixelGrain>(x, y.ToString());
            return new PixelDto
            {
                X = x,
                Y = y,
                Color = await pixel.GetColor()
            };
        }

        [HttpGet("rectangle")]
        public async Task<IActionResult> Get(long topLeftX, long topLeftY, long bottomRightX, long bottomRightY)
        {
            //var pixel = grainFactory.GetGrain<IPixelGrain>(x, y.ToString());
            var pixelTasks = new List<Task<PixelDto>>();
            for (var x = topLeftX; x <= bottomRightX; x++) 
            { 
                for (var y = bottomRightY; y <= topLeftY; y++)
                {
                    var task = GetPixelDto(grainFactory, x, y);
                    pixelTasks.Add(task);
                }
            }
            await Task.WhenAll(pixelTasks);
            return Ok(pixelTasks.Select(t => t.Result));
        }

        [HttpPut]
        public async Task<IActionResult> Put(PixelDto pixel)
        {
            var pixelGrain = grainFactory.GetGrain<IPixelGrain>(pixel.X, pixel.Y.ToString());
            bool success = await pixelGrain.ChangeColor(pixel.Color);
            PixelDto pixelDto = new PixelDto
            {
                X = pixel.X,
                Y = pixel.Y,
                Color = await pixelGrain.GetColor()
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
