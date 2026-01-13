using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TMPro;

public class FeatureHost : MonoBehaviour
{
    Transform _uiRoot; //Radice della UI
    Canvas _canvas; //Canvas attivo su cui inserire i pannelli

    //Parametri di layout e margini per posizionare i pannelli nell'UI
    [Header("Margins / Layout")]
    [SerializeField] float edgeMargin = 40f;
    [SerializeField] float mainVOffset = 40f;
    [SerializeField] float mainTowardsCenterX = 160f;
    [SerializeField] float inclinoTowardsCenterX = 180f;
    [SerializeField] float inclinoBottomMarginY = 110f;
    [SerializeField] float arrowTopMarginY = 38f;
    [SerializeField] float modebarMidOffsetY = -340f;
    [SerializeField] float gmeterSize = 300f;

    //Config caricamento TPMS via Addressables e piccoli offset
    [Header("Tyre Pressure panel")]
    [SerializeField] string  tyrePressureAddress = "TyrePressurePanel";
    [SerializeField] Vector2 tyrePanelSize = new Vector2(0,0);
    [SerializeField] float   tyreOffsetRightFromGear = 74f;
    [SerializeField] float   tyreNudgeRight = 100f;
    [SerializeField] float   tyreNudgeDown  = -20f;

    //Posizioni standard per ancorare i pannelli
    public enum Dock { TopLeft, TopRight, BottomLeft, BottomRight, TopCenter, BottomCenter, CenterLeft, CenterRight, Center }

    readonly List<GameObject> _spawned = new(); //Traccia istanze Addressables per poi rilasciarle

    //Avvio dove si istanziano tutti i pannelli UI via Addressables e li posiziona sul Canvas UIRoot
    public async Task BootAsync(Transform uiRoot)
    {
        _uiRoot = uiRoot;
        _canvas = _uiRoot.GetComponentInChildren<Canvas>(true);
        if (!_canvas) return;

        //Pannelli SpeedPanel,RpmPanel
        await AddPanelDockedAsync("SpeedPanel", Dock.CenterLeft, null, new Vector2(edgeMargin + mainTowardsCenterX, mainVOffset));
        await AddPanelDockedAsync("RpmPanel",   Dock.CenterRight, null, new Vector2(edgeMargin + mainTowardsCenterX,  mainVOffset));

        //Pannello GMeterPanel
        var gmeter = await AddPanelDockedAsync("GMeterPanel", Dock.Center, new Vector2(gmeterSize, gmeterSize), new Vector2(0f, mainVOffset));
        if (gmeter) FixGMeterLabels(gmeter); //Aggiusta etichette TMP

        //Pannelli RollPanel,PitchPanel
        await AddPanelDockedAsync("RollPanel", Dock.BottomLeft, null, new Vector2(edgeMargin + inclinoTowardsCenterX, inclinoBottomMarginY));
        await AddPanelDockedAsync("PitchPanel", Dock.BottomRight, null, new Vector2(edgeMargin + inclinoTowardsCenterX, inclinoBottomMarginY));

        //Pannello ArrowGear
        var gear = await AddPanelDockedAsync("ArrowGear", Dock.TopCenter, null, new Vector2(0f, arrowTopMarginY));
        if (gear) gear.SetAsLastSibling();   //in cima allo z-order

        //TPMS vicino a Gear con calcolo offset/clamp sui bordi
        await SpawnTyrePressurePanelNearGearAsync(gear);

        //Pannello PanelMode al centro verticale
        var mode = await AddPanelDockedAsync("PanelMode", Dock.Center, null, new Vector2(0f, modebarMidOffsetY));
        if (mode) mode.SetSiblingIndex(Mathf.Max(0, _canvas.transform.childCount - 2)); //dietro all’ultimo
    }

