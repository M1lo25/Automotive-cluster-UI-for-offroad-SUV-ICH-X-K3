//Gestisce la selezione della modalità terreno Road,Trail,Snow aggiornando lo stato dei pulsanti,
//applicando il tema e notificando il resto del sistema via event bus

using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ICXK3
{
    public class PanelModeController : MonoBehaviour
    {
        [SerializeField] ModeButton btnRoad;
        [SerializeField] ModeButton btnTrail;
        [SerializeField] ModeButton btnSnow;

        public enum TerrainUiMode { Road, Trail, Snow }
        [SerializeField] TerrainUiMode current = TerrainUiMode.Road;

        [SerializeField] TerrainModeSO roadMode;
        [SerializeField] TerrainModeSO trailMode;
        [SerializeField] TerrainModeSO snowMode;

        IThemeService _themeSvc;
        IBroadcaster _bus;

        //Risolve il bus eventi e prova a bindare automaticamente i pulsanti se non assegnati da Inspector
        void Awake()
        {
            Locator.TryResolve(out _bus);
            AutoBind();
        }

        //Aggancia gli eventi tema,aggiorna subito la UI e invia la modalità corrente al resto del sistema
        void OnEnable()
        {
            _themeSvc = Locator.Resolve<IThemeService>();
            if (_themeSvc != null)
            {
                _themeSvc.Changed        += OnThemeChanged;
                _themeSvc.OnThemeChanged += OnThemeChanged;
            }
            ApplyVisuals();
            BroadcastCurrent();
        }

        //Sgancia gli eventi per evitare leak e callback quando l’oggetto è disabilitato
        void OnDisable()
        {
            if (_themeSvc != null)
            {
                _themeSvc.Changed        -= OnThemeChanged;
                _themeSvc.OnThemeChanged -= OnThemeChanged;
            }
        }

        //Quando cambia il tema,riapplica lo stato On/Off dei pulsanti 
        void OnThemeChanged(ThemeSO _)
        {
            ApplyVisuals();
        }

        //Aggiorna la UI evidenziando solo il bottone corrispondente alla modalità corrente
        void ApplyVisuals()
        {
            if (btnRoad)  btnRoad.SetOn(current == TerrainUiMode.Road);
            if (btnTrail) btnTrail.SetOn(current == TerrainUiMode.Trail);
            if (btnSnow)  btnSnow.SetOn(current == TerrainUiMode.Snow);
        }

        //Handler UI per i click che imposta la modalità richiesta
        public void OnClickRoad()  => SetMode(TerrainUiMode.Road);
        public void OnClickTrail() => SetMode(TerrainUiMode.Trail);
        public void OnClickSnow()  => SetMode(TerrainUiMode.Snow);

        //Cambia modalità,aggiorna la UI e notifica gli altri sistemi
        public void SetMode(TerrainUiMode m)
        {
            current = m;
            ApplyVisuals();
            BroadcastCurrent();
        }

        //Traduce la modalità UI nel relativo TerrainModeSO e lo broadcasta sul bus
        void BroadcastCurrent()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            if (_bus == null) return;

            TerrainModeSO so = null;
            switch (current)
            {
                case TerrainUiMode.Road:  so = roadMode;  break;
                case TerrainUiMode.Trail: so = trailMode; break;
                case TerrainUiMode.Snow:  so = snowMode;  break;
            }
            if (so != null)
                _bus.Broadcast(new TerrainModeChanged(so));
        }

        //Shortcut da tastiera F1,F2,F3 per cambiare modalità,compatibile con Input System nuovo o legacy
        void Update()
        {
        #if ENABLE_INPUT_SYSTEM
                    var kb = Keyboard.current;
                    if (kb != null)
                    {
                        if (kb.f1Key.wasPressedThisFrame) OnClickRoad();
                        if (kb.f2Key.wasPressedThisFrame) OnClickTrail();
                        if (kb.f3Key.wasPressedThisFrame) OnClickSnow();
                    }
        #else
                    if (Input.GetKeyDown(KeyCode.F1)) OnClickRoad();
                    if (Input.GetKeyDown(KeyCode.F2)) OnClickTrail();
                    if (Input.GetKeyDown(KeyCode.F3)) OnClickSnow();
        #endif
        }

        //Auto risoluzione dei ModeButton cercandoli nella gerarchia e aggiungendo il componente se manca
        void AutoBind()
        {
            if (!btnRoad)
            {
                var t = transform.Find("Content/BtnModeRoad") ?? transform.Find("BtnModeRoad");
                if (t)
                {
                    btnRoad = t.GetComponent<ModeButton>();
                    if (!btnRoad) btnRoad = t.gameObject.AddComponent<ModeButton>();
                }
            }
            if (!btnTrail)
            {
                var t = transform.Find("Content/BtnModeTrail") ?? transform.Find("BtnModeTrail");
                if (t)
                {
                    btnTrail = t.GetComponent<ModeButton>();
                    if (!btnTrail) btnTrail = t.gameObject.AddComponent<ModeButton>();
                }
            }
            if (!btnSnow)
            {
                var t = transform.Find("Content/BtnModeSnow") ?? transform.Find("BtnModeSnow");
                if (t)
                {
                    btnSnow = t.GetComponent<ModeButton>();
                    if (!btnSnow) btnSnow = t.gameObject.AddComponent<ModeButton>();
                }
            }
        }
    }
}
