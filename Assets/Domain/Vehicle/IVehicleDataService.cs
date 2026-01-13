using UnityEngine;
using Gear = ICXK3.Gear;

namespace ICXK3.Domain
{
    //Servizio dati veicolo per il cluster
    public interface IVehicleDataService
    {
        float   SpeedKph  { get; }  //Velocità(km/h)
        float   Rpm       { get; }  //Regime motore(Rpm)
        Gear    DriveGear { get; }  //Marcia attuale(ICXK3.Gear)
        float   RollDeg   { get; }  //Rollio telaio
        Vector2 G         { get; }  //Accelerazioni in g(x=longitudine,y=latitudine)

        void    SetGear(Gear g);    //Richiesta cambio marcia(validazioni lato implementazione)
        void    SimTick(float dt);  //Tick di aggiornamento(dt in secondi)
    }
}
