using System;
using UnityEngine;

//Marker per i servizi risolvibili via ServiceRegistry
public interface IService { } 

//API minimale per broadcasting tipizzato che usa il bus sottostante
public interface IBroadcaster : IService
{
    void Broadcast<T>(T arg);   //Invia evento o messaggio T
    void Add<T>(Action<T> action);  //Registra handler per T
    void Remove<T>(Action<T> action);  //Deregistra handler per T
}

//Adattatore che delega a IEventBus risolto dal ServiceRegistry
public class Broadcaster : IBroadcaster
{
    IEventBus Bus => ServiceRegistry.Resolve<IEventBus>(); //Richiede IEventBus registrato

    public void Broadcast<T>(T arg)    => Bus.Publish(arg);  //Inoltra al bus
    public void Add<T>(Action<T> a)    => Bus.Subscribe(a);   //Aggiunge listener
    public void Remove<T>(Action<T> a) => Bus.Unsubscribe(a);  //Rimuove listener
}
