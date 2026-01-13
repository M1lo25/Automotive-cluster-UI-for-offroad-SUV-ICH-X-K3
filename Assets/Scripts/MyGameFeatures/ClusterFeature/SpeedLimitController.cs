//Controller UI per il limite di velocità.
//Mostra il limite in km/h e segnala quando la velocità lo supera.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class SpeedLimitController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image pillBg;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        [Header("Colors")]
        [SerializeField] private Color normalBg = new Color(0.16f, 0.18f, 0.22f, 1f);
        [SerializeField] private Color normalText = Color.white;
        [SerializeField] private Color alertBg = new Color(0.85f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color alertText = Color.white;

        [Header("Limit (km/h)")]
        [SerializeField] private int limitKmh = 90;

        [Header("Preset Keys")]
        [SerializeField] private KeyCode set30  = KeyCode.Alpha1;
        [SerializeField] private KeyCode set50  = KeyCode.Alpha2;
        [SerializeField] private KeyCode set70  = KeyCode.Alpha3;
        [SerializeField] private KeyCode set90  = KeyCode.Alpha4;
        [SerializeField] private KeyCode set110 = KeyCode.Alpha5;
        [SerializeField] private KeyCode set130 = KeyCode.Alpha6;

        //Costanti del lampeggio quando si supera il limite.
        const float FLASH_HZ = 1.0f;
        const float FLASH_MIN_ALPHA = 0.65f;
        const float FLASH_MAX_ALPHA = 1f;

        private IBroadcaster _bus;
        private bool _subscribed;
        private float _speedKmh;
        private Color _cachedAlertBg, _cachedNormalBg;

        //Setup iniziale con autobind UI,resolve bus,cache colori e aggiornamento del testo limite
        void Awake()
        {
            AutoBindIfNeeded();
            Locator.TryResolve(out _bus);
            _cachedAlertBg = alertBg;
            _cachedNormalBg = pillBg ? pillBg.color : normalBg;
            UpdateValueText();
            ApplyStaticNormal();
        }

        //Attiva la sottoscrizione agli eventi di velocità quando il component è abilitato
        void OnEnable()
        {
            TrySubscribe();
        }

        //Disiscrive il listener per evitare duplicazioni quando il component viene disabilitato
        void OnDisable()
        {
            if (_bus != null && _subscribed)
            {
                _bus.Remove<OnSpeedChanged>(OnSpeed);
                _subscribed = false;
            }
        }

        //Gestisce preset da tastiera e aggiorna la UI ogni frame
        void Update()
        {
            if (!_subscribed) TrySubscribe();

            if (Input.GetKeyDown(set30))  SetLimit(30);
            if (Input.GetKeyDown(set50))  SetLimit(50);
            if (Input.GetKeyDown(set70))  SetLimit(70);
            if (Input.GetKeyDown(set90))  SetLimit(90);
            if (Input.GetKeyDown(set110)) SetLimit(110);
            if (Input.GetKeyDown(set130)) SetLimit(130);

            UpdateVisual();
        }

        //Prova a risolvere e registrare il listener sull'event bus una sola volta
        void TrySubscribe()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            if (_bus != null && !_subscribed)
            {
                _bus.Add<OnSpeedChanged>(OnSpeed);
                _subscribed = true;
            }
        }

        //Callback evento che aggiorna l'ultima velocità letta
        void OnSpeed(OnSpeedChanged e)
        {
            _speedKmh = e.kmh;
        }

        //Applica feedback,infatti se oltre limite lampeggia e forza colori alert,altrimenti stato normale
        void UpdateVisual()
        {
            bool over = _speedKmh > limitKmh;

            if (over)
            {
                float a = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * FLASH_HZ) + 1f) * 0.5f;
                a = Mathf.Lerp(FLASH_MIN_ALPHA, FLASH_MAX_ALPHA, a);

                if (pillBg)
                {
                    var c = _cachedAlertBg; c.a = a;
                    pillBg.color = c;
                }
                if (valueText) valueText.color = alertText;
                if (labelText) labelText.color = alertText;
            }
            else
            {
                ApplyStaticNormal();
            }
        }

        //Ripristina colori standard (nessun lampeggio) usando il background cached
        void ApplyStaticNormal()
        {
            if (pillBg)    pillBg.color = _cachedNormalBg;
            if (valueText) valueText.color = normalText;
            if (labelText) labelText.color = normalText;
        }

        //Imposta il limite in modo sicuro e aggiorna subito il testo mostrato
        public void SetLimit(int kmh)
        {
            limitKmh = Mathf.Clamp(kmh, 0, 200);
            UpdateValueText();
        }

        //Aggiorna la label numerica del limite nella UI
        void UpdateValueText()
        {
            if (valueText) valueText.text = limitKmh.ToString();
        }

        //Se i campi UI non sono assegnati in Inspector,si prova a recuperarli dalla gerarchia cercando
        //nei figli PillBG,Label,Value e,per pillBg,si usa come fallback l'Image sul GameObject corrente
        void AutoBindIfNeeded()
        {
            var r = transform;
            if (!pillBg)
            {
                pillBg = r.Find("PillBG")?.GetComponent<Image>();
                if (!pillBg) pillBg = r.GetComponent<Image>();
            }
            if (!labelText) labelText = r.Find("Label")?.GetComponent<TMP_Text>();
            if (!valueText) valueText = r.Find("Value")?.GetComponent<TMP_Text>();
        }

    }
}
