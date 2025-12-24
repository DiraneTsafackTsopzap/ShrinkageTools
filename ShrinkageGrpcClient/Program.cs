using Grpc.Core;
using Grpc.Net.Client;
using GrpcShrinkageServiceTraining.Protobuf;

var channel = GrpcChannel.ForAddress("http://localhost:9090");

// Le nom du Service ici est celui défini dans le fichier .proto
var client = new ShrinkageProtoService.ShrinkageProtoServiceClient(channel);

GetUserByEmailResponse? response = null;

try
{
    response = await client.GetUserByEmailAsync(new GetUserByEmailRequest
    {
        CorrelationId = new AppUuid { Value = Guid.NewGuid().ToString() },
        Email = "diraneserges@gmail.com"
    });
}
catch (RpcException ex)
{
    Console.WriteLine("🔥 gRPC ERROR:");
    Console.WriteLine($"StatusCode: {ex.StatusCode}");
    Console.WriteLine($"Detail: {ex.Status.Detail}");
    Console.WriteLine(ex);
    return;
}
catch (Exception ex)
{
    Console.WriteLine("🔥 CLIENT ERROR:");
    Console.WriteLine(ex);
    return;
}

if (response?.User == null)
{
    Console.WriteLine("No user returned (response.User is null).");
    return;
}

Console.WriteLine("USER FOUND:");
Console.WriteLine($"Email: {response.User.Email}");
Console.WriteLine($"UserId: {response.User.UserId?.Value}");
Console.WriteLine($"TeamId: {response.User.TeamId.Value}");
Console.WriteLine($"ValidFrom: {response.User.ValidFrom}");

Console.ReadKey();