namespace SirPixAlot.Core.EventStore
{
    public interface IEventStorage
    {
        Task<List<Event>> ReadEvents(string grainId, int version);
        Task<bool> SaveEvents(IReadOnlyCollection<Event> events);
    }
}
