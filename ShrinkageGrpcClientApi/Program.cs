using GrpcShrinkageServiceTraining.Protobuf;

var builder = WebApplication.CreateBuilder(args);

// =======================
// SERVICES
// =======================

// Controllers (API REST)
builder.Services.AddControllers();

// CORS → autoriser le frontend Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy
            .WithOrigins("http://localhost:7000") // FRONTEND
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Client gRPC
builder.Services.AddGrpcClient<ShrinkageProtoService.ShrinkageProtoServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:9090");
});

var app = builder.Build();

// =======================
// MIDDLEWARE
// =======================

app.UseHttpsRedirection();

app.UseRouting();

// ⚠️ CORS AVANT Authorization
app.UseCors("AllowBlazor");

app.UseAuthorization();

app.MapControllers();

app.Run();
