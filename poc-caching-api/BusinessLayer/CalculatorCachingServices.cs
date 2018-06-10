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
                    response.isValid = true;
                    var visu_id = sqlResult.Result.Id;

                    var manager = new RedisManagerPool("localhost:6379");
                    using (var client = manager.GetClient())
                    {
                        var cachedData = client.Get<CalculationResponse>(visu_id.ToString());
                        if (cachedData != null)
                        {
                            response.Result = cachedData;
                        }
                        else
                        {
                            var elasticService = new ElasticService();
                            var elasticResponse = await elasticService.GetData();
                            if (elasticResponse.IsValid)
                            {
                                Console.WriteLine($"[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - {elasticResponse.Result.Count} docs retrieved from elasticearch");

                                // Initialize listening to Redis server
                                List<Task> tasks = new List<Task>();
                                tasks.Add(new Task(() =>
                                {
                                    for (int i = 0; i < 1; i++)
                                    {
                                    }
                                }));

                                var psRedisServer = new RedisPubSubServer(manager, visu_id.ToString())
                                {
                                    OnMessage = (channel, msg) =>
                                    {
                                        tasks.Add(Task.Run(() =>
                                        {
                                            Console.WriteLine($"Received '{msg}' from channel '{channel}'");
                                            var longList = JsonConvert.DeserializeObject<CalculationResponse>(msg);
                                            response.Result = longList;
                                            tasks.ElementAt(0).Start();
                                        }));                                        
                                    }
                                }.Start();

                                // SEND TO RABBITMQ
                                RabbitMQService.SendToQueue(sqlResult.Result, elasticResponse.Result);

                                Console.WriteLine("[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - Waiting for response of reddis.");
                                await Task.WhenAll(tasks);
                                Console.WriteLine("[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - All tasks done.");
                                response.isValid = true;
                            }
                            else
                            {
                                response.Message = elasticResponse.Message;
                                Console.WriteLine($"[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - {elasticResponse.Message}");
                            }
                        }
                    }
                }
                else
                {
                    response.isValid = true;
                    response.Message = sqlResult.Message;
                    Console.WriteLine($"[pocCachingApi.BusinessLayer.CalculatorCachingService] GetResults - {sqlResult.Message}");
                }
            }

            return response;
        }
    }
}