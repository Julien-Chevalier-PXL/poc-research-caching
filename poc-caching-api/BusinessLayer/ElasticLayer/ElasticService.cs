namespace pocCachingApi.BusinessLayer.ElasticLayer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Nest;
    using pocCachingApi.BusinessLayer.ElasticLayer.Models;

    public class ElasticService
    {
        public List<object> Hits;
        public ElasticClient EsClient { get; set; }

        public ElasticService()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"));
            EsClient = new ElasticClient(settings);
        }

        public async Task<ElasticLayerResponse<List<POCData>>> GetData(string queryString = "*")
        {
            Console.WriteLine("[pocCachingApi.BusinessLayer.ElasticLayer.ElasticService] GetData");
            var response = new ElasticLayerResponse<List<POCData>>();
            var query = new SearchDescriptor<POCData>().Index("logstash-poc-*").Scroll("30s").Size(10000);
            if (queryString != "*")
            {
                query.Query(q => q.Match(qm => qm.Field(f => f.MessageData).Query(queryString)));
            }
            else
            {
                query.Query(q => q.MatchAll());
            }


            var result = await EsClient.SearchAsync<POCData>(query);
            if (result.IsValid)
            {
                response.Result = new List<POCData>(result.Documents);
                while (response.Result.Count < result.Total)
                {
                    result = await EsClient.ScrollAsync<POCData>("30s", result.ScrollId);
                    if (result.IsValid)
                    {
                        response.Result.AddRange(result.Documents);
                    }
                    else
                    {
                        response.Message = "Error during scroll in elasticsearch";
                        return response;
                    }
                }
                response.IsValid = true;
            }
            else
            {
                response.Message = "Error during search in elasticsearch";
            }
            
            Console.WriteLine("[pocCachingApi.BusinessLayer.ElasticLayer.ElasticService] GetData - End");
            return response;
        }
    }
}