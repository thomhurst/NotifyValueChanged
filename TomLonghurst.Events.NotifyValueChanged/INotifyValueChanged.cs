namespace TomLonghurst.Events.NotifyValueChanged;

public interface INotifyValueChanged<T>
{
    event ValueChangedEventHandler<T> OnValueChange;
}