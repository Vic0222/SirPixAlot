using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using SirPixAlot.Core.EventStore;
using SirPixAlot.Core.GrainInterfaces;
using SirPixAlot.Core.Metrics;
using SirPixAlot.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SirPixAlot.Core.Grains
{
    public class CanvasGrain(IEventStorage eventStorage, ILogger<CanvasGrain> logger) : JournaledGrain<CanvasGrainState, ICanvasGrainEvent>, ICustomStorageInterface<CanvasGrainState, ICanvasGrainEvent>, ICanvasGrain
    {
        public async Task<IEnumerable<Pixel>> GetPixels()
        {
            return State.Pixels;
        }

        public Task<Pixel?> GetPixel(long x, long y)
        {
            var pixel = State.Pixels.FirstOrDefault(p => p.X == x && p.Y == y);
            return Task.FromResult(pixel);
        }

        public async Task<bool> ChangePixelColor(long x, long y, string color)
        {
            if (!ValidateColor(color))
                return false;

            RaiseEvent(new PixelColorChanged() {X = x, Y = y, Color = color, EventDateTime = DateTimeOffset.UtcNow });
            await ConfirmEvents();
            return true;
        }

        private bool ValidateColor(string color)
        {
            return Regex.IsMatch(color, @"[#][0-9A-Fa-f]{6}\b");
        }

        public (long topLeftX, long topLeftY, long bottomRightX, long bottomRightY) GetRect()
        {
            var id = this.GetGrainId().Key.ToString() ?? string.Empty;
            var coords = id.Split(',');
            var topleftString = coords[0].Split(':');
            var bottomRightString = coords[1].Split(':');

            long.TryParse(topleftString[0], out long topLeftX);
            long.TryParse(topleftString[1], out long topLeftY);
            long.TryParse(bottomRightString[0], out long bottomRightX);
            long.TryParse(bottomRightString[1], out long bottomRightY);

            return (topLeftX, topLeftY, bottomRightX, bottomRightY);
        }

        public async Task<KeyValuePair<int, CanvasGrainState>> ReadStateFromStorage()
        {
            var events = await eventStorage.ReadEvents(this.GetGrainId().ToString(), Version);
            var rect = GetRect();
            var state = new CanvasGrainState();
            var stopwatch = Stopwatch.StartNew();
            state.InitPixels(rect.topLeftX, rect.topLeftY, rect.bottomRightX, rect.bottomRightY);
            stopwatch.Stop();
            logger.LogInformation("Init Pixels Took {InitPixelsDuration} Milliseconds", stopwatch.ElapsedMilliseconds);
            int version = 0;
            foreach (var @event in events)
            {

                switch (@event.EventType)
                {
                    case "PixelColorChanged":
                        var pixelColorChanged = JsonSerializer.Deserialize<PixelColorChanged>(@event.Data);
                        if (pixelColorChanged != null)
                            state.Apply(pixelColorChanged);

                        break;
                    default:
                        break;
                }

                version = @event.Version;
            }

            return new KeyValuePair<int, CanvasGrainState>(version, state);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<ICanvasGrainEvent> updates, int expectedVersion)
        {
            var events = new List<Event>();
            int version = Version;
            foreach (var update in updates)
            {
                var @event = new Event
                {
                    GrainId = this.GetGrainId().ToString(),
                    Version = ++version,
                    GlobalPosition = update.EventDateTime.UtcTicks,
                    EventType = update.GetType().Name,
                    Data = JsonSerializer.Serialize(update, update.GetType())
                };
                events.Add(@event);
            }
            return await eventStorage.SaveEvents(events);
        }
    }

    public interface ICanvasGrainEvent
    {
        DateTimeOffset EventDateTime { get; set; }
    }

    public class PixelColorChanged : ICanvasGrainEvent
    {
        public DateTimeOffset EventDateTime { get; set; }
        public string Color { get; set; } = string.Empty;
        public long X { get; set; }
        public long Y { get; set; }
    }

    public class CanvasGrainState
    {
        public ICollection<Pixel> Pixels { get; set; }

        public long TopLeftX { get; set; }
        public long TopLeftY { get; set; }

        public long BottomRightX { get; set; }
        public long BottomRightY { get; set; }

        public CanvasGrainState()
        {
            Pixels = new List<Pixel>();
        }

        public CanvasGrainState(long topLeftX, long topLeftY, long bottomRightX, long bottomRightY)
        {
            Pixels = new List<Pixel>();
        }

        public void InitPixels(long topLeftX, long topLeftY, long bottomRightX, long bottomRightY)
        {
            TopLeftX = topLeftX;
            TopLeftY = topLeftY;
            BottomRightX = bottomRightX;
            BottomRightY = bottomRightY;

            for (long x = TopLeftX; x <= BottomRightX; x++) 
            {
                for (long y = TopLeftY; y >= BottomRightY; y--)
                {
                    try
                    {

                        Pixels.Add(new Pixel(x, y, "#FFFFFF"));
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }
            }
        }

        public void Apply(PixelColorChanged @event)
        {
            var pixel = Pixels.FirstOrDefault(p => p.X == @event.X && p.Y == @event.Y);
            if (pixel != null)
            {
                pixel.Color = @event.Color;
            }

        }
    }

}
