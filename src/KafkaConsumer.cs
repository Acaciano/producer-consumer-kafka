using System;
using System.Threading;
using Confluent.Kafka;

namespace consumer
{
    public class KafkaConsumer
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        static ConsumerConfig consumerConfig = null;
        static IProducer<string, string> producer = null;
        static ProducerConfig producerConfig = null;
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando o Projeto!");
            bool sair = true;
            int count = 0;

            while (sair)
            {                
                Console.WriteLine("Digite 1 para publicar uma mensagem!");
                Console.WriteLine("Digite 2 para consumir mensagem criada!");
                Console.WriteLine("Digite 3 para sair!");
                Console.WriteLine("");
                Console.WriteLine("");

                string opcao = Console.ReadKey().Key.ToString().Replace("D","");

                if (opcao == "1")
                {
                    count++;

                    ProducerCreateConfig();
                    CreateProducer();
                    SendMessage("TestCassiano", "This is a test42 " + count.ToString());
                }
                
                if (opcao == "2")
                {
                    CreateConfig();
                    CreateConsumerAndConsume();
                }

                if (opcao == "3")
                {
                    sair = false;
                }
            }                           
        }

        static async void SendMessage(string topic, string message)
        {
            Console.WriteLine("Gerando a Mensagem na fila");
            var msg = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            };

            var delRep = await producer.ProduceAsync(topic, msg);
            var topicOffset = delRep.TopicPartitionOffset;

            Console.WriteLine("Gerando a Mensagem na fila");
            Console.WriteLine($"Delivered '{delRep.Value}' to: {topicOffset}");
            Console.WriteLine("");
        }

        static void CreateProducer()
        {
            var pb = new ProducerBuilder<string, string>(producerConfig);
            producer = pb.Build();
        }

        static void ProducerCreateConfig()
        {
            Console.WriteLine(" Informe o bootstrapServers: (exemplo: 10.97.120.173:9092)");
            var bootstrapServers = Console.ReadLine();

            Console.WriteLine($" Capturando a configuração do BootstrapServers {bootstrapServers} -- Producer");
            producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            Console.WriteLine("");
        }

        static void CreateConfig()
        {
            Console.WriteLine("Informe o bootstrapServers: (exemplo: 10.97.120.173:9092)");
            var bootstrapServers = Console.ReadLine();

            Console.WriteLine($"Capturando a configuração do BootstrapServers {bootstrapServers} -- Consumer");
            consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "test-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            Console.WriteLine("");
        }

        static void CreateConsumerAndConsume()
        {
            Console.WriteLine("Vamos consumir a fila");
            var cb = new ConsumerBuilder<string, string>(consumerConfig);

            Console.WriteLine("Press Ctrl+C to exit");

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

            using (var consumer = cb.Build())
            {
                consumer.Subscribe("TestCassiano");

                try
                {
                    while (!cts.IsCancellationRequested)
                    {
                        var cr = consumer.Consume(cts.Token);
                        var offset = cr.TopicPartitionOffset;

                        Console.WriteLine($"Message '{cr.Value}' at: '{offset}'.");
                        Console.WriteLine("");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Olha o erro ai!");
                    Console.WriteLine(e.Message);
                    consumer.Close();
                }
            }
        }

        static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            Console.WriteLine("In OnExit");
            cts.Cancel();

        }
    }
}