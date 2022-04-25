namespace TomLonghurst.Events.NotifyContextChanged;

public interface INotifyContextChanged<T>
{
    event ContextChangedEventHandler<T> OnContextChangeEvent;
}