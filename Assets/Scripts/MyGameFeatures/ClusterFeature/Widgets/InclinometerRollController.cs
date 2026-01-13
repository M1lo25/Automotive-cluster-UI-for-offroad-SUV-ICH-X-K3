// Gestisce l’inclinometro di rollio in UI: tramite input (H/J) varia l’angolo solo sopra una soglia di velocità, applica ritorno a zero e smoothing, ruotando l’icona e aggiornando il testo.
using UnityEngine;
using TMPro;
using ICXK3.Domain;

namespace ICXK3
{
    public class InclinometerRollController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RectTransform suvIcon;
        [SerializeField] private TMP_Text valueText;

        const float SpeedThresholdKph = 1f;
        const float MaxAbsDeg = 30f;
        const float RateDegPerSec = 30f;
        const float ReturnRateDegPerSec = 45f;
        const float FollowSpeed = 10f;

        IVehicleDataService _veh;
        float _targetDeg;
        float _shownDeg;

        // Auto-binda i riferimenti UI e risolve il servizio veicolo (per leggere la velocità).
        void Awake()
        {
            if (!suvIcon)
                suvIcon = transform.Find("SUVIcon")?.GetComponent<RectTransform>();
            if (!valueText)
                valueText = transform.Find("InclinoValue")?.GetComponent<TMP_Text>();

            Locator.TryResolve(out _veh);
        }

        // Aggiorna l’angolo target via input, lo clampa e lo filtra, poi aggiorna rotazione icona e testo.
        void Update()
        {
            float dt = Time.deltaTime;

            if (_veh == null && Time.frameCount % 60 == 0)
                Locator.TryResolve(out _veh);

            float speed = (_veh != null) ? _veh.SpeedKph : 0f;
            bool allowed = speed > SpeedThresholdKph;

            // Input: H = roll a sinistra, J = roll a destra (solo se in movimento).
            bool pressLeft  = allowed && Input.GetKey(KeyCode.H);
            bool pressRight = allowed && Input.GetKey(KeyCode.J);

            if (pressLeft)  _targetDeg -= RateDegPerSec * dt;
            if (pressRight) _targetDeg += RateDegPerSec * dt;

            // Se nessun input, ritorna gradualmente verso 0°.
            if (!pressLeft && !pressRight)
                _targetDeg = Mathf.MoveTowards(_targetDeg, 0f, ReturnRateDegPerSec * dt);

            // Clamp e smoothing del valore mostrato.
            _targetDeg = Mathf.Clamp(_targetDeg, -MaxAbsDeg, +MaxAbsDeg);
            _shownDeg  = Mathf.Lerp(_shownDeg, _targetDeg, dt * FollowSpeed);

            // Applica rotazione grafica e aggiorna il valore numerico.
            if (suvIcon)
                suvIcon.localEulerAngles = new Vector3(0f, 0f, -_shownDeg);
            if (valueText)
                valueText.text = $"{Mathf.RoundToInt(_shownDeg)}°";
        }
    }
}
