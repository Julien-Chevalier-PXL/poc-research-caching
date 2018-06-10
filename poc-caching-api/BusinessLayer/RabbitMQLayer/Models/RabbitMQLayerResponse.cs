namespace pocCachingApi.BusinessLayer.RabbitMQLayer.Models
{
    using System.Collections.Generic;

    public class RabbitMQLayerResponse
    {
        public bool IsValid { get; set; } = false;
        public string Message { get; set; }
        public List<int> Results { get; set; }
    }
}