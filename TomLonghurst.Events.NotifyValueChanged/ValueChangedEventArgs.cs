namespace TomLonghurst.Events.NotifyValueChanged;

public class ValueChangedEventArgs<T>
{
    public string? PropertyName { get; }
    public T PreviousValue { get; }
    public T NewValue { get; }
    public DateTimeOffset? PreviousValueDateTimeSet { get; }
    public DateTimeOffset? NewValueDateTimeSet { get; }

    public ValueChangedEventArgs(string propertyName, T previousValue, T newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet)
    {
        PropertyName = propertyName;
        PreviousValue = previousValue;
        NewValue = newValue;
        PreviousValueDateTimeSet = previousValueDateTimeSet;
        NewValueDateTimeSet = newValueDateTimeSet;
    }
}