    //Istanzia un pannello Addressables e docka sul Canvas secondo la posizioe richiesta
    public async Task<RectTransform> AddPanelDockedAsync(string address, Dock dock, Vector2? size, Vector2 margin)
    {
        //Creazione dell'istanza del prefab
        var go = await Addressables.InstantiateAsync(address).Task;
        if (!go) return null;

        _spawned.Add(go);  //Tiene traccia per il teardown

        //Viene assicurata la presenza del RectTrasform e parentata al canvas
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        if (_canvas) go.transform.SetParent(_canvas.transform, false);
        rt.localScale = Vector3.one;
        rt.anchoredPosition3D = Vector3.zero;

        //Controllo per verificare se una size è>0,viene applicata
        if (size.HasValue && size.Value.x > 0f && size.Value.y > 0f)
            rt.sizeDelta = size.Value;

        //Imposta anchor/pivot/pos in base al dock scelto e i margini
        switch (dock)
        {
            case Dock.TopLeft: SetAnchors(rt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(+margin.x, -margin.y)); break;
            case Dock.TopRight: SetAnchors(rt, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-margin.x, -margin.y)); break;
            case Dock.BottomLeft: SetAnchors(rt, new Vector2(0, 0), new Vector2(0, 0), new Vector2(+margin.x, +margin.y)); break;
            case Dock.BottomRight: SetAnchors(rt, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-margin.x, +margin.y)); break;
            case Dock.TopCenter: SetAnchors(rt, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -margin.y)); break;
            case Dock.BottomCenter: SetAnchors(rt, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, +margin.y)); break;
            case Dock.CenterLeft: SetAnchors(rt, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(+margin.x, margin.y)); break;
            case Dock.CenterRight: SetAnchors(rt, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(-margin.x, margin.y)); break;
            case Dock.Center: SetAnchors(rt, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(margin.x, margin.y)); break;
        }


        //Se esiste un RectMask ma la size è nulla compare l'avviso
        var mask = go.GetComponent<RectMask2D>();
        if (mask && (rt.sizeDelta.x <= 1f || rt.sizeDelta.y <= 1f))
            Debug.LogWarning("[FeatureHost] RectMask2D:verifica size e anchors");

        return rt;
    }

    //Impostazioni rapide di anchor/pivot/pos/trasformazioni
    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 anchored)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot     = max;
        rt.anchoredPosition = anchored;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
    }

    //Ritocca allineamento e posizioni delle label del G-Meter
    void FixGMeterLabels(RectTransform gmeter)
    {
        TMP_Text xTxt = null, yTxt = null, title = null;
        //Cerca TMP figli identifica quelli pertinenti in base al testo
        foreach (var t in gmeter.GetComponentsInChildren<TMP_Text>(true))
        {
            var s = t.text?.ToUpperInvariant() ?? "";
            if (s.Contains("G-METER") || s.Contains("GMETER")) title = t;
            else if (s.Contains("X") && s.Contains("G")) xTxt = t;
            else if (s.Contains("Y") && s.Contains("G")) yTxt = t;
        }
        //Titolo centrato in basso rispetto al g-meter
        if (title)
        {
            var rt = title.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.0f);
            rt.pivot = new Vector2(0.5f, 0.0f);
            rt.anchoredPosition = new Vector2(0f, -26f);
        }
        //Etichetta X centrata al 25% larghezza
        if (xTxt)
        {
            var rt = xTxt.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.25f, 0.0f);
            rt.pivot = new Vector2(0.5f, 0.0f);
            rt.anchoredPosition = new Vector2(0f, -44f);
            xTxt.alignment = TextAlignmentOptions.Center;
        }
        //Etichetta Y centrata al 75% larghezza
        if (yTxt)
        {
            var rt = yTxt.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.75f, 0.0f);
            rt.pivot     = new Vector2(0.5f, 0.0f);
            rt.anchoredPosition = new Vector2(0f, -44f);
            yTxt.alignment = TextAlignmentOptions.Center;
        }
    }

    //Istanzia TPMS vicino a WidgetGear,calcola gli offset e clampa ai bordi del Canvas
    async Task SpawnTyrePressurePanelNearGearAsync(RectTransform gear)
    {
        //Mostra TPMS ancorato in alto centro
        var tyre = await AddPanelDockedAsync(tyrePressureAddress, Dock.TopCenter, tyrePanelSize, new Vector2(0f, arrowTopMarginY));
        if (tyre == null) return;

        var parent = tyre.parent as RectTransform;
        if (gear != null && parent != null)
        {
            //Aggiorna il layout per ottenere misure coerenti
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);

            float gearWidth = gear.rect.width  > 0 ? gear.rect.width  : gear.sizeDelta.x;
            float tyreWidth = tyre.rect.width  > 0 ? tyre.rect.width  : tyre.sizeDelta.x;
            float tyreHeight= tyre.rect.height > 0 ? tyre.rect.height : tyre.sizeDelta.y;

            //Ancore e pivot in alto centro per il pannello
            tyre.anchorMin = tyre.anchorMax = new Vector2(0.5f, 1f);
            tyre.pivot     = new Vector2(0.5f, 1f);

             //Calcola l’offset orizzontale dalla Gear
            float dx = (gearWidth * 0.5f) + tyreOffsetRightFromGear + (tyreWidth * 0.5f);

            //Posizione target con nudge personalizzati
            Vector2 target = new Vector2(
                gear.anchoredPosition.x + dx + tyreNudgeRight,
                gear.anchoredPosition.y - tyreNudgeDown
            );

            //Clamping nei bordi del Canvas
            var canvasRT = _canvas.transform as RectTransform;
            if (canvasRT)
            {
                float halfW = canvasRT.rect.width  * 0.5f;
                float halfH = canvasRT.rect.height * 0.5f;

                float maxX = +halfW - edgeMargin - (tyreWidth  * 0.5f);
                float minX = -halfW + edgeMargin + (tyreWidth  * 0.5f);
                float maxY = +halfH - edgeMargin;
                float minY = -halfH + edgeMargin + (tyreHeight * 0.5f);

                target.x = Mathf.Clamp(target.x, minX, maxX);
                target.y = Mathf.Clamp(target.y, minY, maxY);
            }

            tyre.anchoredPosition = target;
        }
        else
        {
            //Per il fallback dock in alto a destra con margini se manca il riferimento gear o canvas
            SetAnchors(tyre, new Vector2(1, 1), new Vector2(1, 1),
                       new Vector2(-edgeMargin, -arrowTopMarginY - tyreNudgeDown));
        }

        tyre.SetAsLastSibling();
    }

    //Tutte le istanze Addressables create da questo host vengono rilasciate in maniera pulita
    public void Teardown()
    {
        for (int i = _spawned.Count - 1; i >= 0; --i)
        {
            var go = _spawned[i];
            if (go) Addressables.ReleaseInstance(go);
        }
        _spawned.Clear();
    }
}
