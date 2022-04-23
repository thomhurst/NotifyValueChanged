namespace TomLonghurst.Events.NotifyContextChanged;

public class ContextChangedEventArgs<T>
{
    public string? PropertyName { get; }
    public T PreviousContext { get; }
    public T NewContext { get; }

    public ContextChangedEventArgs(string propertyName, T previousContext, T newContext)
    {
        PropertyName = propertyName;
        PreviousContext = previousContext;
        NewContext = newContext;
    }
}