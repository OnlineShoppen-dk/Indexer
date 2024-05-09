using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Indexer.Documents;
using Indexer.Elasticsearch;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class Program
{
    static async Task Main(string[] args)
    {
        var repository = new ElasticsearchRepository();

        var queueContainerHostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "user";
        var password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "userpass";

        var factory = new ConnectionFactory { HostName = queueContainerHostName, UserName = user, Password = password };


        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();


        #region Product Queue handling
        string productQueueName = "productQueue";


        channel.QueueDeclare(queue: productQueueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);



        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async 
            (sender, eventArgs) =>
        {
            try
            {
                Console.WriteLine("Product Received");
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var product = JsonSerializer.Deserialize<Product>(message);

                if (product != null) await repository.IndexProduct(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while adding product. Exception: ", ex.Message);
            }
        };

        channel.BasicConsume(queue: productQueueName,
                             autoAck: true,
                             consumer: consumer);
        #endregion

        #region Product Delete Queue Handling

        string productDeleteQueueName = "productDeleteQueue";


        channel.QueueDeclare(queue: productDeleteQueueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);



        var consumerDelete = new EventingBasicConsumer(channel);
        consumerDelete.Received += async (sender, eventArgs) =>
        {
            try
            {
                Console.WriteLine("Product to be deleted ID received");

                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await repository.DeleteProduct(message);
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Error while deleting product. Exception: ", ex.Message);
            }


        };

        channel.BasicConsume(queue: productDeleteQueueName,
                             autoAck: true,
                             consumer: consumerDelete);
        #endregion



        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
        await Task.Run(() => Thread.Sleep(Timeout.Infinite));
    }
}
