namespace pocCalculator.Models
{
    using System.Collections.Generic;

    public class CalculationObject
    {
        public int Id { get; set; }
        public string Visu_Id { get; set; }
        public bool Calc_Sum { get; set; }
        public bool Calc_Average { get; set; }
        public List<POCData> Values { get; set; }
    }
}