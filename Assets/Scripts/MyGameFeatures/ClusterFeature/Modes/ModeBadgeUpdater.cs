using UnityEngine;
using TMPro;


//Consente di tenere aggiornato un TMP_Text(badge) con il nome della modalità di terreno corrente

namespace ICXK3
{
    [DisallowMultipleComponent]
    public class ModeBadgeUpdater : MonoBehaviour
    {
        [SerializeField] TMP_Text target;   //Testo da aggiornare 

        IBroadcaster _bus;   //Bus eventi risolto via sevice Locator
        bool _subscribed;  //Flag per evitare doppie sottoscrizioni

        //Metodo hiamato quando il componente viene istanziato che risolve 'target' se mancante,
        //tenta la risoluzione del bus eventi e imposta un testo di default per avere un valore immediatamente visibile
        void Awake()
        {
            if (!target) target = GetComponent<TMP_Text>();
            Locator.TryResolve(out _bus);
            if (target) target.text = "MODE"; 
        }

        //Attiva sottoscrizione quando il componente viene chiamato
        void OnEnable() => EnsureSubscribed();
        //Disattiva sottoscrizione quando il componente viene disabilitato
        void OnDisable() => Unsubscribe();        
        
        //Ritenta la risoluzione del bus se non disponibile,chiamato in ogni frame
        void Update() 
        {
            if (_bus == null)
            {
                Locator.TryResolve(out _bus);
                EnsureSubscribed();
            }
        }

        //Registra il callback OnModeChanged per l'evento TerrainModeChanged
        void EnsureSubscribed()
        {
            if (_bus == null || _subscribed) return;
            _bus.Add<TerrainModeChanged>(OnModeChanged);
            _subscribed = true;
        }

        //Rimuove il callback OnModeChanged
        void Unsubscribe()
        {
            if (_bus == null || !_subscribed) return;
            _bus.Remove<TerrainModeChanged>(OnModeChanged);
            _subscribed = false;
        }

        //Chiamato quando arriva un TerrainModeChanged dal bus,gestisce l'evento TerrainModeChanged:
        //se il componente è attivo e c'è un TMP_Text,legge il nome della modalità dall'evento,
        //lo trasforma in MAIUSCOLO e lo mostra.
        //Se il nome non c'è,visualizza "MODE"
        void OnModeChanged(TerrainModeChanged e) 
        {
            if (!isActiveAndEnabled || target == null) return;
            var name = (e.mode != null && !string.IsNullOrEmpty(e.mode.modeName)) ? e.mode.modeName.ToUpperInvariant() : "MODE";
            target.text = name;
        }
    }
}
