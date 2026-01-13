using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//Gestisce i bottoni delle modalità,aggiorna UI,invia TerrainModeChanged e lampeggia il LED

namespace ICXK3
{
    public class ModeButtonsController : MonoBehaviour
    {
        [Header("Buttons")]
        // Riferimenti ai bottoni per le modalitàassegnabili da Inspector)
        [SerializeField] private Button btnRoad;
        [SerializeField] private Button btnTrail;
        [SerializeField] private Button btnSnow;

        [Header("Button BG images")]
        //Immagini di background dei bottoni
        [SerializeField] private Image bgRoad;
        [SerializeField] private Image bgTrail;
        [SerializeField] private Image bgSnow;

        [Header("Colors")]
        // Colori per stato normal o selected dei bottoni
        [SerializeField] private Color normalColor = new Color(0.1f, 0.1f, 0.13f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.2f, 0.3f, 0.6f, 1f);

        [Header("Header LED")]
        //LED e bagliore per feedback visivo,blinkDuration e blinkCount definiscono il lampeggio
        [SerializeField] private Image led;
        [SerializeField] private Image ledGlow;
        [SerializeField] private float blinkDuration = 0.6f;
        [SerializeField] private int blinkCount = 2;

        [Header("Mode Assets")]
        //ScriptableObject delle tre modalità controllate dai bottoni
        [SerializeField] private TerrainModeSO road;
        [SerializeField] private TerrainModeSO trail;
        [SerializeField] private TerrainModeSO snow;

        [Header("Hotkeys")]
        //Tasti rapidi per selezionare le modalità da tastiera
        [SerializeField] private KeyCode keyRoad = KeyCode.F1;
        [SerializeField] private KeyCode keyTrail = KeyCode.F2;
        [SerializeField] private KeyCode keySnow = KeyCode.F3;

        private IBroadcaster _bus; //Bus eventi applicativo che viene risolto da Locator
        private Coroutine _blinkCo;  //Handle della coroutine di blink,per evitare overlap

        private void Awake()
        {
            //Risolve il bus e autocollega i riferimenti UI se non impostati 
            Locator.TryResolve(out _bus);
            AutoBindIfNeeded();

            //Registra i listener dei bottoni solo se presenti
            if (btnRoad) btnRoad.onClick.AddListener(() => SelectMode(road));
            if (btnTrail) btnTrail.onClick.AddListener(() => SelectMode(trail));
            if (btnSnow) btnSnow.onClick.AddListener(() => SelectMode(snow));
        }

        private void OnEnable()
        {
            //All'attivazione imposta uno stato visivo coerente
            if (trail) SetVisual(trail);
            else if (road) SetVisual(road);
            else if (snow) SetVisual(snow);
        }

        private void OnDisable()
        {
            //Ferma il blink in corso per evitare coroutine sospese quando il GO è disabilitato
            if (_blinkCo != null) { StopCoroutine(_blinkCo); _blinkCo = null; }
        }

        private void Update()
        {
            //Selezione modalità da tastiera
            if (Input.GetKeyDown(keyRoad)) SelectMode(road);
            if (Input.GetKeyDown(keyTrail)) SelectMode(trail);
            if (Input.GetKeyDown(keySnow)) SelectMode(snow);
        }

        //Seleziona una modalità,aggiornando lo stato dei bottoni,avvia il feedback visivo con il led e
        //Broadcast dell'evento TerrainModeChanged sul bus
        private void SelectMode(TerrainModeSO mode)
        {
            if (!isActiveAndEnabled || mode == null) return;

            SetVisual(mode);

            if (_bus == null) Locator.TryResolve(out _bus);
            _bus?.Broadcast(new TerrainModeChanged(mode));

            if (_blinkCo != null) StopCoroutine(_blinkCo);
            _blinkCo = StartCoroutine(BlinkLed());
        }

        //Aggiorna i colori di background in base alla modalità selezionata
        private void SetVisual(TerrainModeSO mode)
        {
            if (bgRoad) bgRoad.color = (mode == road) ? selectedColor : normalColor;
            if (bgTrail) bgTrail.color = (mode == trail) ? selectedColor : normalColor;
            if (bgSnow) bgSnow.color = (mode == snow) ? selectedColor : normalColor;
        }

        //Effetto di lampeggio del LED che alterna alpha tra pieno e attenuato per i cicli del 'blinkCount'
        private IEnumerator BlinkLed()
        {
            if (!led && !ledGlow) yield break;

            float per = blinkDuration / (blinkCount * 2f);
            for (int i = 0; i < blinkCount; i++)
            {
                SetLedAlpha(1f);
                yield return new WaitForSecondsRealtime(per);
                SetLedAlpha(0.2f);
                yield return new WaitForSecondsRealtime(per);
            }
            SetLedAlpha(1f);
        }

        //Imposta l'alpha del LED e del suo bagliore 
        private void SetLedAlpha(float a)
        {
            if (led)
            {
                var c = led.color; c.a = a; led.color = c;
            }
            if (ledGlow)
            {
                var c = ledGlow.color; c.a = a * 0.6f; ledGlow.color = c;
            }
        }

        //Autocollegamento dei riferimenti UI se non assegnati,cercando per percorso relativo ai figli
        private void AutoBindIfNeeded()
        {
            var root = transform;

            if (!btnRoad) btnRoad = root.Find("Content/BtnModePrefabRoad")?.GetComponent<Button>();
            if (!btnTrail) btnTrail = root.Find("Content/BtnModePrefabTrail")?.GetComponent<Button>();
            if (!btnSnow) btnSnow = root.Find("Content/BtnModePrefabSnow")?.GetComponent<Button>();

            if (!bgRoad) bgRoad = root.Find("Content/BtnModePrefabRoad/BGRoad")?.GetComponent<Image>();
            if (!bgTrail) bgTrail = root.Find("Content/BtnModePrefabTrail/BGTrail")?.GetComponent<Image>();
            if (!bgSnow) bgSnow = root.Find("Content/BtnModePrefabSnow/BGSnow")?.GetComponent<Image>();

            if (!led) led = root.Find("Header/LED")?.GetComponent<Image>();
            if (!ledGlow) ledGlow = root.Find("Header/LEDGlow")?.GetComponent<Image>();
        }
    }
}
