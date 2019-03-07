using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsumExchange();


            Console.ReadLine();
        }


        static void ConsumeQueue()
        {
            var factory = new ConnectionFactory();
            var conn = factory.CreateConnection();
            var model = conn.CreateModel();

            model.QueueDeclare("testing", true, false, false, null);
            model.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (sender, arg) =>
            {
                Console.WriteLine(DateTime.Now.ToString() + "<<" + Encoding.UTF8.GetString(arg.Body));
                //if[autoAck==false] model.BasicAck(arg.DeliveryTag, false);
            };

            model.BasicConsume("testing", true, consumer);
        }

        static void ConsumExchange()
        {
            var factory = new ConnectionFactory();
            var conn = factory.CreateConnection();
            var model = conn.CreateModel();



            model.ExchangeDeclare("ex_test", "fanout");
            var queue = model.QueueDeclare();
            model.QueueBind(queue.QueueName, "ex_test","");

            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (sender, arg) =>
            {
                Console.WriteLine(DateTime.Now.ToString() + "<<" + Encoding.UTF8.GetString(arg.Body));
                //if[autoAck==false] model.BasicAck(arg.DeliveryTag, false);
            };

            model.BasicConsume(queue.QueueName, true, consumer);

            

        }
    }
}
