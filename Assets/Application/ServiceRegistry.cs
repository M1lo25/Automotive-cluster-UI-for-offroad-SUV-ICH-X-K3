using System;
using System.Collections.Generic;

public static class ServiceRegistry
{
    //Mappa centrale dei servizi
    private static readonly Dictionary<Type, object> map = new();

    //Svuota tutte le registrazioni,così tutti i registri spariscono
    public static void Reset() => map.Clear();

    //Registra e sostituisce l'implementazione per T dove impl non può essere null
    public static void Register<T>(T impl)
    {
        if (impl == null)
            throw new ArgumentNullException(nameof(impl), $"ServiceRegistry.L'istanza register per {typeof(T).Name} è null.");
        map[typeof(T)] = impl!;
    }

    //Risolve il servizio registrato per T quando deve essere obbligatorio
    public static T Resolve<T>()
    {
        if (!map.TryGetValue(typeof(T), out var obj) || obj is not T typed)
            throw new KeyNotFoundException($"ServiceRegistry.Resolve,nessun servizio registrato per {typeof(T).Name}.");
        return typed;
    }

    //Risoluzione soft,ritorna bool ed evita eccezioni
    public static bool TryResolve<T>(out T service)
    {
        if (map.TryGetValue(typeof(T), out var obj) && obj is T typed)
        {
            service = typed;
            return true;
        }
        service = default!;
        return false;
    }

    //Verifica se esiste una registrazione per T non controllando la validità runtime
    public static bool IsRegistered<T>() => map.ContainsKey(typeof(T));
    // Rimuove l'eventuale registrazione per T
    public static bool Unregister<T>() => map.Remove(typeof(T));
}
