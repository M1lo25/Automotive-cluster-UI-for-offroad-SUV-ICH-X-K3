//Applica automaticamente i colori del tema "ThemeSO" a componenti UI 
//permettendo di scegliere un colore e si aggiorna quando il tema cambia

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThemeApplier : MonoBehaviour
{
    public enum Slot
    {
        Background = 0,
        PrimaryText = 1,
        SecondaryText = 2,
        Accent = 3,
        GaugeFill = 4,
        GaugeBackground = 5,
        Warning = 6,
        BackgroundContent = 7
    }

    [Header("Colore")]
    public Slot ColorSlot = Slot.Accent;

    [Header("Targets")]
    public Image imageTarget;
    public TMP_Text tmpTextTarget;
    public Graphic genericGraphic; 
    public bool applyOnEnable = true;

    IThemeService _theme;

    //Viene effettuato l'autobind dei target che se non assegnati in Inspector,
    //prova a prendere i component sul GameObject corrente
    void Awake()
    {
        if (imageTarget == null) imageTarget = GetComponent<Image>();
        if (tmpTextTarget == null) tmpTextTarget = GetComponent<TMP_Text>();
        if (genericGraphic == null) genericGraphic = GetComponent<Graphic>();
    }

    //Risolve il servizio tema,si registra al cambio tema e applica,se necessario subito il tema corrente
    void OnEnable()
    {
        _theme = Locator.Resolve<IThemeService>();
        _theme.OnThemeChanged += Apply;
        if (applyOnEnable) Apply(_theme.Current);
    }

    //Deregistra il listener per evitare duplicazioni quando il component viene disabilitato
    void OnDisable()
    {
        if (_theme != null) _theme.OnThemeChanged -= Apply;
    }

    //Seleziona il colore corretto dal ThemeSO in base allo slot scelto
    Color Pick(ThemeSO t)
    {
        switch (ColorSlot)
        {
            case Slot.Background:       return t.Background;
            case Slot.PrimaryText:      return t.PrimaryText;
            case Slot.SecondaryText:    return t.SecondaryText;
            case Slot.Accent:           return t.Accent;
            case Slot.GaugeFill:        return t.GaugeFill;
            case Slot.GaugeBackground:  return t.GaugeBackground;
            case Slot.Warning:          return t.Warning;
            case Slot.BackgroundContent:return t.BackgroundContent;
            default: return Color.magenta;
        }
    }

    //Applica il colore selezionato ai target disponibili tra cui Image,TMP_Text,Graphic
    public void Apply(ThemeSO t)
    {
        var c = Pick(t);
        if (imageTarget != null)      imageTarget.color = c;
        if (tmpTextTarget != null)    tmpTextTarget.color = c;
        if (genericGraphic != null)   genericGraphic.color = c;
    }
}
