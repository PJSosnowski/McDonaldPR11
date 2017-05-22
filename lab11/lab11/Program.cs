using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using System.Threading;
using RabbitMQ.Client;
//using RabbitMQ.Client.Events;

namespace lab11
{
    class Program
    {

        static ConnectionFactory factory = new ConnectionFactory();
        static void Main(string[] args)
        {
            //factory.Uri = "amqp://user:pass@hostName:port/vhost";
            //factory.Uri = "amqp://ciytdxcx:J-vYXXMw7_1-_odJMT0wr_Ri4ToPNaP7@puma.rmq.cloudamqp.com:5672/ciytdxcx";
            factory.UserName = "ciytdxcx";
            factory.Password = "J-vYXXMw7_1-_odJMT0wr_Ri4ToPNaP7";
            factory.VirtualHost = "ciytdxcx";
            factory.HostName = "puma.rmq.cloudamqp.com";
            IConnection conn = factory.CreateConnection();

            IModel channel = conn.CreateModel();

            channel.ExchangeDeclare("test1", ExchangeType.Direct);
            //channel.QueueDeclare(queueName, false, false, false, null);
            //channel.QueueBind(queueName, exchangeName, routingKey, null);

            Thread kasa = new Thread(() =>
            {
                while(true)
                {
                    Console.WriteLine("Kasa: Wyslano dane do exchange'a z zamowieniem hambuergera");
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Poprosze hambixa");
                    channel.BasicPublish("test1", "", null, messageBodyBytes);
                    Thread.Sleep(1000);
                }
            });
            Thread kuchnia = new Thread(() =>
             {
                 bool noAck = false;
                 
                while (true)
                {
                    RabbitMQ.Client.BasicGetResult result = channel.BasicGet("kolejkaZZamowieniami", noAck);
                    if (result == null)
                    {
                        // No message available at this time.
                    }
                    else
                    {
                        IBasicProperties props = result.BasicProperties;
                        byte[] body = result.Body;
                        Console.WriteLine("Klient: otrzymalem hamburgera!");
                        channel.BasicAck(result.DeliveryTag, false);
                    }
                }
             });
            kasa.Start();
            kuchnia.Start();
        }


    }

   
}
