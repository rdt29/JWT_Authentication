using JWT_Practice.DaraBase;
using JWT_Practice.MiddleWare;

//using JWT_Practice.MiddleWare;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//?-------------------------database Connection ==============================================

builder.Services.AddDbContext<JwtDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

//?-------------------------Api swager Authorization Connection ==============================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
}
);

//?--------------------------JWT authentication ---------------------------------------------

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

//?----------------------- Serilog Configuraion  --------------------------------

string con = builder.Configuration.GetConnectionString("DefaultConnection");
string table = "Logs";

//var _Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .WriteTo.MSSqlServer(con, table).CreateLogger();
//builder.Logging.AddSerilog(_Logger);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.MSSqlServer(con, table).CreateLogger();
//builder.Logging.AddSerilog(_Logger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.ConfigureGlobalExceptionHandling();
//? GLobal Exception handling----------------------------
app.UseMiddleware<GlobalExceptionHandlling>();

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

//app.UseMiddleware<GlobalExceptionHandlling>();

app.MapControllers();

app.Run();