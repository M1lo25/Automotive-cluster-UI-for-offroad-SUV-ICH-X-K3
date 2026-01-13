using System;

//Facciata statica per accedere al ServiceRegistry
public static class Locator
{
    //Viene registrata l'istanza per T
    public static void Register<T>(T instance) where T : class
        => ServiceRegistry.Register(instance);   

    //Ottiene l'istanza e la può lanciare se mancante
    public static T Resolve<T>() where T : class
        => ServiceRegistry.Resolve<T>();

     //Prova a risolvere senza eccezioni anche se non registrato
    public static bool TryResolve<T>(out T service) where T : class
        => ServiceRegistry.TryResolve(out service); 

    //Se T è registrato ritorna true
    public static bool IsRegistered<T>() where T : class
        => ServiceRegistry.IsRegistered<T>();

    //Rimuove registrazione di T e fornisce true se rimossa
    public static bool Unregister<T>() where T : class
        => ServiceRegistry.Unregister<T>();      
}
