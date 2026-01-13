using UnityEngine;
using UnityEngine.AddressableAssets;

//Punto d'ingresso del progetto che crea e avvia il kernel
public static class Bootstrap
{
    //Eseguito automaticamente prima del caricamento della prima scena,viene instanziato il kernel via Adressables
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static async void Run()
    {
        //Evita di creare più istanze del kernel,infatti se ne esiste già uno si esce subito
        if (Object.FindAnyObjectByType<Kernel>() != null)
            return;

        //Carica e istanzia il prefab Kernel dagli Addressables
        var handle = Addressables.InstantiateAsync("Kernel");
        var kernelGo = await handle.Task;

        //Mantieni il Kernel persistente nei cambi di scena
        Object.DontDestroyOnLoad(kernelGo);

        //Recupera il componente Kernel dal GameObject instanziato e avvia l’inizializzazione dei servizi interni
        var kernel = kernelGo.GetComponent<Kernel>();
        kernel.Initialize();
    }
}