using UnityEngine;
using ICXK3.Domain;

public class VehicleSimLoop : MonoBehaviour
{
    //Servizio dati veicolo risolto dal ServiceRegistry
    IVehicleDataService _veh;

    //Contatore per log periodico
    int _frameMod;

    void Awake()
    {
        //Prova a ottenere il servizio,loggando errore se non registrato
        if (ServiceRegistry.TryResolve<IVehicleDataService>(out _veh))
            Debug.Log("[SimLoop] Vehicle service OK");
        else
            Debug.LogError("[SimLoop] IVehicleDataService non registrato");
    }

    void Update()
    {
        if (_veh == null) return;

        //Tick di simulazione con dt del frame
        _veh.SimTick(Time.deltaTime);

        //Log ogni 30 frame
        if (++_frameMod % 30 == 0)
            Debug.Log("[SimLoop] tick");
    }
}
