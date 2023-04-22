using ChatServer.Protos;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using grpc.WebClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton(services =>
{
    // Create a channel with a GrpcWebHandler that is addressed to the backend server.
    //
    // GrpcWebText is used because server streaming requires it. If server streaming is not used in your app
    // then GrpcWeb is recommended because it produces smaller messages.
    var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());

    return GrpcChannel.ForAddress("https://localhost:7284", new GrpcChannelOptions { HttpHandler = httpHandler });
});
//builder.Services
//    .AddGrpcClient<ChatGrpcService.ChatGrpcServiceClient>(options =>
//    {
//        options.Address = new Uri("https://localhost:7284");
//    })
//    .ConfigurePrimaryHttpMessageHandler(
//        () => new GrpcWebHandler(GrpcWebMode.GrpcWebText,new HttpClientHandler()));

await builder.Build().RunAsync();
