// Gestisce il selettore PRND in UI: evidenzia la marcia corrente, muove una “pill” sul label selezionato e aggiorna il numero marcia in D, adattandosi anche al tema (day/night).
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ICXK3
{
    public class GearController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private RectTransform pill;
        [SerializeField] private TMP_Text pillText;
        [SerializeField] private RectTransform labelP;
        [SerializeField] private RectTransform labelR;
        [SerializeField] private RectTransform labelN;
        [SerializeField] private RectTransform labelD;

        [Header("Style")]
        [SerializeField] private Color labelNormal = new Color32(123,138,150,255);
        [SerializeField] private Color selectedDayColor = new Color32(13,13,13,255);
        [SerializeField] private Color selectedNightColor = new Color32(255,255,255,255);
        [SerializeField] private float moveDuration = 0.25f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);

        private TMP_Text _txtP, _txtR, _txtN, _txtD;
        private int _curIndex = 0;
        private Coroutine _moveCo;
        private IThemeService _theme;
        private bool _isNight = false;

        // Cache dei TMP_Text sui label, inizializza highlight e nasconde la pill al boot.
        void Awake()
        {
            _txtP = labelP ? labelP.GetComponent<TMP_Text>() : null;
            _txtR = labelR ? labelR.GetComponent<TMP_Text>() : null;
            _txtN = labelN ? labelN.GetComponent<TMP_Text>() : null;
            _txtD = labelD ? labelD.GetComponent<TMP_Text>() : null;
            Highlight();
            if (pill) pill.gameObject.SetActive(false);
        }

        // Aggancia il servizio tema e riallinea lo stato visuale all’abilitazione.
        void OnEnable()
        {
            Locator.TryResolve(out _theme);
            if (_theme != null) _theme.Changed += OnThemeChanged;
            Highlight();
            UpdatePillVisibility();
        }

        // Sgancia l’evento tema per evitare callback su oggetto disabilitato.
        void OnDisable()
        {
            if (_theme != null) _theme.Changed -= OnThemeChanged;
        }

        // Imposta il selettore PRND (char), aggiorna highlight e anima lo spostamento della pill.
        public void SetSelector(char selPRND)
        {
            int idx = selPRND switch { 'P'=>0, 'R'=>1, 'N'=>2, 'D'=>3, _=>_curIndex };
            if (idx == _curIndex) { UpdatePillVisibility(); return; }
            _curIndex = idx;
            Highlight();
            MovePillToCurrent();
            UpdatePillVisibility();
        }

        // Imposta il numero marcia mostrato nella pill (solo in D), altrimenti la nasconde.
        public void SetDriveGearNumber(int n)
        {
            if (!pillText) return;
            if (_curIndex != 3 || n <= 0)
            {
                pillText.text = "";
                if (pill) pill.gameObject.SetActive(false);
                return;
            }
            pillText.text = Mathf.Clamp(n, 1, 7).ToString();
            if (pill && !pill.gameObject.activeSelf) pill.gameObject.SetActive(true);
        }

        // Mostra la pill solo in D e gestisce un valore di default quando è visibile.
        void UpdatePillVisibility()
        {
            if (!pill) return;
            bool show = _curIndex == 3;
            pill.gameObject.SetActive(show);
            if (!show && pillText) pillText.text = "";
            if (show && pillText && string.IsNullOrEmpty(pillText.text)) pillText.text = "1";
        }

        // Evidenzia il label selezionato con colore dipendente dal tema (day/night).
        void Highlight()
        {
            var active = _isNight ? selectedNightColor : selectedDayColor;
            if (_txtP) _txtP.color = (_curIndex==0)? active : labelNormal;
            if (_txtR) _txtR.color = (_curIndex==1)? active : labelNormal;
            if (_txtN) _txtN.color = (_curIndex==2)? active : labelNormal;
            if (_txtD) _txtD.color = (_curIndex==3)? active : labelNormal;
        }

        // Calcola la posizione target della pill in locale e avvia la coroutine di animazione sul solo asse X.
        void MovePillToCurrent()
        {
            if (!pill) return;
            RectTransform target = _curIndex switch { 0=>labelP, 1=>labelR, 2=>labelN, 3=>labelD, _=>null };
            if (!target) return;

            Vector3 world = target.TransformPoint(Vector3.zero);
            Vector3 local = pill.parent.InverseTransformPoint(world);
            float startX = pill.anchoredPosition.x;
            float endX = local.x;

            if (_moveCo != null) StopCoroutine(_moveCo);
            _moveCo = StartCoroutine(CoMove(startX, endX));
        }

        // Coroutine: anima lo spostamento con easing e usando unscaledDeltaTime (indipendente dal timeScale).
        IEnumerator CoMove(float fromX, float toX)
        {
            float t = 0f;
            var start = new Vector2(fromX, pill.anchoredPosition.y);
            var end   = new Vector2(toX,   pill.anchoredPosition.y);
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.001f, moveDuration);
                float k = ease.Evaluate(Mathf.Clamp01(t));
                pill.anchoredPosition = Vector2.LerpUnclamped(start, end, k);
                yield return null;
            }
            pill.anchoredPosition = end;
        }

        // Determina “night mode” dal nome del tema e aggiorna colori/visibilità di conseguenza.
        void OnThemeChanged(ThemeSO theme)
        {
            _isNight = theme && theme.name.IndexOf("night", StringComparison.OrdinalIgnoreCase) >= 0;
            Highlight();
            UpdatePillVisibility();
        }
    }
}
