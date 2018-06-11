namespace pocCachingApi.BusinessLayer.Models
{
    using System.Collections.Generic;
    using pocCachingApi.BusinessLayer.RedisLayer.Models;
    using pocCachingApi.BusinessLayer.SQLiteLayer.Models;
    
    public class CalculatorCachingResponse
    {
        public CalculationResponse Result { get; set; }
        public bool IsValid { get; set; } = false;
        public string Message { get; set; }
    }
}