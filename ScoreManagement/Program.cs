using ScoreManagement.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ScoreManagement.Services.Encrypt;
//using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ScoreManagement.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ScoreManagement.Interfaces;
using ScoreManagement.Query;
using ScoreManagement.Services.Mail;
using ScoreManagement.Interfaces.Dashboard;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Query.Dashboard;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var env = builder.Environment;

// Add CORS allow all origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // ระบุ origin ที่อนุญาต
                  .AllowAnyHeader()   // อนุญาตให้ทุก header
                  .AllowAnyMethod()   // อนุญาตให้ทุก HTTP method
                  .AllowCredentials();  // อนุญาต credentials (cookies, authorization headers, etc.)
        });
});

// add services Authentication and Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JWT:Issuer"], // เปลี่ยนเป็น Issuer ของคุณ
            ValidAudience = configuration["JWT:Issuer"], // เปลี่ยนเป็น Audience ของคุณ
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:PrivateKey"]!)) // ใส่ Secret Key ที่ปลอดภัย
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddSignalR();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.AllowTrailingCommas = true;
    })
    //.AddJsonOptions(options =>
    //{
    //    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    //})
    ;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
// Add Swagger services with customized schema IDs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BRF API",
        Version = "v1",
        Description = "BRF Application API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Edwin Deloso",
            Email = "ext.edwin.deloso@ph.dsv.com",
            Url = new Uri("http://dsv.com"),
        },

    });

    // Customize schema Ids
    c.CustomSchemaIds(type => type.FullName);

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authirization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
    // Map IFormFile to a Swagger file schema
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

});

builder.Services.AddTransient<IEncryptService, EncryptService>();
builder.Services.AddTransient<IUserQuery, UserQuery>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddTransient<IStudentScoreQuery, StudentScoreQuery>();
builder.Services.AddTransient<IMasterDataQuery, MasterDataQuery>();
builder.Services.AddTransient<ILovContantQuery, LovContantQuery>();
builder.Services.AddTransient<ISystemParamQuery, SystemParamQeury>();
builder.Services.AddTransient<IDashboardQuery, DashboardQuery>();
//builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddDbContext<scoreDB>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("scoreDb"));
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    // Define Thailand's timezone (GMT+7)
    TimeZoneInfo thailandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    // Get current time in Thailand timezone
    DateTime thailandTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, thailandTimeZone);

    // Set Date header in response
    context.Response.Headers["Date"] = thailandTime.ToString("R"); // "R" format for RFC1123

    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Use CORS policy
app.UseCors("AllowSpecificOrigin");

// Add signalIR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Run();
