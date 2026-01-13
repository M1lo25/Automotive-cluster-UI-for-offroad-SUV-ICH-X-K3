using System.Collections;
using UnityEngine;

namespace ICXK3
{
    //Helper di animazione UI basato su coroutine con easing SmoothStep pensato per chiamate come StartCoroutine
    public static class UIAnimator
    {
        //Anima l'alpha di un CanvasGroup da valore corrente a 'to' in 'dur' secondi
        public static IEnumerator Fade(CanvasGroup cg, float to, float dur)
        {
            cg.gameObject.SetActive(true);
            var from = cg.alpha; float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, dur);  //avanza tempo normalizzato 
                cg.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0,1,t));  //interpola con easing(ease-in/out)
                yield return null;   //attende il frame successivo
            }
            cg.alpha = to;  // assicura il valore finale
            if (to <= 0.001f) cg.gameObject.SetActive(false);   //nasconde se completamente trasparente
        }

        //Sposta la anchoredPosition di un RectTransform da 'from' a 'to' in 'dur' secondi
        //usa SmoothStep per la progressione e LerpUnclamped per mantenere una curva pulita fino a fine del ciclo
        public static IEnumerator Move(RectTransform rt, Vector2 from, Vector2 to, float dur)
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, dur);
                var k = Mathf.SmoothStep(0,1,t);       //fattore di interpolazione smussato
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, k);   //applica la posizione interpolata
                yield return null;
            }
            rt.anchoredPosition = to;          //snap preciso al target
        }

        // Scala localScale da 'from' a 'to'
        public static IEnumerator Scale(RectTransform rt, Vector3 from, Vector3 to, float dur)
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, dur);
                var k = Mathf.SmoothStep(0,1,t);
                rt.localScale = Vector3.LerpUnclamped(from, to, k);        
                yield return null;
            }
            rt.localScale = to;   //garantisce il valore finale
        }
    }
}
