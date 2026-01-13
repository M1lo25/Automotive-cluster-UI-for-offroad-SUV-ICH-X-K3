//Gestisce il bottone Day/Night per la UI,aggiornando testo e icona in base al tema corrente.

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class NightModeButton : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TMP_Text labelState;
        [SerializeField] private Image iconState;
        [SerializeField] private Sprite sunIcon;
        [SerializeField] private Sprite moonIcon;

        [Header("Texts")]
        [SerializeField] private string dayText = "DAY";
        [SerializeField] private string nightText = "NIGHT";

        [Header("Colors")]
        [SerializeField] private Color labelDayColor = new Color32(13,13,13,255);
        [SerializeField] private Color iconDayColor = new Color32(255,255,255,255);
        [SerializeField] private Color nightColor = new Color32(255,255,255,255);

        [Header("Hotkey")]
        [SerializeField] private bool useHotkey = true;
        [SerializeField] private KeyCode hotkey = KeyCode.F4;

        private IThemeService _theme;
        private bool _isNight = false;

        //Risolve il ThemeService registrandosi all'evento Changed e allinea testo e icona allo stato attuale
        void OnEnable()
        {
            Locator.TryResolve(out _theme);
            if (_theme != null) _theme.Changed += OnThemeChanged;
            SyncVisuals();
        }

        //Deregistra l'evento per evitare listener duplicati quando l'oggetto viene disabilitato
        void OnDisable()
        {
            if (_theme != null) _theme.Changed -= OnThemeChanged;
        }

        //Gestisce,se abilitata,l'hotkey per fare toggle del night mode
        void Update()
        {
            if (!useHotkey) return;
            if (Input.GetKeyDown(hotkey)) Toggle();
        }

        //Toggle pubblico,se esiste il ThemeService concreto usa il suo Toggle(),altrimenti gestisce uno stato locale
        public void Toggle()
        {
            var concrete = _theme as ThemeService;
            if (concrete != null) { concrete.Toggle(); return; }
            _isNight = !_isNight;
            SyncVisuals();
        }

        //Forza lo stato night o day dall'esterno e aggiorna l'aspetto
        public void SetIsNight(bool value)
        {
            _isNight = value;
            SyncVisuals();
        }

        //Callback del ThemeService che deduce se è night dal nome del ThemeSO e aggiorna la UI
        void OnThemeChanged(ThemeSO theme)
        {
            _isNight = theme && theme.name.IndexOf("night", StringComparison.OrdinalIgnoreCase) >= 0;
            SyncVisuals();
        }

        //Applica a testo e icona sprite o colori corretti in base a _isNight
        void SyncVisuals()
        {
            if (labelState != null)
            {
                labelState.text = _isNight ? nightText : dayText;
                labelState.color = _isNight ? nightColor : labelDayColor;
            }

            if (iconState != null)
            {
                iconState.enabled = true;
                iconState.sprite = _isNight ? moonIcon : sunIcon;
                iconState.color = _isNight ? nightColor : iconDayColor;
            }
        }
    }
}
