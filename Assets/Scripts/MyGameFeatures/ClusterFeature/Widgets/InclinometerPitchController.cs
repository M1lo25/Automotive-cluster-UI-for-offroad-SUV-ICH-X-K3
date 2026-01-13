// Gestisce l’inclinometro (pitch) in UI: genera un angolo di pendenza tramite input (K/L) con limiti e ritorno a zero, ruota l’icona e aggiorna CurrentPitchDeg per gli altri sistemi.
using UnityEngine;
using TMPro;
using ICXK3.Domain;
using Gear = ICXK3.Gear;

namespace ICXK3
{
    public class InclinometerPitchController : MonoBehaviour
    {
        public static float CurrentPitchDeg { get; private set; }

        [Header("UI")]
        [SerializeField] private RectTransform suvIcon;
        [SerializeField] private TMP_Text valueText;

        const float MaxAbsDeg = 30f;
        const float RateDegPerSec = 30f;
        const float ReturnRateDegPerSec = 45f;
        const float FollowSpeed = 10f;
        const float SpeedEps = 0.1f;

        IVehicleDataService _veh;
        float _targetDeg;
        float _shownDeg;

        // Auto-binda i riferimenti UI e risolve il servizio veicolo (per leggere marcia e velocità).
        void Awake()
        {
            if (!suvIcon)
                suvIcon = transform.Find("SUVIcon")?.GetComponent<RectTransform>();
            if (!valueText)
                valueText = transform.Find("InclinoValue")?.GetComponent<TMP_Text>();

            Locator.TryResolve(out _veh);
        }

        // Aggiorna il target pitch via input, applica clamp/return-to-zero e aggiorna rotazione + testo.
        void Update()
        {
            float dt = Time.deltaTime;

            if (_veh == null && Time.frameCount % 60 == 0)
                Locator.TryResolve(out _veh);

            var gear = (_veh != null) ? _veh.DriveGear : Gear.P;
            float speed = (_veh != null) ? _veh.SpeedKph : 0f;

            // Abilitazioni: K solo in D e in movimento, L in D o N (simula discesa/roll).
            bool allowK = (gear == Gear.D) && (speed > SpeedEps);
            bool allowL = (gear == Gear.D) || (gear == Gear.N);

            bool pressK = allowK && Input.GetKey(KeyCode.K);
            bool pressL = allowL && Input.GetKey(KeyCode.L);

            // Input: modifica il target (nota: segni invertiti rispetto al display).
            if (pressK) _targetDeg -= RateDegPerSec * dt; // K = salita (positiva a display)
            if (pressL) _targetDeg += RateDegPerSec * dt; // L = discesa (negativa a display)

            // Se nessun input, ritorna gradualmente verso 0°.
            if (!pressK && !pressL)
                _targetDeg = Mathf.MoveTowards(_targetDeg, 0f, ReturnRateDegPerSec * dt);

            // Clamp: in N consente solo “discesa” (valori display negativi), in D consente +/-.
            if (gear == Gear.N)
                _targetDeg = Mathf.Clamp(_targetDeg, 0f, MaxAbsDeg);
            else
                _targetDeg = Mathf.Clamp(_targetDeg, -MaxAbsDeg, +MaxAbsDeg);

            // Smoothing visivo tra target e valore mostrato.
            _shownDeg  = Mathf.Lerp(_shownDeg, _targetDeg, dt * FollowSpeed);

            // Rotazione icona SUV (segno per orientamento grafico).
            if (suvIcon)
                suvIcon.localEulerAngles = new Vector3(0f, 0f, -_shownDeg);

            // Conversione al valore “da display” (positivo salita, negativo discesa).
            float signedDisplayDeg = -_shownDeg;

            // Aggiorna testo inclinazione.
            if (valueText)
                valueText.text = $"{Mathf.RoundToInt(signedDisplayDeg)}°";

            // Espone la pendenza corrente agli altri sistemi (es. VehicleDataService per slope influence).
            CurrentPitchDeg = signedDisplayDeg;
        }
    }
}
