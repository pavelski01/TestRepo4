﻿//#define DUMMY
//#define UNRARY

//#define CLIENT_STREAMING

using Dummy;
using Greet;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

#if UNRARY
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
#if CLIENT_STREAMING
            var client = new GreetingService.GreetingServiceClient(channel);
            var greeting = new Greeting
            {
                FirstName = "Bruce",
                LastName = "Wayne"
            };
            var request = new LongGreetRequest { Greeting = greeting };
            var stream = client.LongGreet();
            foreach (var i in Enumerable.Range(0, 10)) 
            {
                await stream.RequestStream.WriteAsync(request);
            }
            await stream.RequestStream.CompleteAsync();
            var response = await stream.ResponseAsync;
            Console.WriteLine(response.Result);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
#endif
        }
    }
}
