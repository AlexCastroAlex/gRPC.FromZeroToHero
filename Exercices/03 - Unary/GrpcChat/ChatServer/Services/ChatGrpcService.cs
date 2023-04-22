using ChatServer.Protos;
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
    }
}
