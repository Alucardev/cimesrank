using ApiRanking.Services;
using ApiRanking.Infrastructure.Data;
using ApiRanking.Infrastructure;
using ApiRanking.Infrastructure.Data.Abstractions;
using ApiRanking.Services.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<RankingFiadoService>();
builder.Services.AddScoped<RankingRecaudacionService>();
builder.Services.AddSingleton<IHostedService, RefreshRankingWorker>(); 
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<SubscriberService>();    
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseCors("AllowAll");


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

