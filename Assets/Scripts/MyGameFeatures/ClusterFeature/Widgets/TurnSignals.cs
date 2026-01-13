// Gestisce le frecce in UI (sinistra/destra/4-frecce): collega bottoni e hotkey, fa lampeggiare le icone con coroutine e broadcasta lo stato tramite evento.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ICXK3
{
    public enum TurnSignalState { Off, Left, Right, Hazard }
    public struct OnTurnSignalChanged
    {
        public TurnSignalState state;
        public OnTurnSignalChanged(TurnSignalState s) { state = s; }
    }

    public class TurnSignals : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button hazardButton;

        [Header("Icons")]
        [SerializeField] private Graphic leftIcon;
        [SerializeField] private Graphic rightIcon;
        [SerializeField] private Graphic hazardIcon;

        [Header("Colors")]
        [SerializeField] private Color inactiveColor = new Color32(168, 179, 188, 255);
        [SerializeField] private Color activeColor   = new Color32(35, 227, 210, 255);
        [SerializeField] private bool useIconOriginalColorsAsInactive = true;

        [Header("Blink")]
        [SerializeField, Range(0.05f, 1f)] private float onTime  = 0.25f;
        [SerializeField, Range(0.05f, 1f)] private float offTime = 0.25f;

        [Header("Hotkeys")]
        [SerializeField] private bool enableHotkeys = true;
        [SerializeField] private KeyCode keyLeft   = KeyCode.J;
        [SerializeField] private KeyCode keyHazard = KeyCode.X;
        [SerializeField] private KeyCode keyRight  = KeyCode.L;

        private IBroadcaster _bus;
        private Coroutine _blinkCo;
        private TurnSignalState _state = TurnSignalState.Off;

        private Color _leftBase, _rightBase, _hazardBase;

        // Risolve bus e riferimenti UI (anche via Find), collega i click dei bottoni e inizializza lo stato visivo.
        void Awake()
        {
            Locator.TryResolve(out _bus);

            if (!leftIcon)   leftIcon   = transform.Find("Header/LeftArrowCircle/LeftArrow")?.GetComponent<Graphic>();
            if (!rightIcon)  rightIcon  = transform.Find("Header/RightArrowCircle/RightArrow")?.GetComponent<Graphic>();
            if (!hazardIcon) hazardIcon = transform.Find("Header/HazardCircle/Hazard")?.GetComponent<Graphic>();

            if (!leftIcon)   leftIcon   = FindGraphicByName(transform, "LeftArrow");
            if (!rightIcon)  rightIcon  = FindGraphicByName(transform, "RightArrow");
            if (!hazardIcon) hazardIcon = FindGraphicByName(transform, "Hazard");

            if (leftIcon)   _leftBase   = leftIcon.color;
            if (rightIcon)  _rightBase  = rightIcon.color;
            if (hazardIcon) _hazardBase = hazardIcon.color;

            if (!leftButton)   leftButton   = leftIcon   ? leftIcon.GetComponentInParent<Button>()   : null;
            if (!rightButton)  rightButton  = rightIcon  ? rightIcon.GetComponentInParent<Button>()  : null;
            if (!hazardButton) hazardButton = hazardIcon ? hazardIcon.GetComponentInParent<Button>() : null;

            if (leftButton)   leftButton.onClick.AddListener(() => Toggle(TurnSignalState.Left));
            if (rightButton)  rightButton.onClick.AddListener(() => Toggle(TurnSignalState.Right));
            if (hazardButton) hazardButton.onClick.AddListener(() => Toggle(TurnSignalState.Hazard));

            ApplyVisuals();
        }

        // Gestione hotkey per toggle rapido delle frecce (se abilitate).
        void Update()
        {
            if (!enableHotkeys) return;
            if (Input.GetKeyDown(keyLeft))   Toggle(TurnSignalState.Left);
            if (Input.GetKeyDown(keyHazard)) Toggle(TurnSignalState.Hazard);
            if (Input.GetKeyDown(keyRight))  Toggle(TurnSignalState.Right);
        }

        // Riapplica lo stato quando riabilitato (utile se l’oggetto viene riattivato).
        void OnEnable()  => ApplyVisuals();

        // Stoppa il blink quando disabilitato per evitare coroutine attive.
        void OnDisable() => StopBlink();

        // Toggle: se lo stesso stato è già attivo spegne, altrimenti attiva quello richiesto.
        public void Toggle(TurnSignalState requested)
        {
            _state = (_state == requested) ? TurnSignalState.Off : requested;
            Broadcast();
            ApplyVisuals();
        }

        // Imposta direttamente lo stato e sincronizza bus + UI.
        public void SetState(TurnSignalState s)
        {
            _state = s;
            Broadcast();
            ApplyVisuals();
        }

        // Forza lo spegnimento (Off) e sincronizza bus + UI.
        public void Clear()
        {
            _state = TurnSignalState.Off;
            Broadcast();
            ApplyVisuals();
        }

        // Broadcast dello stato corrente sul bus eventi.
        private void Broadcast()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            _bus?.Broadcast(new OnTurnSignalChanged(_state));
        }

        // Applica colori base/inattivi e avvia la coroutine di blink corretta in base allo stato.
        private void ApplyVisuals()
        {
            if (leftIcon)   leftIcon.color   = useIconOriginalColorsAsInactive ? _leftBase   : inactiveColor;
            if (rightIcon)  rightIcon.color  = useIconOriginalColorsAsInactive ? _rightBase  : inactiveColor;
            if (hazardIcon) hazardIcon.color = useIconOriginalColorsAsInactive ? _hazardBase : inactiveColor;

            StopBlink();

            switch (_state)
            {
                case TurnSignalState.Left:   _blinkCo = StartCoroutine(BlinkHard(leftIcon));                 break;
                case TurnSignalState.Right:  _blinkCo = StartCoroutine(BlinkHard(rightIcon));                break;
                case TurnSignalState.Hazard: _blinkCo = StartCoroutine(BlinkPairHard(leftIcon, rightIcon));  break;
                default: break;
            }
        }

        // Coroutine blink singola: alterna active/inactive usando WaitForSecondsRealtime.
        private IEnumerator BlinkHard(Graphic g)
        {
            if (!g) yield break;
            while (true)
            {
                g.color = activeColor;
                yield return new WaitForSecondsRealtime(onTime);
                g.color = useIconOriginalColorsAsInactive ? _leftBase : inactiveColor;
                yield return new WaitForSecondsRealtime(offTime);
            }
        }

        // Coroutine blink doppia: lampeggia insieme due icone (usata per hazard).
        private IEnumerator BlinkPairHard(Graphic a, Graphic b)
        {
            while (true)
            {
                if (a) a.color = activeColor;
                if (b) b.color = activeColor;
                yield return new WaitForSecondsRealtime(onTime);

                if (a) a.color = useIconOriginalColorsAsInactive ? _leftBase : inactiveColor;
                if (b) b.color = useIconOriginalColorsAsInactive ? _rightBase : inactiveColor;
                yield return new WaitForSecondsRealtime(offTime);
            }
        }

        // Ferma la coroutine di blink corrente (se presente).
        private void StopBlink()
        {
            if (_blinkCo != null) StopCoroutine(_blinkCo);
            _blinkCo = null;
        }

        // Utility: cerca un Graphic nei figli per nome esatto.
        private static Graphic FindGraphicByName(Transform root, string name)
        {
            foreach (var g in root.GetComponentsInChildren<Graphic>(true))
                if (g.name == name) return g;
            return null;
        }
    }
}
