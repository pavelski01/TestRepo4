//#define DUMMY
//#define UNARY
//#define SERVER_STREAMING
//#define CLIENT_STREAMING
#define BIDI_STREAMING

using Greet;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace client
{
    internal class Program
    {
        const string target = "127.0.0.1:50051";

        static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    Console.WriteLine("The client connected successfully");
                }
            });
#if DUMMY
            var client = new DummyService.DummyServiceClient(channel);
#endif

#if UNARY
            var client = new GreetingService.GreetingServiceClient(channel);
            var greeting = new Greeting
            {
                FirstName = "Bruce",
                LastName = "Wayne"
            };
            var request = new GreetingRequest
            {
                Greeting = greeting
            };
            var response = client.Greet(request);
            Console.WriteLine(response.Result);
#endif
#if SERVER_STREAMING
            var client = new GreetingService.GreetingServiceClient(channel);
            var greeting = new Greeting
            {
                FirstName = "Bruce",
                LastName = "Wayne"
            };
            var request = new GreetManyTimesRequest { Greeting = greeting };
            var response = client.GreetManyTimes(request);

            while (await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.Result);
                await Task.Delay(200);
            }
#endif
#if CLIENT_STREAMING
            var client = new GreetingService.GreetingServiceClient(channel);
            var greeting = new Greeting
            {
                FirstName = "Bruce",
                LastName = "Wayne"
            };
            var request = new LongGreetRequest { Greeting = greeting };
            var stream = client.LongGreet();
            foreach (var _ in Enumerable.Range(0, 10)) 
            {
                await stream.RequestStream.WriteAsync(request);
            }
            await stream.RequestStream.CompleteAsync();
            var response = await stream.ResponseAsync;
            Console.WriteLine(response.Result);
#endif
#if BIDI_STREAMING
            var client = new GreetingService.GreetingServiceClient(channel);
            var stream = client.GreetEveryone();
            var responseReader = Task.Run(async () => 
            { 
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine("Received: " + stream.ResponseStream.Current.Result);
                }
            });
            Greeting[] greetings =
            {
                new Greeting { FirstName = "Bruce", LastName = "Wayne" },
                new Greeting { FirstName = "Luke", LastName = "Skywalker"  },
                new Greeting { FirstName = "Tylor", LastName = "Swift"  }
            };

            foreach (var greeting in greetings)
            {
                Console.WriteLine("Sending: " + greeting.ToString());
                await stream.RequestStream.WriteAsync(new GreetEveryoneRequest
                {
                    Greeting = greeting
                });
            }

            await stream.RequestStream.CompleteAsync();
            await responseReader;
#endif
            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }
}
