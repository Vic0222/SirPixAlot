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
            //We originally tried to get pxels at the same time here and used Task.WhenAll.
            //But that seems to hammer the silo's too hard.
            //So we do await here one by one to lessen load.
            var pixelDtos = new List<PixelDto>();
            for (var x = topLeftX; x <= bottomRightX; x++) 
            { 
                for (var y = bottomRightY; y <= topLeftY; y++)
                {
                    var pixelDto = await GetPixelDto(grainFactory, x, y);
                    pixelDtos.Add(pixelDto);
                }
            }

            return Ok(pixelDtos);
        }

        [HttpGet("rectanglePerfTest")]
        public async Task<IActionResult> GetRectanglePerfTest(long topLeftX, long topLeftY, long bottomRightX, long bottomRightY)
        {
            //We originally tried to get pxels at the same time here and used Task.WhenAll.
            //But that seems to hammer the silo's too hard.
            //So we do await here one by one to lessen load.

            const int batchSize = 100;
            var pixelDtos = new List<Task<PixelDto>>();
            for (var x = topLeftX; x <= bottomRightX; x++)
            {
                for (var y = bottomRightY; y <= topLeftY; y++)
                {
                    var pixelDto = GetPixelDto(grainFactory, x, y);
                    pixelDtos.Add(pixelDto);
                    if (x + y % batchSize == 0)
                    {
                        await Task.WhenAll(pixelDtos);
                    }
                }
            }

            await Task.WhenAll(pixelDtos);

            return Ok(pixelDtos.Select(p => p.Result));
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
