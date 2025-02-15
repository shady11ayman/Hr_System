using Hr_System_Demo_3.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;

namespace Hr_System_Demo_3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var corsSettings = new CorsSettings();
            builder.Configuration.GetSection("CorsSettings").Bind(corsSettings);

            // Register CORS with dynamic origins
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowConfiguredOrigins", policy =>
                    policy.WithOrigins(corsSettings.AllowedOrigins) // Pass the array from settings
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()); // Use this if sending cookies/auth headers
            });


            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection("CorsSettings"));

            builder.Services.AddOpenApi();

            // Database Configuration
            var connectionString = builder.Environment.IsDevelopment()
                ? "Data Source=82.112.254.244,1433;Initial Catalog=Hr_System_DB;Persist Security Info=True;Encrypt=false;TrustServerCertificate=true;User ID=sa;Password=Peter@123"
                : "Data Source=172.17.0.12,1433;Initial Catalog=Hr_System_DB;Persist Security Info=True;Encrypt=false;TrustServerCertificate=true;User ID=sa;Password=Peter@123";

            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            // Configure JWT Authentication
            var jwtSection = builder.Configuration.GetSection("Jwt");
            builder.Services.Configure<JwtOptions>(jwtSection);

            var jwtOptions = jwtSection.Get<JwtOptions>();
            if (jwtOptions == null || string.IsNullOrEmpty(jwtOptions.SigningKey))
            {
                throw new Exception("JWT configuration is missing or invalid.");
            }

            builder.Services.AddSingleton(jwtOptions);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                    policy.WithOrigins("http://localhost:3000", "http://82.112.254.244:8011", "http://192.168.100.44:8081")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()); // Use only if sending cookies/auth headers
            });

            var app = builder.Build();

            app.UseCors("AllowConfiguredOrigins");

            // Configure Middleware
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // OpenAPI / Swagger
            app.MapOpenApi();
            app.MapScalarApiReference();

            app.Run();
        }
    }
}
