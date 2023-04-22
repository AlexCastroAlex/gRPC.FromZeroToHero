using ChatServer.Protos;
using ChatServer.Utils;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
namespace ChatServer.Services
{
    public class ChatGrpcService : ChatServer.Protos.ChatGrpcService.ChatGrpcServiceBase
    {
        private readonly ILogger<ChatGrpcService> _logger;
        public ChatGrpcService(ILogger<ChatGrpcService> logger)
        {
            _logger = logger;
        }

        public override Task<RoomRegistrationResponse> RegisterToRoom(RoomRegistrationRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Service called...");
            var rnd = new Random();
            var roomNum = rnd.Next(1, 100);
            _logger.LogInformation($"Room no. {roomNum}");
            var resp = new RoomRegistrationResponse { RoomId = roomNum };
            return Task.FromResult(resp);
        }

        public override async Task<NewsStreamStatus> SendNewsFlash(IAsyncStreamReader<NewsFlash> newsStream, ServerCallContext context)
        {
            while (await newsStream.MoveNext())
            {
                var news = newsStream.Current;
                MessageQueue.AddNewsToQueue(news);
                Console.WriteLine($"Here is new flash info : {news.NewsItem} at {news.NewsTime.ToDateTime()}");
            }

            return new NewsStreamStatus {Success = true};
        }


        public override async Task StartMonitoring(Empty request, IServerStreamWriter<ReceivedMessage> streamWriter,
            ServerCallContext context)
        {
            while (true)
            {
                if (MessageQueue.GetMessagesCount() > 0)
                {
                    await streamWriter.WriteAsync(MessageQueue.GetNextMessage());
                }
                await Task.Delay(1000);
            }
        }
    }
}
