using GraphProcessorAPI.Repositories;
using GraphProcessorAPI.Models;
using GraphProcessorAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var databaseConnectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddControllers();

builder.Services.AddHttpLogging(logging => { });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddProblemDetails();

builder.Services.AddScoped<IDistanceGraphProcessorService, DistanceGraphProcessingService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();

builder.Services.AddDbContextPool<GraphProcessorContext>(options =>
    options.UseNpgsql(
        databaseConnectionString, o => 
        {
            o.MapEnum<UserRole>("accountrole");
            o.MapEnum<GraphType>("graphtype");
            o.MapEnum<AlgorithmType>("algorithm_type");
        }
    ));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtParams:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtParams:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtParams:SecretKey"])),
            ValidateIssuerSigningKey = true
        };
    });

var app = builder.Build();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Graph Processor API V1");
        options.RoutePrefix = string.Empty;
    });
}
if (app.Environment.IsProduction())
{
    app.UseExceptionHandler();
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GraphProcessorContext>();
        db.Database.Migrate();
    }
}

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "Welcome to Graph Processor API" }));

app.Run();