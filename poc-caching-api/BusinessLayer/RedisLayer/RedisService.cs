namespace pocCachingApi.BusinessLayer.RedisLayer
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using pocCachingApi.BusinessLayer.RedisLayer.Models;
    using ServiceStack.Redis;

    public class RedisService
    {
        private RedisManagerPool _manager;
        private int _visu_id;


        public RedisService(string host, int visu_id)
        {
            _manager = new RedisManagerPool(host);
            _visu_id = visu_id;
        }

        public CalculationResponse GetCachedData()
        {
            using (var client = _manager.GetClient())
            {
                return client.Get<CalculationResponse>(_visu_id.ToString());
            }

        }

        public Task<CalculationResponse> InitializeRedisPubSub()
        {
            var redisChannel = "";
            var redisMsg = "";
            var task = new Task<CalculationResponse>(() =>
            {
                Console.WriteLine($"Received '{redisMsg}' from channel '{redisChannel}'");
                return JsonConvert.DeserializeObject<CalculationResponse>(redisMsg);
            });

            // Initialize listening to Redis server
            var psRedisServer = new RedisPubSubServer(_manager, _visu_id.ToString())
            {
                OnMessage = (channel, msg) =>
                {
                    redisChannel = channel;
                    redisMsg = msg;
                    task.Start();
                }
            }.Start();

            return task;
        }
    }
}