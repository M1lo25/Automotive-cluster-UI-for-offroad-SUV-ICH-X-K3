//ScriptableObject che contiene una palette di colori per il tema UI
//Viene creato come asset dal menu e usato dai component per applicare i colori

using UnityEngine;

[CreateAssetMenu(menuName = "Theme/Theme SO")]
public class ThemeSO : ScriptableObject
{
    [Header("Base")]
    public Color BackgroundContent; 
    public Color Background;      

    [Header("Typography")]
    public Color PrimaryText;
    public Color SecondaryText;

    [Header("UI Accents")]
    public Color Accent;

    [Header("Gauges")]
    public Color GaugeFill;
    public Color GaugeBackground;

    [Header("Status")]
    public Color Warning;
}
