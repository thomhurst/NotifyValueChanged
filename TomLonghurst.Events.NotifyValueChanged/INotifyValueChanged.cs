namespace TomLonghurst.Events.NotifyValueChanged;

public interface INotifyValueChanged
{
    event ValueChangedEventHandler<object> OnAnyValueChange;
}