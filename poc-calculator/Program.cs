namespace pocCalculator
{
    using System;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using pocCalculator.Models;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using ServiceStack.Redis;

    class Program
    {
        private readonly static TimeSpan CACHING_TIME = new TimeSpan(0, 15, 0);

        public static void Main()
        {
            var response = new CalculationResponse()
            {
                Sum = -1,
                Average = -1.0
            };

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                            {
                                var watch = System.Diagnostics.Stopwatch.StartNew();

                                var body = ea.Body;
                                var message = Encoding.UTF8.GetString(body);
                                var receivedObj = JsonConvert.DeserializeObject<CalculationObject>(message);
                                Console.WriteLine($" [x] Received calculation request for the visualization {receivedObj.Visu_Id}");
                                if (receivedObj.Calc_Sum)
                                {
                                    var watchSum = System.Diagnostics.Stopwatch.StartNew();

                                    response.Sum = receivedObj.Values.Sum(v => (long)v.Value);

                                    watchSum.Stop();
                                    var elapsedMsSum = watchSum.ElapsedMilliseconds;
                                    Console.WriteLine($"consumer.Received Sum process time: {elapsedMsSum.ToString()} ms for {receivedObj.Values.Count} docs");
                                }
                                if (receivedObj.Calc_Average)
                                {
                                    var watchAverage = System.Diagnostics.Stopwatch.StartNew();

                                    response.Average = receivedObj.Values.Average(v => (long)v.Value);

                                    watchAverage.Stop();
                                    var elapsedMsAverage = watchAverage.ElapsedMilliseconds;
                                    Console.WriteLine($"consumer.Received Average process time: {elapsedMsAverage.ToString()} ms for {receivedObj.Values.Count} docs");

                                }

                                var manager = new RedisManagerPool("localhost:6379");
                                using (var client = manager.GetClient())
                                {
                                    client.Set(receivedObj.Visu_Id, response, CACHING_TIME);
                                    client.PublishMessage(receivedObj.Visu_Id, JsonConvert.SerializeObject(response));
                                    Console.WriteLine($" [x] [{DateTime.Now.ToString()}] Set {receivedObj.Visu_Id} to {response.ToString()}");
                                }

                                watch.Stop();
                                var elapsedMs = watch.ElapsedMilliseconds;
                                Console.WriteLine("----------------------------------------------");
                                Console.WriteLine($"consumer.Received process time: {elapsedMs.ToString()} ms");
                                Console.WriteLine("----------------------------------------------");

                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            };
                channel.BasicConsume(queue: "task_queue",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine("Press [enter]  to exit.");
                Console.ReadLine();
            }
        }
    }
}
