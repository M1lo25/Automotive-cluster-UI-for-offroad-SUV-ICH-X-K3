// Simula la pressione gomme: permette foratura/riparazione via hotkey, interpola le pressioni verso un target e broadcasta lo snapshot quando cambia.
using UnityEngine;
using ICXK3.Domain;

namespace ICXK3
{
    [DisallowMultipleComponent]
    public class TyrePressureController : MonoBehaviour
    {
        public const float NormalPressureBar    = 2.6f;
        public const float PuncturedPressureBar = 1.0f;

        static readonly KeyCode PunctureKey = KeyCode.B;
        static readonly KeyCode RepairKey   = KeyCode.V;

        [SerializeField] float deflateRateBarPerSec = 0.40f;  
        [SerializeField] float inflateRateBarPerSec = 0.80f;  
        [SerializeField] float broadcastEpsilon     = 0.001f;  

        public float FrontLeft  => _pressures[0];
        public float FrontRight => _pressures[1];
        public float RearLeft   => _pressures[2];
        public float RearRight  => _pressures[3];

        float[] _pressures       = new float[4];
        float[] _targets         = new float[4];
        int     _puncturedIndex  = -1;

        // Inizializza tutte le gomme a pressione normale e invia uno snapshot iniziale sul bus.
        void Start()
        {
            for (int i = 0; i < 4; i++)
            {
                _pressures[i] = NormalPressureBar;
                _targets[i]   = NormalPressureBar;
            }
            BroadcastSnapshot();
        }

        // Gestisce input foratura/riparazione, aggiorna le pressioni con MoveTowards e broadcasta solo se cambia davvero.
        void Update()
        {
            // Foratura: sceglie una gomma random una sola volta e imposta target a pressione bucata.
            if (Input.GetKeyDown(PunctureKey))
            {
                if (_puncturedIndex == -1)
                {
                    _puncturedIndex = Random.Range(0, 4);
                    _targets[_puncturedIndex] = PuncturedPressureBar;
                }
            }

            // Riparazione: riporta il target a pressione normale per la gomma bucata.
            if (Input.GetKeyDown(RepairKey))
            {
                if (_puncturedIndex != -1)
                {
                    _targets[_puncturedIndex] = NormalPressureBar;
                }
            }

            bool changed = false;

            // Aggiorna ogni ruota verso il proprio target con rate diverso (sgonfia/gonfia).
            for (int i = 0; i < 4; i++)
            {
                float rate = (_targets[i] < _pressures[i]) ? deflateRateBarPerSec : inflateRateBarPerSec;
                float next = Mathf.MoveTowards(_pressures[i], _targets[i], rate * Time.deltaTime);

                if (Mathf.Abs(next - _pressures[i]) > broadcastEpsilon)
                {
                    _pressures[i] = next;
                    changed = true;
                }
            }

            // Quando la gomma riparata torna alla pressione normale, resetta lo stato “punctured”.
            if (_puncturedIndex != -1 && Mathf.Approximately(_pressures[_puncturedIndex], NormalPressureBar))
                _puncturedIndex = -1;

            // Broadcast solo quando c’è una variazione significativa.
            if (changed) BroadcastSnapshot();
        }

        // Getter robusto per pressione ruota (fallback a normale se indice invalido).
        public float GetPressureBar(int wheelIndex)
        {
            if (wheelIndex < 0 || wheelIndex > 3) return NormalPressureBar;
            return _pressures[wheelIndex];
        }

        // Invia sul bus le 4 pressioni correnti (snapshot).
        void BroadcastSnapshot()
        {
            if (ServiceRegistry.TryResolve<IBroadcaster>(out var bus))
            {
                bus.Broadcast(new OnTyrePressuresChanged(_pressures[0], _pressures[1], _pressures[2], _pressures[3]));
            }
        }
    }

    // Evento dati: snapshot delle pressioni (FL/FR/RL/RR) in bar.
    public readonly struct OnTyrePressuresChanged
    {
        public readonly float FrontLeftBar;
        public readonly float FrontRightBar;
        public readonly float RearLeftBar;
        public readonly float RearRightBar;

        public OnTyrePressuresChanged(float fl, float fr, float rl, float rr)
        {
            FrontLeftBar  = fl;
            FrontRightBar = fr;
            RearLeftBar   = rl;
            RearRightBar  = rr;
        }
    }
}
