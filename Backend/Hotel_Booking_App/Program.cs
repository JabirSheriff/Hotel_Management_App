using Hotel_Booking_App.Contexts;
using Hotel_Booking_App.Interface;
using Hotel_Booking_App.Interface.Hotel_Room;
using Hotel_Booking_App.Repositories;
using Hotel_Booking_App.Service;
using Hotel_Booking_App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Hotel_Booking_App.Mappings;
using HotelBookingApp.Interfaces;
using HotelBookingApp.Services;
using HotelBookingApp.Repositories;
using Azure.Storage.Blobs;
using Hotel_Booking_App.Interface.Customer;
using Hotel_Booking_App.Interface.Payment;
using Hotel_Booking_App.Interface.Review;
using System.Text.Json.Serialization;

namespace Hotel_Booking_App
{
    public class Program
    {
        public static async Task Main(string[] args) 
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add CORS services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });

            // Register AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddHttpContextAccessor();

            // Load configuration
            var configuration = builder.Configuration;

            // Add services to the container
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Ensures Status is string
             });

            // Add Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });

            // Ensure connection string is loaded correctly
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("Connection string 'DefaultConnection' is not set in appsettings.json.");
            }

            // Database Context Configuration
            builder.Services.AddDbContext<HotelBookingDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register repository and services
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IHotelService, HotelService>();
            builder.Services.AddScoped<IHotelRepository, HotelRepository>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddHostedService<BookingCleanupService>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddSingleton(new BlobServiceClient(builder.Configuration["AzureBlobStorage:ConnectionString"]));
            builder.Services.AddScoped<DatabaseSeeder>(); 

            // Configure Authentication & JWT
            var jwtSecret = configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
                throw new ArgumentNullException("Jwt:Secret is missing in appsettings.json.");

            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            var app = builder.Build();

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync(); // Run seeder asynchronously
            }

            app.UseRouting();
            app.UseCors("AllowAllOrigins");
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync(); // Changed to async run
        }
    }
}