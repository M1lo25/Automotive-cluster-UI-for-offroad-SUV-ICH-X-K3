// Controlla una “striscia” UI ambient: riempie la barra in base alla velocità e aggiunge un leggero effetto pulse sincronizzato con gli RPM.
using UnityEngine;
using UnityEngine.UI;

namespace ICXK3
{
    public class AmbientStripController : MonoBehaviour
    {
        [SerializeField] private Image bar;
        [SerializeField] private float vmax = 200f;

        private float uiFill, targetFill, rpm;
        private bool pulseEnabled = true;

        // Si registra al bus per ricevere aggiornamenti di velocità e RPM.
        private void OnEnable()
        {
            var bus = Locator.Resolve<IBroadcaster>();
            bus.Add<OnSpeedChanged>(OnSpeed);
            bus.Add<OnRpmChanged>(OnRpm);
        }

        // Deregistra i listener dal bus per evitare callback quando l’oggetto è disabilitato.
        private void OnDisable()
        {
            var bus = Locator.Resolve<IBroadcaster>();
            bus.Remove<OnSpeedChanged>(OnSpeed);
            bus.Remove<OnRpmChanged>(OnRpm);
        }

        // Aggiorna il fill con smoothing e applica un “pulse” opzionale basato sugli RPM (toggle con P).
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P)) pulseEnabled = !pulseEnabled;

            uiFill = Mathf.Lerp(uiFill, targetFill, Time.deltaTime * 4f);
            float pulse = pulseEnabled ? 0.05f * Mathf.Sin(Time.time * (rpm/60f) * 2f * Mathf.PI) : 0f;
            if (bar) bar.fillAmount = Mathf.Clamp01(uiFill + pulse);
        }

        // Converte la velocità (km/h) in un valore 0..1 per il riempimento della barra.
        private void OnSpeed(OnSpeedChanged e)
        {
            targetFill = Mathf.InverseLerp(0, vmax, e.kmh);
        }

        // Memorizza gli RPM correnti usati per la frequenza del pulse.
        private void OnRpm(OnRpmChanged e)
        {
            rpm = e.rpm;
        }
    }
}
