// Gestisce il tachimetro UI (velocità + RPM): ascolta gli eventi dal bus, applica smoothing ai valori e aggiorna testi e barre di riempimento.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class GaugeSpeed : MonoBehaviour
    {
        [Header("Speed Panel")]
        [SerializeField] private TMP_Text speedValue;
        [SerializeField] private TMP_Text speedUnit;
        [SerializeField] private Image speedBarFill;
        [SerializeField] private float vmaxKmh = 220f;

        [Header("RPM Panel")]
        [SerializeField] private TMP_Text rpmValue;
        [SerializeField] private TMP_Text rpmUnit;
        [SerializeField] private Image rpmBarFill;
        [SerializeField] private float rpmMax = 7000f;

        [Header("Visual")]
        [SerializeField, Range(0f, 1f)] private float numberSmoothing = 0.25f;
        [SerializeField] private bool showZeroPadding = false;

        private IBroadcaster _bus;
        private float _uiSpeed, _targetSpeed;
        private float _uiRpm, _targetRpm;
        private bool _subscribed;

        // Risolve il bus e prova ad auto-binda i riferimenti UI se non assegnati da Inspector/prefab.
        void Awake()
        {
            Locator.TryResolve(out _bus);
            TryAutoBind(); // prova a collegare automaticamente i riferimenti UI
        }

        // Attiva la sottoscrizione agli eventi quando il componente è abilitato.
        void OnEnable()
        {
            EnsureSubscribed();
        }

        // Rimuove le sottoscrizioni per evitare callback su oggetti disabilitati.
        void OnDisable()
        {
            Unsubscribe();
        }

        // Aggiorna valori UI con smoothing e scrive testi/unità + fill delle barre.
        void Update()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            EnsureSubscribed();

            // smoothing numeri
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(numberSmoothing), Time.unscaledDeltaTime * 60f);
            _uiSpeed = Mathf.Lerp(_uiSpeed, _targetSpeed, k);
            _uiRpm   = Mathf.Lerp(_uiRpm,   _targetRpm,   k);

            // testo
            if (speedValue) speedValue.text = FormatSpeed(_uiSpeed);
            if (rpmValue)   rpmValue.text   = Mathf.RoundToInt(_uiRpm).ToString();

            // unità (una volta)
            if (speedUnit && string.IsNullOrEmpty(speedUnit.text)) speedUnit.text = "km/h";
            if (rpmUnit   && string.IsNullOrEmpty(rpmUnit.text))   rpmUnit.text   = "RPM";

            // barre
            if (speedBarFill) speedBarFill.fillAmount = Mathf.Clamp01(_uiSpeed / Mathf.Max(1f, vmaxKmh));
            if (rpmBarFill)   rpmBarFill.fillAmount   = Mathf.Clamp01(_uiRpm   / Mathf.Max(1f, rpmMax));
        }

        // Gestori eventi “compatibili”: supporta sia eventi plain che eventi On* (nomi diversi, stesso significato).
        void OnSpeedPlain(SpeedChanged e)     { _targetSpeed = Mathf.Max(0f, e.value); }
        void OnSpeedOn(OnSpeedChanged e)      { _targetSpeed = Mathf.Max(0f, e.kmh);   }
        void OnRpmPlain(RpmChanged e)         { _targetRpm   = Mathf.Max(0f, e.value); }
        void OnRpmOn(OnRpmChanged e)          { _targetRpm   = Mathf.Max(0f, e.rpm);   }

        // Sottoscrive una sola volta gli handler al bus (idempotente).
        void EnsureSubscribed()
        {
            if (_bus == null || _subscribed) return;
            _bus.Add<SpeedChanged>(OnSpeedPlain);
            _bus.Add<OnSpeedChanged>(OnSpeedOn);
            _bus.Add<RpmChanged>(OnRpmPlain);
            _bus.Add<OnRpmChanged>(OnRpmOn);
            _subscribed = true;
        }

        // Rimuove gli handler dal bus quando non servono più.
        void Unsubscribe()
        {
            if (_bus == null || !_subscribed) return;
            _bus.Remove<SpeedChanged>(OnSpeedPlain);
            _bus.Remove<OnSpeedChanged>(OnSpeedOn);
            _bus.Remove<RpmChanged>(OnRpmPlain);
            _bus.Remove<OnRpmChanged>(OnRpmOn);
            _subscribed = false;
        }

        // Format della velocità (opzionale padding a 3 cifre).
        string FormatSpeed(float kmh)
        {
            int v = Mathf.RoundToInt(kmh);
            return showZeroPadding ? v.ToString("D3") : v.ToString();
        }

        // Auto-bind “robusto” dei riferimenti UI cercando per nome nella gerarchia.
        void TryAutoBind()
        {
            if (!speedValue)   speedValue   = FindDeep<TMP_Text>(transform, "SpeedValue");
            if (!speedUnit)    speedUnit    = FindDeep<TMP_Text>(transform, "Unit");            // speed unit
            if (!speedBarFill) speedBarFill = FindDeep<Image>(transform,    "SpeedBar");

            if (!rpmValue)     rpmValue     = FindDeep<TMP_Text>(transform, "RpmValue");
            if (!rpmUnit)      rpmUnit      = FindDeep<TMP_Text>(transform, "Unit");            // rpm unit (se i nomi coincidono, assegna tu a mano)
            if (!rpmBarFill)   rpmBarFill   = FindDeep<Image>(transform,    "RpmBar");
        }

        // Utility: ricerca nei figli un Transform con nome esatto e ritorna il componente richiesto.
        static T FindDeep<T>(Transform root, string name) where T : Component
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t.GetComponent<T>();
            return null;
        }
    }
}
