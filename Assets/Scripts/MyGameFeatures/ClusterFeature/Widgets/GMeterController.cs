// Gestisce il G-Meter UI: ascolta la G (laterale/longitudinale) dal bus, muove la “bolla” dentro un cerchio con clamp e smoothing, e aggiorna le etichette X/Y.
using UnityEngine;
using TMPro;

namespace ICXK3
{
    // NOTA: gli eventi (OnGChanged, ecc.) sono ora in Events.cs

    public class GMeterController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] RectTransform circleBg;
        [SerializeField] RectTransform bubble;
        [SerializeField] TMP_Text      labelX;
        [SerializeField] TMP_Text      labelY;

        [Header("Ranges")]
        [SerializeField] float gMax = 1.0f;
        [SerializeField] float uiPadding = 6f;

        [Header("Smoothing")]
        [SerializeField, Range(0f,1f)] float lerpFactor = 0.20f;

        [Header("Vertical Response")]
        [SerializeField] float yGain = 1.2f;
        [SerializeField] float yBiasPixels = 6f;

        IBroadcaster _bus;
        bool     _subscribed;
        Vector2  _gTarget;
        Vector2  _gUi;
        float    _radiusPx;

        // Auto-binda i riferimenti UI, risolve il bus e calcola il raggio utile del cerchio.
        void Awake()
        {
            AutoBind();
            Locator.TryResolve(out _bus);
            RecomputeRadius();
        }

        // Attiva la sottoscrizione all’evento G e ricalcola il raggio (utile se layout cambia).
        void OnEnable()
        {
            TrySubscribe();
            RecomputeRadius();
        }

        // Rimuove la sottoscrizione al bus per evitare callback quando il componente è disabilitato.
        void OnDisable()
        {
            if (_bus != null && _subscribed)
            {
                _bus.Remove<OnGChanged>(OnG);
                _subscribed = false;
            }
        }

        // Applica smoothing, clamp a gMax, conversione g->pixel e muove la bolla; aggiorna label X/Y.
        void Update()
        {
            if (!_subscribed) TrySubscribe();
            if (circleBg) RecomputeRadius();

            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(lerpFactor), Time.unscaledDeltaTime * 60f);
            _gUi = Vector2.Lerp(_gUi, _gTarget, k);

            Vector2 clamped = Vector2.ClampMagnitude(_gUi, gMax);
            float   scale   = _radiusPx / Mathf.Max(0.0001f, gMax);

            float xPx = clamped.x * scale;

            // Curva non-lineare sull’asse Y + bias per separare visivamente le direzioni (+/-).
            float yNorm = Mathf.Sign(clamped.y) * Mathf.Pow(Mathf.Abs(clamped.y), 0.85f);
            float yPx   = (yNorm * yGain) * scale + Mathf.Max(0f, yBiasPixels * Mathf.Sign(yNorm));

            if (bubble) bubble.anchoredPosition = new Vector2(xPx, yPx);

            if (labelX) labelX.text = $"X:{clamped.x:+0.00;-0.00;0.00}g";
            if (labelY) labelY.text = $"Y:{clamped.y:+0.00;-0.00;0.00}g";
        }

        // Garantisce la sottoscrizione al bus (idempotente) e risolve il bus se manca.
        void TrySubscribe()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            if (_bus != null && !_subscribed)
            {
                _bus.Add<OnGChanged>(OnG);
                _subscribed = true;
            }
        }

        // Handler evento: memorizza la G target che verrà filtrata nello Update.
        void OnG(OnGChanged e) { _gTarget = e.g; }

        // Ricalcola il raggio in pixel del cerchio (min(width,height)/2) meno padding UI.
        void RecomputeRadius()
        {
            if (!circleBg) return;
            var r = circleBg.rect;
            _radiusPx = Mathf.Max(0f, Mathf.Min(r.width, r.height) * 0.5f - uiPadding);
        }

        // Auto-bind per nome dei figli (utile se prefab non ha riferimenti assegnati).
        void AutoBind()
        {
            var t = transform;
            if (!circleBg) circleBg = t.Find("CircleBg")?.GetComponent<RectTransform>();
            if (!bubble)   bubble   = t.Find("Bubble")?.GetComponent<RectTransform>();
            if (!labelX)   labelX   = t.Find("LabelX")?.GetComponent<TMP_Text>();
            if (!labelY)   labelY   = t.Find("LabelY")?.GetComponent<TMP_Text>();
        }
    }
}
