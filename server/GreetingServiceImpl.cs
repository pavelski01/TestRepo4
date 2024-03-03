using Greet;
using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Greet.GreetingService;

namespace server
{
    public class GreetingServiceImpl : GreetingServiceBase
    {
        public override Task<GreetingResponse> Greet(GreetingRequest request, ServerCallContext context)
        {
            var result = $"Hello {request.Greeting.FirstName} {request.Greeting.LastName}";
            return Task.FromResult(new GreetingResponse { Result = result });
        }

        public override async Task GreetManyTimes(GreetManyTimesRequest request, IServerStreamWriter<GreetManyTimesResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine("The server received the reqest: ");
            Console.WriteLine(request.ToString());
            var result = $"Hello {request.Greeting.FirstName} {request.Greeting.LastName}";
            foreach (var _ in Enumerable.Range(1, 10))
            {
                await responseStream.WriteAsync(new GreetManyTimesResponse { Result = result });
            }
        }

        public override async Task<LongGreetResponse> LongGreet(IAsyncStreamReader<LongGreetRequest> requestStream, ServerCallContext context)
        {
            var result = string.Empty;
            while (await requestStream.MoveNext())
            {
                result += $"Hello {requestStream.Current.Greeting.FirstName} {requestStream.Current.Greeting.LastName} {Environment.NewLine}";
            }
            return new LongGreetResponse() { Result = result };
        }

        public override async Task GreetEveryone(IAsyncStreamReader<GreetEveryoneRequest> requestStream, IServerStreamWriter<GreetEveryoneResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var result = $"Hello {requestStream.Current.Greeting.FirstName} {requestStream.Current.Greeting.LastName}";
                Console.WriteLine("Sending: " + result);
                await responseStream.WriteAsync(new GreetEveryoneResponse { Result = result });
            }
        }
    }
}
