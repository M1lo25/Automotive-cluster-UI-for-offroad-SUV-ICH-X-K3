//Definisce un servizio di gestione tema con proprietà,eventi e metodi di controllo
using System;

public interface IThemeService
{
    ThemeSO Current { get; }
    bool IsDay { get; }
    //Notifica il cambio tema ai listener
    event Action<ThemeSO> Changed;
    //Evento legacy
    event Action<ThemeSO> OnThemeChanged;
    void SetTheme(ThemeSO theme);
    //Shortcut per impostare il tema Day
    void SetDay();
    //Shortcut per impostare il tema Night
    void SetNight();
    //Alterna Day/Night
    void Toggle();
}

public class ThemeService : IThemeService
{
    readonly ThemeSO _day;
    readonly ThemeSO _night;

    public ThemeSO Current { get; private set; }
    public bool IsDay => Current == _day;

    public event Action<ThemeSO> Changed;
    public event Action<ThemeSO> OnThemeChanged; 

    //Costruisce il servizio con i due temi e sceglie quello iniziale
    public ThemeService(ThemeSO day, ThemeSO night, bool startAsDay = true)
    {
        _day = day;
        _night = night;
        Current = startAsDay ? _day : _night;

        //Notifica immediata dello stato iniziale,che torna utile per chi si registra prima della UI
        Changed?.Invoke(Current);
        OnThemeChanged?.Invoke(Current);
    }

    //Imposta un nuovo tema e invoca gli eventi di notifica.
    public void SetTheme(ThemeSO theme)
    {
        if (theme == null || theme == Current) return;
        Current = theme;

        Changed?.Invoke(Current);
        OnThemeChanged?.Invoke(Current);
    }

    //Imposta Day usando lo shortcut
    public void SetDay()   => SetTheme(_day);

    //Imposta Night usando lo shortcut
    public void SetNight() => SetTheme(_night);

    //Alterna tra Day e Night
    public void Toggle()   => SetTheme(IsDay ? _night : _day);
}
