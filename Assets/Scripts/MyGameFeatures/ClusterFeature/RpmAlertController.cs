//Gestisce lo stato di allerta RPM per un widget UI in Unity.
//Il suo funzionamento consiste nel quando gli RPM superano una soglia,
//cambia colori e fa lampeggiare lo sfondo

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class RpmAlertController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image pillBg;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        [Header("Colors")]
        [SerializeField] private Color normalBg = new Color(0.16f, 0.18f, 0.22f, 1f);
        [SerializeField] private Color normalText = Color.white;
        [SerializeField] private Color alertBg = new Color(0.85f, 0.10f, 0.10f, 1f);
        [SerializeField] private Color alertText = Color.white;

        [Header("Thresholds")]
        [SerializeField] private float alertRpm = 4500f;
        [SerializeField] private float hysteresis = 200f;

        [Header("Flash")]
        [SerializeField, Range(0.1f, 4f)] private float flashHz = 1.0f;
        [SerializeField, Range(0f, 1f)] private float flashMinAlpha = 0.65f;
        [SerializeField, Range(0f, 1f)] private float flashMaxAlpha = 1.00f;

        private IBroadcaster _bus;
        private bool _subscribed;
        private float _rpm;
        private bool _isAlert;
        private Color _cachedNormalBg;

        //Inizializza riferimenti UI,risolve l'event bus e imposta lo stato visivo iniziale
        void Awake()
        {
            AutoBind();
            Locator.TryResolve(out _bus);
            _cachedNormalBg = pillBg ? pillBg.color : normalBg;
            ApplyNormal();
        }

        //Attiva la sottoscrizione agli eventi quando l'oggetto viene abilitato
        void OnEnable()
        {
            TrySubscribe();
        }

        //Rimuove la sottoscrizione per evitare doppi listener quando l'oggetto viene disabilitato
        void OnDisable()
        {
            if (_bus != null && _subscribed)
            {
                _bus.Remove<OnRpmChanged>(OnRpm);
                _subscribed = false;
            }
        }

        //Mantiene viva la sottoscrizione e aggiorna il feedback visivo ogni frame
        void Update()
        {
            if (!_subscribed) TrySubscribe();
            UpdateVisual();
        }

        //Prova a risolvere e registrare il listener sull'event bus una sola volta
        void TrySubscribe()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            if (_bus != null && !_subscribed)
            {
                _bus.Add<OnRpmChanged>(OnRpm);
                _subscribed = true;
            }
        }

        //Riceve gli RPM,applica clamp e gestisce stato alert per evitare flicker.
        void OnRpm(OnRpmChanged e)
        {
            _rpm = Mathf.Max(0f, e.rpm);
            if (!_isAlert && _rpm >= alertRpm) _isAlert = true;
            else if (_isAlert && _rpm <= alertRpm - Mathf.Abs(hysteresis)) _isAlert = false;
        }

        //Aggiorna la ui in modo takle che in alert lampeggia l'alpha del background e forza i colori del testo
        void UpdateVisual()
        {
            if (_isAlert)
            {
                float t = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * flashHz) + 1f) * 0.5f;
                float a = Mathf.Lerp(flashMinAlpha, flashMaxAlpha, t);
                if (pillBg) { var c = alertBg; c.a = a; pillBg.color = c; }
                if (valueText) valueText.color = alertText;
                if (labelText) labelText.color = alertText;
            }
            else
            {
                ApplyNormal();
            }
        }

        //Ripristina lo stato normale usando colore in cache del background e colori del testo standard
        void ApplyNormal()
        {
            if (pillBg) pillBg.color = _cachedNormalBg;
            if (valueText) valueText.color = normalText;
            if (labelText) labelText.color = normalText;
        }

        //Se i riferimenti UI non sono assegnati in inspector,vengono cercati automaticamente nei figli(PillBG,Label,Value)
        //e per il background,fa fallback sull'Image presente sul GameObject corrente
        void AutoBind()
        {
            var r = transform;
            if (!pillBg) pillBg = r.Find("PillBG")?.GetComponent<Image>() ?? r.GetComponent<Image>();
            if (!labelText) labelText = r.Find("Label")?.GetComponent<TMP_Text>();
            if (!valueText) valueText = r.Find("Value")?.GetComponent<TMP_Text>();
        }

    }
}
