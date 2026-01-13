//Gestisce il cambio variante di un gauge usando due CanvasGroup e una transizione in cross-fade

using System.Collections;
using UnityEngine;

namespace ICXK3
{
    //Gestisce due GameObject con morph/crossfade
    public class VariantGauge : MonoBehaviour, IVariantWidget
    {
        [SerializeField] private CanvasGroup dialGroup;
        [SerializeField] private CanvasGroup barGroup;
        [SerializeField, Range(0.1f,1f)] private float fadeDur = 0.35f;

        private Coroutine _swap;

        //Richiesta esterna di cambio variante che stoppa la transizione precedente e ne avvia una nuova
        public void SwapVariant(GaugeVariant v)
        {
            if (_swap != null) StopCoroutine(_swap);
            _swap = StartCoroutine(SwapCR(v));
        }

        //Coroutine che esegue il cross-fade tra i due CanvasGroup 
        private IEnumerator SwapCR(GaugeVariant v)
        {
            var on = v == GaugeVariant.Dial ? dialGroup : barGroup;
            var off = v == GaugeVariant.Dial ? barGroup : dialGroup;

            if (on == null || off == null) yield break;

            on.gameObject.SetActive(true);
            off.gameObject.SetActive(true);

            //Prima fade-in del nuovo
            on.alpha = 0f;
            yield return UIAnimator.Fade(on, 1f, fadeDur * 0.6f);

            //Poi fade-out del vecchio
            yield return UIAnimator.Fade(off, 0f, fadeDur * 0.4f);
        }
    }
}
