using RouteOptimizationApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ApiDatabaseService>();

var app = builder.Build();

var databaseService = app.Services.GetRequiredService<ApiDatabaseService>();
databaseService.InitializeDatabase();


app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:8080");