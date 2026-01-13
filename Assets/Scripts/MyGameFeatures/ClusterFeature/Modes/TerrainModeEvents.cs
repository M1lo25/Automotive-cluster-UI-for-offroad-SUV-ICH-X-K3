//Definisce i "payload" degli eventi relativi al cambio modalità terreno

namespace ICXK3
{
    //Evento emesso quando cambia la modalità terreno.
    //readonly struct=immutabile (value type) e trasporta il TerrainModeSO della modalità attiva
    public readonly struct TerrainModeChanged
    {
        public readonly TerrainModeSO mode;
        public TerrainModeChanged(TerrainModeSO mode) { this.mode = mode; }
    }

}
