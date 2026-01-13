using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using ICXK3.Domain;
using ICXK3;

public class Kernel : MonoBehaviour
{
    //Theme SO assegnati via Inspector
    public ThemeSO DayTheme;
    public ThemeSO NightTheme;

    //Evita doppia inizializzazione
    bool _initialized;

    //Bootstrap centrale con servizi,Addressables,UI,feature,sim loop
    public async void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        // Reset e registrazione servizi core:eventi e broadcast
        ServiceRegistry.Reset();
        ServiceRegistry.Register<IEventBus>(new EventBus());
        ServiceRegistry.Register<IBroadcaster>(new Broadcaster());

        //Servizio dati veicolo tramite broadcaster
        var bus = ServiceRegistry.Resolve<IBroadcaster>();
        var vehicle = new VehicleDataService(bus);
        ServiceRegistry.Register<IVehicleDataService>(vehicle);

        //Servizio temi che parte in play da Day
        var themeSvc = new ThemeService(DayTheme, NightTheme, startAsDay: true);
        ServiceRegistry.Register<IThemeService>(themeSvc);

        //Inizializza Addressables
        await Addressables.InitializeAsync().Task;

        //Istanzia UI root in modo persistente
        var uiRoot = await Addressables.InstantiateAsync("UiRoot").Task;
        DontDestroyOnLoad(uiRoot);

        //Crea FeatureHost persistente e boota moduli
        var hostGo = new GameObject("FeatureHost");
        DontDestroyOnLoad(hostGo);
        var host = hostGo.AddComponent<FeatureHost>();
        await host.BootAsync(uiRoot.transform);

        //Avvia loop di simulazione veicolo
        gameObject.AddComponent<VehicleSimLoop>();
    }
}
