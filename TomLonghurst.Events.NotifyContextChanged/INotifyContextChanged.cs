using System.Runtime.CompilerServices;

namespace TomLonghurst.Events.NotifyContextChanged;

public interface INotifyContextChanged<T>
{
    event ContextChangedEventHandler<T> OnContextChangeEvent;
    void OnContextChanged(T previousValue, T newValue, [CallerMemberName] string propertyName = null);
}