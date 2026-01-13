//Gestisce la UI di un pulsante mode:applica i colori del tema corrente 
//e aggiorna bg,icona,testo in base allo stato On/Off

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class ModeButton : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] Image bg;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text label;

        [Header("Slots")]
        [SerializeField] ThemeApplier.Slot bgActive   = ThemeApplier.Slot.Accent;
        [SerializeField] ThemeApplier.Slot fgActive   = ThemeApplier.Slot.PrimaryText;
        [SerializeField] ThemeApplier.Slot bgInactive = ThemeApplier.Slot.GaugeBackground;
        [SerializeField] ThemeApplier.Slot fgInactive = ThemeApplier.Slot.SecondaryText;

        bool _isOn;
        IThemeService _themeSvc;

        //Aggancia il servizio tema e forza un re apply leggermente in ritardo per evitare problemi 
        //di ordine e inizializzazione UI
        void OnEnable()
        {
            Locator.TryResolve(out _themeSvc);
            if (_themeSvc != null)
            {
                _themeSvc.Changed        += OnThemeChanged;
                _themeSvc.OnThemeChanged += OnThemeChanged;
            }
            StartCoroutine(ReapplyDeferred());
        }

        //Sgancia gli eventi per evitare leak e callback su oggetti disabilitati o distrutti
        void OnDisable()
        {
            if (_themeSvc != null)
            {
                _themeSvc.Changed        -= OnThemeChanged;
                _themeSvc.OnThemeChanged -= OnThemeChanged;
            }
        }

        //Callback quando cambia il tema:rimanda l'aggiornamento a fine frame
        void OnThemeChanged(ThemeSO _)
        {
            StartCoroutine(ReapplyDeferred());
        }

        //Aspetta un frame,poi auto-binda i riferimenti e riapplica lo stato On/Off con i colori del tema corrente
        IEnumerator ReapplyDeferred()
        {
            yield return null;
            if (!this || !gameObject.activeInHierarchy) yield break;
            TryAutoBind();
            SetOn(_isOn);
        }

        //Imposta lo stato del bottone e applica i colori bg,icon,label usando gli slot configurati
        public void SetOn(bool on)
        {
            _isOn = on;
            var theme = _themeSvc != null ? _themeSvc.Current : null;
            if (theme == null) return;

            if (on)
            {
                if (bg)    bg.color    = Pick(theme, bgActive);
                if (icon)  icon.color  = Pick(theme, fgActive);
                if (label) label.color = Pick(theme, fgActive);
            }
            else
            {
                if (bg)    bg.color    = Pick(theme, bgInactive);
                if (icon)  icon.color  = Pick(theme, fgInactive);
                if (label) label.color = Pick(theme, fgInactive);
            }
        }

        //Mappa uno slot logico del tema sul colore effettivo nel ThemeSO
        static Color Pick(ThemeSO t, ThemeApplier.Slot slot)
        {
            switch (slot)
            {
                case ThemeApplier.Slot.Background:      return t.Background;
                case ThemeApplier.Slot.PrimaryText:     return t.PrimaryText;
                case ThemeApplier.Slot.SecondaryText:   return t.SecondaryText;
                case ThemeApplier.Slot.Accent:          return t.Accent;
                case ThemeApplier.Slot.GaugeFill:       return t.GaugeFill;
                case ThemeApplier.Slot.GaugeBackground: return t.GaugeBackground;
                case ThemeApplier.Slot.Warning:         return t.Warning;
                default: return Color.magenta;
            }
        }

        //Prova ad auto-assegnare bg,icon,label cercando figli con nomi contenenti le keyword previste
        void TryAutoBind()
        {
            if (!bg)    bg    = FindImage(transform, "BG");
            if (!icon)  icon  = FindImage(transform, "Icon");
            if (!label) label = FindLabel(transform, "Label");
        }

        //Cerca un componente Image nei figli per nome con un fallback sul primo Image trovato
        static Image FindImage(Transform root, string key)
        {
            var imgs = root.GetComponentsInChildren<Image>(true);
            foreach (var i in imgs) if (i && i.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0) return i;
            return imgs.Length > 0 ? imgs[0] : null;
        }

        //Cerca un TMP_Text nei figli per nome con un fallback sul primo TMP_Text trovato
        static TMP_Text FindLabel(Transform root, string key)
        {
            var labs = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in labs) if (t && t.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0) return t;
            return labs.Length > 0 ? labs[0] : null;
        }
    }
}
