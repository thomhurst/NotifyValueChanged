namespace TomLonghurst.Events.NotifyContextChanged;

public delegate void ContextChangedEventHandler<T>(object? sender, ContextChangedEventArgs<T> e);