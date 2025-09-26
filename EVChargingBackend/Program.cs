using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;  // For JwtBearerDefaults
using Microsoft.Extensions.Configuration;  // For IConfiguration
using Microsoft.IdentityModel.Tokens;  // For JwtBearerDefaults, TokenValidationParameters
using EVChargingBackend.Services;  // For IUserService and UserService
using MongoDB.Driver;  // For MongoDB-related functionality
using System.Text;  // For encoding the SecretKey

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB connection
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = new MongoClient(builder.Configuration.GetConnectionString("MongoDb"));
    return client.GetDatabase("EVChargingDB");  // Connect to your EVChargingDB
});

// Register services for User Management and JWT Authentication
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = "http://localhost:5033",//  backend API runs on port 5000
            ValidAudience = "http://localhost:3000",// Assuming you plan to have your frontend running on port 3000 (common for React apps)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),

        };
    });


builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
//app.UseDeveloperExceptionPage();
app.MapControllers();

app.Run();
