
using ETICARET.API.Identity;
using ETICARET.Business.Abstract;
using ETICARET.Business.Concrete;
using ETICARET.DataAccess.Abstract;
using ETICARET.DataAccess.Concrete.EfCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace ETICARET.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"))
            );

            // Identity Servisleri
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
            .AddDefaultTokenProviders();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "ETICARET.API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer" // Yukarýda tanýmladýðýmýz Bearer tanýmýný kullan.
                            }
                        },
                        Array.Empty<string>() // Boþ Array: Tüm endpointler için geçerli.
                    }
                });
            });

            // Business ve Data Access Layer
            builder.Services.AddScoped<IProductDal, EfCoreProductDal>();
            builder.Services.AddScoped<IProductService, ProductManager>();
            builder.Services.AddScoped<ICategoryDal, EfCoreCategoryDal>();
            builder.Services.AddScoped<ICategoryService, CategoryManager>();
            builder.Services.AddScoped<ICommentDal, EfCoreCommentDal>();
            builder.Services.AddScoped<ICommentService, CommentManager>();
            builder.Services.AddScoped<ICartDal, EfCoreCartDal>();
            builder.Services.AddScoped<ICartService, CartManager>();
            builder.Services.AddScoped<IOrderDal, EfCoreOrderDal>();
            builder.Services.AddScoped<IOrderService, OrderManager>();

            // JWT Token Yapýlandýrmasý
            // Kullanýcý giriþ yaptýktan sonra aldýðý þifreli bir "kimlik kartý" gibidir.
            // Her api isteðinde bu token'ý doðrulayarak kullanýcýnýn kimliðini teyit ederiz.
            // Tokenlar client tarafýnda saklanýr (örneðin local storage veya cookie).
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            // Kimlik Doðrulama Servisi
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    // Issuer (Tokenin kim tarafýndan üretildiði)
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],

                    // Audience (Tokenin kim tarafýndan kullanýlacaðý)
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],

                    // Token'ýn geçerlilik süresi
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),

                    // Token'ýn hemen geçersiz sayýlmamasý için bir süre tanýmlanabilir
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Token Doðrulamasý Baþarýsýz: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token Baþarýyla Doðrulandý.");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine("Yetkilendirme Baþarýsýz: " + context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => 
                    builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
