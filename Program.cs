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

            // Add services to the container
            builder.Services.AddControllers();
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

            var app = builder.Build();

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
