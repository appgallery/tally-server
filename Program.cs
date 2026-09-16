using TallyServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

// SignalR endpoint
app.MapHub<TallyHub>("/tallyHub");

app.Run();