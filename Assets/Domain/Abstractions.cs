//Pubblica un evento di tipo T su un canale globale del bus
public interface IEventBus
{
    void Publish<T>(T evt);  //Pubblica l'evento T
    void Subscribe<T>(System.Action<T> handler);  //Registra handler per T
    void Unsubscribe<T>(System.Action<T> handler); //Deregistra handler per T
}

public interface IAudioService
{
    void Play(string key, int priority = 1, bool loop = false); 
    void Stop(string key);                                    
}

