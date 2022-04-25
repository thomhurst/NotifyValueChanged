namespace TomLonghurst.Events.NotifyValueChanged;

public delegate void ValueChangedEventHandler<T>(object? sender, ValueChangedEventArgs<T> e);