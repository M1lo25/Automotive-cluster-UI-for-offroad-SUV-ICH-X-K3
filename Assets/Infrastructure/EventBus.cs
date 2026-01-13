using System;
using System.Collections.Generic;

public class EventBus : IEventBus
{
    // Mappa il tipo dell'evento->lista di handler(Action<T>) per uso single thread
    private readonly Dictionary<Type, List<Delegate>> _subs = new();

    //Notifica tutti gli handler registrati per il tipo T,le eccezzioni non vengono catturate qua infatti si propagano
    public void Publish<T>(T evt)
    {
        if (_subs.TryGetValue(typeof(T), out var list))
            foreach (var d in list) (d as Action<T>)?.Invoke(evt);
    }

    //Registra un handler per eventi di tipo T consentendo registrazioni duplicate
    public void Subscribe<T>(Action<T> handler)
    {
        if (!_subs.TryGetValue(typeof(T), out var list))
        {
            list = new List<Delegate>();
            _subs[typeof(T)] = list;
        }
        list.Add(handler);
    }

    //Deregistra l'handler per il tipo T,se presente
    public void Unsubscribe<T>(Action<T> handler)
    {
        if (_subs.TryGetValue(typeof(T), out var list))
            list.Remove(handler);
    }
}
