using Grpc.Net.Client;
using ChatServer.Protos;
using Google.Protobuf.WellKnownTypes;


var news = new List<NewsFlash>
{
    new() { NewsItem = "Weather is sunny !" , NewsTime = Timestamp.FromDateTime(DateTime.UtcNow)},
    new() { NewsItem = "Time to learn gRPC !" , NewsTime = Timestamp.FromDateTime(DateTime.UtcNow)},
    new() { NewsItem = "Go on with gRPC !" , NewsTime = Timestamp.FromDateTime(DateTime.UtcNow)}
};


using var channel = GrpcChannel.ForAddress("https://localhost:7284");
var client = new ChatGrpcService.ChatGrpcServiceClient(channel);

Thread.Sleep(2000);

var call = client.SendNewsFlash();
foreach (var newItem in news)
{
    await call.RequestStream.WriteAsync(newItem);
    Thread.Sleep(2000);
}
await call.RequestStream.CompleteAsync();
var response = await call;

Console.WriteLine($"Streaming status : {(response.Success ?  "Success" : "Failed")}");
Console.Read();
