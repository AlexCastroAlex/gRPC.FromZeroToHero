using Grpc.Net.Client;
using ChatServer.Protos;


using var channel = GrpcChannel.ForAddress("https://localhost:7284");
var client = new ChatGrpcService.ChatGrpcServiceClient(channel);
Console.WriteLine("Enter a room name to register :");
var roomName = Console.ReadLine();

var response = client.RegisterToRoom(new RoomRegistrationRequest { RoomName = roomName});
Console.WriteLine($"RoomId : {response.RoomId}");
Console.Read();

