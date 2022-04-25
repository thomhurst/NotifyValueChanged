namespace TomLonghurst.Events.NotifyValueChanged;

public class ValueChangedEventArgs<T>
{
    public string? PropertyName { get; }
    public T PreviousValue { get; }
    public T NewValue { get; }

    public ValueChangedEventArgs(string propertyName, T previousValue, T newValue)
    {
        PropertyName = propertyName;
        PreviousValue = previousValue;
        NewValue = newValue;
    }
}