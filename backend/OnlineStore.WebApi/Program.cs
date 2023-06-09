using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineStore.Data;
using OnlineStore.Data.Repositories;
using OnlineStore.Domain.RepositoryInterfaces;
using OnlineStore.Domain.Services;
using OnlineStore.WebApi.Configurations;
using OnlineStore.WebApi.Filters;
using OnlineStore.WebApi.Mappers;
using OnlineStore.WebApi.Middlewares;
using OnlineStore.WebApi.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting up");
try
{
    
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<AppExceptionFilter>();
        options.Filters.Add<ApiKeyFilter>();
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
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
                Array.Empty<string>()
            }
        });
    });
    builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("SmtpConfig"));
    builder.Services.AddScoped<HttpModelsMapper>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IAccountRepository, AccountRepository>();
    builder.Services.AddScoped<ICartRepository, CartRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IParentCategoryRepository, ParentCategoryRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWorkEf>();
    builder.Services.AddScoped<IEmailSender, SmtpMailKitEmailSender>();
    builder.Services.AddScoped<AccountService>();
    builder.Services.AddScoped<ProductService>();
    builder.Services.AddScoped<CartService>();
    builder.Services.AddScoped<CategoryService>();
    builder.Services.AddScoped<ParentCategoryService>();
    builder.Services.AddScoped<OrderService>();
    builder.Services.AddSingleton<IPasswordHasherService, Pbkdf2PasswordHasher>();
    builder.Services.AddScoped<ITokenService, JwtTokenService>();
    builder.Services.AddSingleton<IClock, RealClock>();
    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.RequestHeaders
                                | HttpLoggingFields.ResponseHeaders;
    });
    var jwtConfig = builder.Configuration
        .GetSection("JwtConfig")
        .Get<JwtConfig>()!;
    builder.Services.AddSingleton(jwtConfig);
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(jwtConfig.SigningKeyBytes),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,

                ValidateAudience = true,
                ValidateIssuer = true,
                ValidAudiences = new[] { jwtConfig.Audience },
                ValidIssuer = jwtConfig.Issuer
            };
        });
    builder.Services.AddAuthorization();

    const string dbPath = "myapp.db";
    builder.Services.AddDbContext<AppDbContext>(
        options => options.UseSqlite($"Data Source={dbPath}"));

    builder.Host.UseSerilog((ctx, conf) =>
    {
        conf
            .MinimumLevel.Debug()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .ReadFrom.Configuration(ctx.Configuration);
    });


    builder.Services.AddCors();
    
    var app = builder.Build();

    app.UseStaticFiles();
    
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseMiddleware<PageTransitionsMiddleware>();
    app.UseHttpLogging();

    app.UseCors(policy =>
    {
        policy
            .WithOrigins("http://localhost:7079", "https://aridon.azurewebsites.net")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Unhandled exception on server startup");
    throw;
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush(); 
}

public partial class Program{}
