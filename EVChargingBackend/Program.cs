using EVChargingBackend.Mappings;
using EVChargingBackend.Services;  // For IUserService and UserService
using Microsoft.AspNetCore.Authentication.JwtBearer;  // For JwtBearerDefaults
using Microsoft.Extensions.Configuration;  // For IConfiguration
using Microsoft.IdentityModel.Tokens;  // For JwtBearerDefaults, TokenValidationParameters
using MongoDB.Driver;
using MongoDB.Driver;  // For MongoDB-related functionality
using System.IdentityModel.Tokens.Jwt;
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
builder.Services.AddScoped<IEVOwnerService, EVOwnerService>();
builder.Services.AddScoped<IChargingStationService, ChargingStationService>();
builder.Services.AddScoped<IChargingSlotService, ChargingSlotService>();


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,

            // Allow multiple issuers: localhost (for web/direct access) and the specific
            // IP address (192.168.1.12) used by the Android emulator.
            ValidIssuers = new[]
            {
                "http://localhost:5033",
                "http://10.0.2.2:5033/"
            },

            ValidAudience = "http://localhost:3000",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),

        };
    });


// Define the CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowFrontendOrigin", // Give your policy a name
                      policy =>
                      {
                          // Allow requests from your React development server
                          policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
                          policy.WithOrigins("https://ev-charging-booking-system-booking.vercel.app").AllowAnyHeader().AllowAnyMethod();
                      });
});

builder.Services.AddControllers();
//AutoMapper
builder.Services.AddAutoMapper(typeof(EVChargingBackend.Mappings.AutoMapping));

var app = builder.Build();

// Apply the CORS Policy
app.UseCors("AllowFrontendOrigin");

app.UseAuthentication();
app.UseAuthorization();

//app.UseDeveloperExceptionPage();
app.MapControllers();

app.Run();
