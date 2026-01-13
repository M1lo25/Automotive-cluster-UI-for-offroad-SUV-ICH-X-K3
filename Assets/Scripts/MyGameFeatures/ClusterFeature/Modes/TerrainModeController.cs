using UnityEngine;

//Controller modalità terreno che mantiene CurrentMode e sincronizza il sistema
//pubblicando e ricevendo l’evento TerrainModeChanged sul bus

namespace ICXK3
{
    //Esegue dopo altri script a ordine minore (dipendenze già inizializzate)
    [DefaultExecutionOrder(200)] 
    public class TerrainModeController : MonoBehaviour
    {
        [Header("Modes")]
        //Modalità assegnate da Inspector dove startMode ha priorità come modalità iniziale
        [SerializeField] TerrainModeSO roadMode;
        [SerializeField] TerrainModeSO trailMode;
        [SerializeField] TerrainModeSO snowMode;
        [SerializeField] TerrainModeSO startMode;

        IBroadcaster _bus;  //Bus eventi risolto via service locator
        bool _subscribed;  //Evita doppie sottoscrizioni al bus
        bool _suppressBroadcast;  //Previene il loop quando reagiamo a un evento appena ricevuto

        public TerrainModeSO CurrentMode { get; private set; } //Stato della modalità corrente

        //Prova a risolvere il bus all’avvio che può non esistere ancora
        void Awake()
        {
            Locator.TryResolve(out _bus);
        }

        //All’abilitazione garantisce la sottoscrizione,seleziona la modalità iniziale
        //con fallback (start->road->trail->snow) e notifica il sistema
        void OnEnable()
        {
            EnsureBusSubscribed();
            CurrentMode = startMode != null ? startMode
                        : (roadMode != null ? roadMode
                        : (trailMode != null ? trailMode
                        : snowMode));
            BroadcastCurrent();
        }

        //Rimuove la sottoscrizione quando disabilitato
        void OnDisable()
        {
            if (_bus != null && _subscribed)
            {
                _bus.Remove<TerrainModeChanged>(OnModeChanged);
                _subscribed = false;
            }
        }

        //API principale che imposta la modalità e la propaga al resto del sistema
        public void SetMode(TerrainModeSO mode)
        {
            SetMode(mode, broadcast: true, instant: false);
        }

        //Imposta la modalità cercandola per nome  tra le tre note
        public void SetModeByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (string.Equals(name, "Road", System.StringComparison.OrdinalIgnoreCase) && roadMode != null) { SetMode(roadMode); return; }
            if (string.Equals(name, "Trail", System.StringComparison.OrdinalIgnoreCase) && trailMode != null) { SetMode(trailMode); return; }
            if (string.Equals(name, "Snow", System.StringComparison.OrdinalIgnoreCase) && snowMode != null) { SetMode(snowMode); return; }
        }

        //Scorciatoie pensate per binding da UI
        public void SetRoad() {
            if (roadMode != null) SetMode(roadMode); 
        }
        public void SetTrail() {
            if (trailMode != null) SetMode(trailMode);
        }
        public void SetSnow() {
            if (snowMode != null) SetMode(snowMode); 
        }

        //Riceve un cambio modalità dall’esterno,si allinea e NON ribroadcasta evitando cicli
        void OnModeChanged(TerrainModeChanged e)
        {
            if (e.mode == null) return;
            if (CurrentMode == e.mode) return;
            _suppressBroadcast = true;
            try
            {
                SetMode(e.mode, broadcast: false, instant: false);
            }
            finally
            {
                _suppressBroadcast = false;
            }
        }

        //Cambio centrale che ignora null e doppioni,aggiorna CurrentMode e opzionalmente notifica
        void SetMode(TerrainModeSO mode, bool broadcast, bool instant)
        {
            if (mode == null) return;
            if (CurrentMode == mode) return;
            CurrentMode = mode;
            if (broadcast && !_suppressBroadcast)
                BroadcastCurrent();
        }

        //Invia l’evento con la modalità corrente e risolve il bus se necessario
        void BroadcastCurrent()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            if (_bus != null && CurrentMode != null)
                _bus.Broadcast(new TerrainModeChanged(CurrentMode));
        }

        //Sottoscrizione idempotente al bus che si registra una sola volta)
        void EnsureBusSubscribed()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            if (_bus != null && !_subscribed)
            {
                _bus.Add<TerrainModeChanged>(OnModeChanged);
                _subscribed = true;
            }
        }
    }
}
