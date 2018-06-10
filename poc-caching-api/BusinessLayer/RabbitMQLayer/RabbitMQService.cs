namespace pocCachingApi.BusinessLayer.RabbitMQLayer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using pocCachingApi.BusinessLayer.ElasticLayer.Models;
    using pocCachingApi.BusinessLayer.RabbitMQLayer.Models;
    using pocCachingApi.BusinessLayer.SQLiteLayer.Models;
    using RabbitMQ.Client;

    public static class RabbitMQService
    {
        public static RabbitMQLayerResponse SendToQueue(VisuCalcul visuCalcul, List<POCData> dataList)
        {
            Console.WriteLine("[pocCachingApi.BusinessLayer.RabbitMQLayer] SendToQueue - Enter method");
            var response = new RabbitMQLayerResponse();

            var objectToSend = new
            {
                Id = visuCalcul.Id,
                Visu_Id = visuCalcul.Visu_Id,
                Calc_Sum = visuCalcul.Calc_Sum,
                Calc_Average = visuCalcul.Calc_Average,
                Values = dataList
            };

            var objToSendJSonString = JsonConvert.SerializeObject(objectToSend);
            var body = Encoding.UTF8.GetBytes(objToSendJSonString);

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(exchange: "", routingKey: "task_queue", basicProperties: properties, body: body);
                Console.WriteLine("[pocCachingApi.BusinessLayer.RabbitMQLayer] SendToQueue - Data published");

            }

            response.IsValid = true;
            return response;
        }
    }
}