using ChatServer.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("https://localhost:7284");
var client = new ChatGrpcService.ChatGrpcServiceClient(channel);

Console.WriteLine("Welcome the the gRPC chat!");
Console.Write("Please type your user name: ");
var username = Console.ReadLine();

Console.Write("Please type the name of the room you want to join : ");
var room = Console.ReadLine();

Console.WriteLine($"Joining room {room}...");

try
{
    var joinResponse = client.RegisterToRoom(new RoomRegistrationRequest() { RoomName = room, UserName = username });
    if (joinResponse.JoinedRoom)
    {
        Console.WriteLine("Joined successfully!");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error joining room {room}.");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Press any key to close the window.");
        Console.Read();
        return;
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error joining room {room}. Error: {ex.Message}");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("Press any key to close the window.");
    Console.Read();
    return;
}

Console.WriteLine($"Press any key to enter the {room} room.");
Console.Read();
Console.Clear();

var call = client.StartChatting();
var cts = new CancellationTokenSource();

var promptText = "Type your message: ";
var row = 2;

var task = Task.Run(async () =>
{

    while (true)
    {
        if (await call.ResponseStream.MoveNext(cts.Token))
        {
            var message  = call.ResponseStream.Current;
            //cursor position to stay at same position when we receive a message
            var left = Console.CursorLeft - promptText.Length;
            PrintMessage(message);
        }
        //wait for 0.5 sec before checking for new message
        await Task.Delay(500);
    }
});

Console.Write(promptText);
while (true)
{
    var input = Console.ReadLine();
    RestoreInputCursor();

    var reqMessage = new ChatMessage
    {
        Content = input,
        MsgTime = Timestamp.FromDateTime(DateTime.UtcNow),
        RoomName = room,
        UserName = username
    };
    await call.RequestStream.WriteAsync(reqMessage);
}

void PrintMessage(ChatMessage msg)
{
    var left = Console.CursorLeft - promptText.Length;
    Console.SetCursorPosition(0, row++);
    Console.Write($"{msg.UserName}: {msg.Content}");
    Console.SetCursorPosition(promptText.Length + left, 0);
}

void RestoreInputCursor()  {
    Console.SetCursorPosition(promptText.Length - 1, 0);
    Console.Write("                                    ");
    Console.SetCursorPosition(promptText.Length - 1, 0);
}