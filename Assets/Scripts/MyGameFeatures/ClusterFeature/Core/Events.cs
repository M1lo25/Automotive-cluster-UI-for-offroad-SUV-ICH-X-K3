using UnityEngine;

namespace ICXK3
{
    //Variazione velocità veicolo(km/h)
    public struct OnSpeedChanged { public float kmh;  public OnSpeedChanged(float s){ kmh = s; } }

    //Variazione regime motore(rpm)
    public struct OnRpmChanged   { public float rpm;  public OnRpmChanged(float r){ rpm = r; } }

    //Variazione rollio telaio(gradi),positivo = rollio a destra
    public struct OnRollChanged  { public float deg;  public OnRollChanged(float d){ deg = d; } }

    //Variazione accelerazioni normalizzate(g),g.x=+avanti,indietro,g.y=+destra,sinistra
    public struct OnGChanged     { public Vector2 g;  public OnGChanged(Vector2 v){ g = v; } }

    // Alias storici e legacy con nome generico Changed
    public struct SpeedChanged   { public float value; public SpeedChanged(float v){ value = v; } }
    public struct RpmChanged     { public float value; public RpmChanged(float v){ value = v; } }

    //Stato della marcia,parcheggio,retromarcia,folle,dynamic
    public enum Gear { P, R, N, D }

    //variazione marcia
    public struct OnGearChanged { public Gear gear; public OnGearChanged(Gear g){ gear = g; } }
}
