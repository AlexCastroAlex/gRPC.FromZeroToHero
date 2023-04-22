using ChatServer.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("https://localhost:7284");
var client = new ChatGrpcService.ChatGrpcServiceClient(channel);

Console.WriteLine("*** Admin Console started ***");
Console.WriteLine("Listening...");

using var call = client.StartMonitoring(new Empty() );
var cts = new CancellationTokenSource();
while (await call.ResponseStream.MoveNext(cts.Token))
{
    Console.WriteLine($"Message received from : {call.ResponseStream.Current.User} at {call.ResponseStream.Current.MsgTime.ToDateTime()} with content : {call.ResponseStream.Current.Content}");
}