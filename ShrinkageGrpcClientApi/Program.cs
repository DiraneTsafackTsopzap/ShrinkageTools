
using GrpcShrinkageServiceTraining.Protobuf;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Enregistrement du client gRPC
builder.Services.AddGrpcClient<ShrinkageProtoService.ShrinkageProtoServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:9090");
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
