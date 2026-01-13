//Interfaccia per widget UI che supportano più varianti grafiche selezionabili a runtime
namespace ICXK3
{
    public interface IVariantWidget
    {
        //Richiede al widget di passare alla variante indicata 
        void SwapVariant(GaugeVariant v); //cross-fade,morph interno
    }
}

