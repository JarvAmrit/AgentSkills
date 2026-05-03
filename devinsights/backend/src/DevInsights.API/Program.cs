using DevInsights.Core.Interfaces;
using DevInsights.Infrastructure.Agents;
using DevInsights.Infrastructure.BackgroundServices;
using DevInsights.Infrastructure.Data;
using DevInsights.Infrastructure.Repositories;
using DevInsights.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using OpenAI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<DevInsightsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=devinsights.db"));

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Microsoft Agent Framework
var openAiApiKey = builder.Configuration["OpenAI:ApiKey"] ?? string.Empty;
var openAiModelId = builder.Configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";
var chatClient = new OpenAIClient(openAiApiKey).GetChatClient(openAiModelId);
builder.Services.AddSingleton(chatClient);
builder.Services.AddTransient<TechnologyClassifierAgent>();
builder.Services.AddTransient<AIWorkClassifierAgent>();
builder.Services.AddTransient<CommitSummaryAgent>();
builder.Services.AddTransient<IAgentOrchestrator, AgentOrchestrator>();

// Azure DevOps
var pat = builder.Configuration["AzureDevOps:PersonalAccessToken"] ?? string.Empty;
builder.Services.AddSingleton<IAzureDevOpsService>(sp =>
    new AzureDevOpsService(pat, sp.GetRequiredService<ILogger<AzureDevOpsService>>()));

// Repositories and Services
builder.Services.AddScoped<IAnalysisRepository, AnalysisRepository>();

// Background Service
builder.Services.AddHostedService<CommitAnalysisBackgroundService>();

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DevInsights API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DevInsightsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactApp");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Fallback for SPA routing
app.MapFallbackToFile("index.html");

app.Run();
