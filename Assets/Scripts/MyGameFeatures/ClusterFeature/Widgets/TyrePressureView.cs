// View UI delle pressioni gomme: si iscrive all’evento OnTyrePressuresChanged e aggiorna i testi TMP (FL/FR/BL/BR), con un apply iniziale leggendo il controller.
using UnityEngine;
using TMPro;
using ICXK3.Domain; // dove vivono ServiceRegistry / IBroadcaster

namespace ICXK3
{
    [DisallowMultipleComponent]
    public class TyrePressureView : MonoBehaviour
    {
        [Header("Riferimenti UI (TMP)")]
        [SerializeField] TMP_Text textFL;
        [SerializeField] TMP_Text textFR;
        [SerializeField] TMP_Text textBL;
        [SerializeField] TMP_Text textBR;
        [SerializeField] TMP_Text unitLabel;    // opzionale, es. "bar"
        [SerializeField] TMP_Text titleLabel;   // opzionale, es. "Tyre Pressure"

        IBroadcaster _bus;

        // Si registra al bus e fa un primo “refresh” leggendo lo stato corrente dal TyrePressureController.
        void OnEnable()
{
    if (ServiceRegistry.TryResolve<IBroadcaster>(out _bus))
    {
        _bus.Add<OnTyrePressuresChanged>(OnChanged);     // <— prima era Subscribe
    }

    var controller = FindFirstObjectByType<TyrePressureController>();
    if (controller != null)
    {
        Apply(controller.FrontLeft, controller.FrontRight, controller.RearLeft, controller.RearRight);
    }
}

        // Deregistra il listener quando la view viene disabilitata.
void OnDisable()
{
    if (_bus != null)
    {
        _bus.Remove<OnTyrePressuresChanged>(OnChanged);  // <— prima era Unsubscribe
        _bus = null;
    }
}

        // Handler evento: riceve lo snapshot pressioni e lo applica alla UI.
        void OnChanged(OnTyrePressuresChanged e)
        {
            Apply(e.FrontLeftBar, e.FrontRightBar, e.RearLeftBar, e.RearRightBar);
        }

        // Scrive i valori sui quattro label con formato a 1 decimale.
        void Apply(float fl, float fr, float bl, float br)
        {
            if (textFL) textFL.text = fl.ToString("0.0");
            if (textFR) textFR.text = fr.ToString("0.0");
            if (textBL) textBL.text = bl.ToString("0.0");
            if (textBR) textBR.text = br.ToString("0.0");
        }
    }
}
