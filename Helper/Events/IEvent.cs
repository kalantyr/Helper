namespace Helper.Events
{
    public interface IEvent
    {
        string Name { get; }
        
        bool NeedNotify { get; }
    }
}
