namespace Helper.Models.Events
{
    public interface IEvent
    {
        string Name { get; }
        
        bool NeedNotify { get; }
    }
}
