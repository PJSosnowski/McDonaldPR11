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
            factory.UserName = "ciytdxcx";
            factory.Password = "J-vYXXMw7_1-_odJMT0wr_Ri4ToPNaP7";
            factory.VirtualHost = "ciytdxcx";
            factory.HostName = "puma.rmq.cloudamqp.com";
            IConnection conn = factory.CreateConnection();

            IModel channel = conn.CreateModel();
            IModel channel2 = conn.CreateModel();

            channel.ExchangeDeclare("KasaKuchniaExchange", ExchangeType.Direct, true);

            Thread kasa = new Thread(() =>
            {
                while(true)
                {
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Hamburger");
                    channel.BasicPublish("KasaKuchniaExchange", "", null, messageBodyBytes);
                    Console.WriteLine("Kasa: Wyslano dane do exchange'a z zamowieniem hambuergera\n");
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
                        Console.WriteLine("Kuchnia: otrzymalem zamowienie hamburgera");
                        channel.BasicAck(result.DeliveryTag, false);

                        Console.WriteLine("Kuchnia: wysylam info do magazynu o zrobieniu hamburgera");
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Hamburger");
                        channel.BasicPublish("MagazynExchange", "", null, messageBodyBytes);

                        Console.WriteLine("Kuchnia: wysylam hamburgera do klienta\n");
                        messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Hamburger");
                        channel.BasicPublish("KuchniaKlientExchange", "", null, messageBodyBytes);
                    }
                }
             });

            Thread klient = new Thread(() =>
            {
                bool noAck = false;
                while(true)
                {
                    RabbitMQ.Client.BasicGetResult result = channel2.BasicGet("kolejkaPrzedKlientem", noAck);
                    if (result == null)
                    {
                        // No message available at this time.
                    }
                    else
                    {
                        IBasicProperties props = result.BasicProperties;
                        byte[] body = result.Body;
                        Console.WriteLine("Klient: otrzymalem hamburger, pyszniutki!\n");
                        channel2.BasicAck(result.DeliveryTag, false);
                    }
                }
            });
            kasa.Start();
            kuchnia.Start();
            klient.Start();
        }


    }

   
}
