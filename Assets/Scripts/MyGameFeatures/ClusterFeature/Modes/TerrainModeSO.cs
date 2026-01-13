using UnityEngine;

namespace ICXK3
{
    //ScriptableObject per una modalità terreno (configurabile da Inspector)
    [CreateAssetMenu(menuName = "ICX K3/Terrain Mode", fileName = "Mode_")]
    public class TerrainModeSO : ScriptableObject
    {
        //Nome della modalità visualizzato in UI che viene lett dal codice
        public string modeName = "MODE";
        public Color accent = Color.white; //Colore d'accento per UI/tema
        public Sprite icon;  //Icona opzionale per HUD
    }
}
