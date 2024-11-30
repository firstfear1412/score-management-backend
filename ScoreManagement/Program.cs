using ScoreManagement.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ScoreManagement.Services.Encrypt;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var env = builder.Environment;
// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.AllowTrailingCommas = true;
    });
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

//builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddDbContext<demoDB>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("DemoDB"));
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

app.MapControllers();

app.Run();
