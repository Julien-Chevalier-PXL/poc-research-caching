namespace pocCalculator.Models
{
    public class CalculationResponse
    {
        public long Sum { get; set; }
        public double Average { get; set; }

        public override string ToString(){
            return $"Calculationresponse {{Sum:{Sum}, Average:{Average}}}";
        }
    }
}