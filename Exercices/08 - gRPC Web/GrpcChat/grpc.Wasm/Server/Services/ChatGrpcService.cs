using ChatServer.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using grpc.Wasm.Server.Utils;
using Microsoft.AspNetCore.Authorization;

namespace Chagrpc.Wasm.Server.Services
{
    public class ChatGrpcService : ChatServer.Protos.ChatGrpcService.ChatGrpcServiceBase
    {
        private readonly ILogger<ChatGrpcService> _logger;
        public ChatGrpcService(ILogger<ChatGrpcService> logger)
        {
            _logger = logger;
        }

        public override async Task StartCounter(CounterRequest request, IServerStreamWriter<CounterResponse> responseStream, ServerCallContext context)
        {
            var count = request.Start;

            while (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new CounterResponse
                {
                    Count = ++count
                });

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }


       

        public override async Task<RoomRegistrationResponse> RegisterToRoom(RoomRegistrationRequest request, ServerCallContext context)
        {
            UsersQueue.CreateUserQueue(request.RoomName, request.UserName);
            var resp = new RoomRegistrationResponse { JoinedRoom = true };
            return await Task.FromResult(resp);
        }

        public override async Task<ChatMessageResponse> SendMessage(ChatMessage request, ServerCallContext context)
        {
            string userName = request.UserName;
            string room = request.RoomName;
            UsersQueue.AddMessageToRoom(ConvertToReceivedMessage(request), room);
            var resp = new ChatMessageResponse { Success = true };
            return await Task.FromResult(resp);
        }

        public override async Task PullMessages(RoomRequestMessage request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            var respTask = Task.Run(async () =>
            {
                while (true)
                {
                    var userMsg = UsersQueue.GetMessageForUser(request.UserName);
                    if (userMsg != null)
                    {
                        var userMessage = ConvertToChatMessage(userMsg, request.RoomName);
                        await responseStream.WriteAsync(userMessage);
                    }
                    if (MessageQueue.GetMessagesCount() > 0)
                    {
                        var news = MessageQueue.GetNextMessage();
                        var newsMessage = ConvertToChatMessage(news, request.RoomName);
                        await responseStream.WriteAsync(newsMessage);
                    }

                    await Task.Delay(200);
                }
            });

            // Keep the method running
            while (true)  {
                await Task.Delay(10000);
            }
        }


        private ReceivedMessage ConvertToReceivedMessage(ChatMessage chatMsg)  {
            var rcMsg = new ReceivedMessage
            {
                Content = chatMsg.Content,
                MsgTime = chatMsg.MsgTime,
                User = chatMsg.UserName
            };
            return rcMsg;
        }

        private ChatMessage ConvertToChatMessage(ReceivedMessage rcMsg, string room)  {
            var chatMsg = new ChatMessage
            {
                Content = rcMsg.Content,
                MsgTime = rcMsg.MsgTime,
                UserName = rcMsg.User,
                RoomName = room
            };
            return chatMsg;
        }

    }
}
