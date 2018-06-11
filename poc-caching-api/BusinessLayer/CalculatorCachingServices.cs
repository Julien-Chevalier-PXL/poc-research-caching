namespace pocCachingApi.BusinessLayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using pocCachingApi.BusinessLayer.ElasticLayer;
    using pocCachingApi.BusinessLayer.Models;
    using pocCachingApi.BusinessLayer.RabbitMQLayer;
    using pocCachingApi.BusinessLayer.RedisLayer;
    using pocCachingApi.BusinessLayer.RedisLayer.Models;
    using pocCachingApi.BusinessLayer.SQLiteLayer;
    using ServiceStack;
    using ServiceStack.Redis;

    public class CalculatorCachingService
    {
        public async Task<CalculatorCachingResponse> GetResults(string visuId)
        {
            Console.WriteLine("[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - Enter");

            var response = new CalculatorCachingResponse();
            SqliteService service = new SqliteService();
            var sqlResult = await service.GetVisuCalculByVisuId(visuId);

            if (sqlResult.isValid)
            {
                if (sqlResult.Result != null)
                {
                    var redisService = new RedisService("localhost:6379", sqlResult.Result.Id);
                    var cachedData = redisService.GetCachedData();
                    if (cachedData != null)
                    {
                        response.IsValid = true;
                        response.Result = cachedData;
                    }
                    else
                    {
                        var elasticService = new ElasticService();
                        var elasticResponse = await elasticService.GetData();
                        if (elasticResponse.IsValid)
                        {
                            Console.WriteLine($"[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - {elasticResponse.Result.Count} docs retrieved from elasticearch");

                            var redisOnMessage = redisService.InitializeRedisPubSub();

                            // SEND TO RABBITMQ
                            RabbitMQService.SendToQueue(sqlResult.Result, elasticResponse.Result);

                            Console.WriteLine("[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - Waiting for response of reddis.");
                            var redisResult = await redisOnMessage;
                            response.Result = redisResult;
                            response.IsValid = true;
                            return response;
                        }
                        else
                        {
                            response.Message = elasticResponse.Message;
                            Console.WriteLine($"[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - {elasticResponse.Message}");
                        }
                    }

                }
                else
                {
                    response.IsValid = true;
                    response.Message = sqlResult.Message;
                    Console.WriteLine($"[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - {sqlResult.Message}");
                }
            }

            return response;
        }
    }
}