namespace SirPixAlot.Core.EventStore
{
    public interface IEventStorage
    {
        Task<IEnumerable<Event>> ReadEvents(string grainId, int version);
        Task<bool> SaveEvents(IReadOnlyCollection<Event> events);
    }
}
