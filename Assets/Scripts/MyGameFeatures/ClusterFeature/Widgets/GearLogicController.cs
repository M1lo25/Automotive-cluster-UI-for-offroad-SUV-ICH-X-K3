// Gestisce la logica cambio marcia: ascolta velocità e selettore PRND, calcola automaticamente la marcia 1..7 in D con soglie up/downshift e sincronizza UI + broadcast eventi.
using UnityEngine;
using ICXK3.Domain;

namespace ICXK3
{
    public class GearLogicController : MonoBehaviour
    {
        public struct OnGearSelectorChanged { public char selector; public OnGearSelectorChanged(char s) { selector = s; } }
        public struct OnDriveGearChanged   { public int  gear;     public OnDriveGearChanged  (int g) { gear     = g; } }

        [Header("UI")]
        [SerializeField] private GearController ui;

        [Header("Use Preset")]
        [SerializeField] private bool usePresetICXK3 = true;

        private static readonly float[] PRESET_UP = { 30f, 55f, 85f, 115f, 145f, 170f, 185f };
        private static readonly float[] PRESET_DN = { 25f, 50f, 80f, 110f, 140f, 168f, 999f };

        [Header("Upshift(km/h)")]
        [SerializeField] private float[] upshift   = new float[7] { 30f, 55f, 85f, 115f, 145f, 170f, 185f };

        [Header("Downshift(km/h)")]
        [SerializeField] private float[] downshift = new float[7] { 25f, 50f, 80f, 110f, 140f, 168f, 999f };

        [Header("Input")]
        [SerializeField] private bool enableHotkeys = false;

        IBroadcaster _bus;
        IVehicleDataService _vehicle;
        float _speedKmh;
        char  _selector = 'N';
        int   _driveGear = 1;

        System.Action<OnSpeedChanged> _onSpeedH;
        System.Action<OnGearChanged>  _onGearH;

        // Risolve dipendenze, aggancia listener al bus (speed + gear) e inizializza preset soglie se richiesto.
        void Awake()
        {
            Locator.TryResolve(out _bus);
            Locator.TryResolve(out _vehicle);

            _onSpeedH = (e) => { _speedKmh = e.kmh; };
            _bus?.Add(_onSpeedH);

            _onGearH = (e) =>
            {
                _selector = e.gear switch { Gear.P => 'P', Gear.R => 'R', Gear.N => 'N', Gear.D => 'D', _ => 'N' };
                ApplyUI();
            };
            _bus?.Add(_onGearH);

            if (!ui) ui = GetComponentInChildren<GearController>(true);

            if (usePresetICXK3)
            {
                EnsureArrays();
                for (int i = 0; i < 7; i++) { upshift[i] = PRESET_UP[i]; downshift[i] = PRESET_DN[i]; }
            }
        }

        // All’avvio sincronizza il selettore con la marcia reale del veicolo e aggiorna la UI.
        void Start()
        {
            if (_vehicle != null) _selector = CharFromGear(_vehicle.DriveGear);
            ApplyUI();
        }

        // Cleanup: rimuove i listener dal bus per evitare leak e callback post-destroy.
        void OnDestroy()
        {
            if (_bus != null)
            {
                if (_onSpeedH != null) _bus.Remove(_onSpeedH);
                if (_onGearH  != null) _bus.Remove(_onGearH);
            }
        }

        // Loop: hotkey opzionali, keep-alive delle reference e calcolo marcia automatica in D con broadcast + UI.
        void Update()
        {
            if (enableHotkeys)
            {
                if (Input.GetKeyDown(KeyCode.P)) RequestGear(Gear.P);
                if (Input.GetKeyDown(KeyCode.R)) RequestGear(Gear.R);
                if (Input.GetKeyDown(KeyCode.N)) RequestGear(Gear.N);
                if (Input.GetKeyDown(KeyCode.D)) RequestGear(Gear.D);
            }

            if (_vehicle == null && Time.frameCount % 60 == 0) Locator.TryResolve(out _vehicle);
            if (_vehicle != null && Time.frameCount % 5 == 0) _speedKmh = _vehicle.SpeedKph;

            if (_selector == 'D')
            {
                int g = ComputeDriveGear(_speedKmh);
                if (g != _driveGear)
                {
                    _driveGear = g;
                    _bus?.Broadcast(new OnDriveGearChanged(_driveGear));
                    ui?.SetDriveGearNumber(_driveGear);
                }
            }
        }

        // Richiede al VehicleDataService il cambio selettore; fallback locale se il servizio non è disponibile.
        void RequestGear(Gear g)
        {
            if (_vehicle != null) _vehicle.SetGear(g);
            else SetSelector(CharFromGear(g));
        }

        // Imposta il selettore PRND localmente, pubblica evento e aggiorna UI.
        void SetSelector(char s)
        {
            _selector = s;
            if (_selector == 'D' && _driveGear < 1) _driveGear = 1;
            _bus?.Broadcast(new OnGearSelectorChanged(_selector));
            ApplyUI();
        }

        // Sincronizza UI: selettore PRND e numero marcia (solo in D).
        void ApplyUI()
        {
            ui?.SetSelector(_selector);
            ui?.SetDriveGearNumber(_selector == 'D' ? Mathf.Max(_driveGear, 1) : 0);
        }

        // Utility: converte enum Gear nel char usato dalla UI.
        static char CharFromGear(Gear g) => g switch { Gear.P => 'P', Gear.R => 'R', Gear.N => 'N', Gear.D => 'D', _ => 'N' };

        // Calcola la marcia 1..7 usando isteresi upshift/downshift in base alla marcia corrente e alla velocità.
        int ComputeDriveGear(float v)
        {
            for (int i = 0; i < 7; i++)
            {
                if (_driveGear <= i + 1)
                {
                    if (v > upshift[i]) { return Mathf.Min(7, i + 2); }
                }
                else
                {
                    if (v < downshift[i]) { return Mathf.Max(1, i + 1); }
                }
            }
            return Mathf.Clamp(_driveGear, 1, 7);
        }

        // Garantisce array validi di lunghezza 7 (per Inspector e preset).
        void EnsureArrays()
        {
            if (upshift == null || upshift.Length != 7)   upshift   = new float[7];
            if (downshift == null || downshift.Length != 7) downshift = new float[7];
        }

        // Mantiene coerenti i valori in Inspector quando usePresetICXK3 è attivo.
        void OnValidate()
        {
            if (!usePresetICXK3) return;
            EnsureArrays();
            for (int i = 0; i < 7; i++) { upshift[i] = PRESET_UP[i]; downshift[i] = PRESET_DN[i]; }
        }
    }
}
