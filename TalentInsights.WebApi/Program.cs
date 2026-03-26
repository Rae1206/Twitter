using TalentInsights.Application.Interfaces.Services;
using TalentInsights.Application.Models.DTOs;
using TalentInsights.Application.Services;
using TalentInsights.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Services
builder.Services.AddScoped<ICollaboratorService, CollaboratorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();

builder.Services.AddSingleton<Cache<CollaboratorDto>>();
builder.Services.AddSingleton<Cache<UserDto>>();
builder.Services.AddSingleton<Cache<PostDto>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
