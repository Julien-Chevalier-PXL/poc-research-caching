using System.Collections.Generic;

namespace pocCachingApi.BusinessLayer.ElasticLayer.Models
{
    public class ElasticLayerResponse<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsValid { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T Result { get; set; }
    }
}