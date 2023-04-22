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

        public override async Task<RoomRegistrationResponse> RegisterToRoom(RoomRegistrationRequest request, ServerCallContext context)
        {
            UsersQueue.CreateUserQueue(request.RoomName, request.UserName);
            var resp = new RoomRegistrationResponse { JoinedRoom = true };
            return await Task.FromResult(resp);
        }

        public override async Task<NewsStreamStatus> SendNewsFlash(IAsyncStreamReader<NewsFlash> newsStream, ServerCallContext context)
        {
            while (await newsStream.MoveNext())
            {
                var news = newsStream.Current;
                MessageQueue.AddNewsToQueue(news);
                foreach (var queue in UsersQueue.GetUsersQueues())
                {
                    UsersQueue.AddMessageToRoom(new ReceivedMessage { User = "NewBot" , Content = news.NewsItem , MsgTime = news.NewsTime} , queue.Room);
                }
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

                if (UsersQueue.GetAdminQueueMessageCount() > 0)
                {
                    await streamWriter.WriteAsync(UsersQueue.GetNextAdminMessage());
                }

                await Task.Delay(1000);
            }
        }

        public override async Task StartChatting(IAsyncStreamReader<ChatMessage> incomingStream, IServerStreamWriter<ChatMessage> outgoingStream, ServerCallContext context)  {
	
            // Wait for the first message to get the user name
            while (!await incomingStream.MoveNext())  {
                await Task.Delay(100);
            }

            string userName = incomingStream.Current.UserName;
            string room = incomingStream.Current.RoomName;
            Console.WriteLine($"User {userName} connected to room {incomingStream.Current.RoomName}");

            // TO USE ONLY WHEN TESTING WITH POSTMAN -- WILL BE REMOVED LATER
            //UsersQueue.CreateUserQueue(room, userName);
            // END TEST END TEST END TEST

            // Get messages from the user
            var reqTask = Task.Run(async () =>
            {
                while (await incomingStream.MoveNext())
                {
                    Console.WriteLine($"Message received: {incomingStream.Current.Content}");
                    UsersQueue.AddMessageToRoom(ConvertToReceivedMessage(incomingStream.Current), incomingStream.Current.RoomName);
                }
            });


            // Check for messages to send to the user from users queue and admin queue
            var respTask = Task.Run(async () =>
            {
                while (true)
                {
                    var userMsg = UsersQueue.GetMessageForUser(userName);
                    if (userMsg != null)
                    {
                        var userMessage = ConvertToChatMessage(userMsg, room);
                        await outgoingStream.WriteAsync(userMessage);
                    }
                    if (MessageQueue.GetMessagesCount() > 0)
                    {
                        var news = MessageQueue.GetNextMessage();
                        var newsMessage = ConvertToChatMessage(news, room);
                        await outgoingStream.WriteAsync(newsMessage);
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
