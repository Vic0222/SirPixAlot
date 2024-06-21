using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using SirPixAlot.Core.EventStore;
using SirPixAlot.Core.GrainInterfaces;
using SirPixAlot.Core.Metrics;
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
    public class PixelGrain(IEventStorage eventStorage) : JournaledGrain<PixelGrainState, IPixelGrainEvent>, ICustomStorageInterface<PixelGrainState, IPixelGrainEvent>, IPixelGrain
    {

        public Task<string> GetColor()
        {
            return Task.FromResult(State.Color);
        }


        public async Task<bool> ChangeColor(string color)
        {
            if (!ValidateColor(color))
                return false;

            RaiseEvent(new ColorChanged() { Color = color, EventDateTime = DateTimeOffset.UtcNow });
            await ConfirmEvents();
            return true;
        }

        private bool ValidateColor(string color)
        {
            return Regex.IsMatch(color, @"[#][0-9A-Fa-f]{6}\b");
        }

        public async Task<KeyValuePair<int, PixelGrainState>> ReadStateFromStorage()
        {
            var events = await eventStorage.ReadEvents(this.GetGrainId().ToString(), Version);
            var state = new PixelGrainState();
            int version = 0;
            foreach (var @event in events)
            {

                switch (@event.EventType)
                {
                    case "ColorChanged":
                        var colorChanged = JsonSerializer.Deserialize<ColorChanged>(@event.Data);
                        if (colorChanged != null)
                            state.Apply(colorChanged);

                        break;
                    default:
                        break;
                }

                version = @event.Version;
            }

            return new KeyValuePair<int, PixelGrainState>(version, state);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<IPixelGrainEvent> updates, int expectedVersion)
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

    public interface IPixelGrainEvent
    {
        DateTimeOffset EventDateTime { get; set; }
    }

    public class ColorChanged : IPixelGrainEvent
    {
        public DateTimeOffset EventDateTime { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class PixelGrainState
    {
        public string Color { get; set; }

        public PixelGrainState()
        {
            Color = "#FFFFFF";
        }

        public void Apply(ColorChanged @event)
        {
            this.Color = @event.Color;  
        }
    }
}
