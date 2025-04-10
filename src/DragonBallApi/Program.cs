using DragonBallApi.Data;
using DragonBallApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration
var configuration = builder.Configuration;

// 2. Logging (already configured by default, add providers if needed)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 3. Services

// -- Database Context --
var connectionString = configuration.GetConnectionString("DefaultConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion,
        mySqlOptions => mySqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)
            .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    )
   
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    .EnableDetailedErrors(builder.Environment.IsDevelopment())
);


// -- HTTP Client Factory --
builder.Services.AddHttpClient("DragonBallApiClient", client =>
{
   
   
    client.DefaultRequestHeaders.Add("Accept", "application/json");
   
});

// -- Register Custom Services --
builder.Services.AddScoped<IDragonBallApiService, DragonBallApiService>();
builder.Services.AddScoped<ISyncService, SyncService>();
// Register other services (like ICharacterService, ITransformationService if you create them)


// -- Authentication (JWT) --
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
    };
});

// -- Authorization --
builder.Services.AddAuthorization();

// -- Controllers --
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });


// -- Swagger/OpenAPI --
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dragon Ball API Integration", Version = "v1" });

   
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


var app = builder.Build();

// 4. Middleware Pipeline

// -- Configure the HTTP request pipeline. --
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dragon Ball API v1");
           
        }
    );
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();