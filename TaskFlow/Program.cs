using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using TaskFlow.Domain.Identity;
using TaskFlow.Features.Tasks;
using TaskFlow.Hubs;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        await RunAsync(args);
    }

    public static async Task RunAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load environment variables from .env file (if exists)
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
        if (!File.Exists(envPath))
        {
            envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        }

        if (File.Exists(envPath))
        {
            var envVars = File.ReadAllLines(envPath)
                .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#'))
                .Select(l => l.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());

            foreach (var kvp in envVars)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }

            Log.Logger.Information(".env loaded from {Path}, JWT_KEY present: {HasJwt}", envPath,
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_KEY")));
        }
        else
        {
            Log.Logger.Warning(".env file not found. Checked: {Path}", envPath);
        }

        // Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/taskflow-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        // Database
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
            }));

        // Identity
        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? builder.Configuration["JwtSettings:Key"]
            ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? builder.Configuration["JwtSettings:Issuer"]
            ?? "TaskFlow";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? builder.Configuration["JwtSettings:Audience"]
            ?? "FrontendApp";
        var jwtExpiration = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"), out var exp)
            ? exp
            : 1440;

        var key = Encoding.UTF8.GetBytes(jwtKey);

        Log.Logger.Information("JWT Configured: Key length={KeyLen}, Issuer={Issuer}, Audience={Audience}, Expiration={Exp}min",
            jwtKey.Length, jwtIssuer, jwtAudience, jwtExpiration);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Logger.Warning(context.Exception, "JWT Authentication failed");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                            Log.Logger.Debug("Token received from query string for SignalR");
                        }
                        else if (context.Request.Headers.ContainsKey("Authorization"))
                        {
                            Log.Logger.Debug("Authorization header present: {Header}", context.Request.Headers["Authorization"].ToString().Substring(0, 20) + "...");
                        }
                        else
                        {
                            Log.Logger.Debug("No Authorization header for {Path}", path);
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        // CORS
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173" };
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            });
        });

        // Services
        builder.Services.AddScoped<ITokenService, TokenService>();

        // MediatR
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TaskFlowMarker).Assembly);
        });

        // FluentValidation
        builder.Services.AddValidatorsFromAssemblyContaining<TaskFlowMarker>();

        // AutoMapper
        builder.Services.AddAutoMapper(config =>
        {
            config.AddMaps(typeof(TaskFlowMarker).Assembly);
        });

        // SignalR
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            options.MaximumReceiveMessageSize = 102400;
        });

        // File Upload
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10485760; // 10 MB
        });

        // Controllers & API
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Swagger
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TaskFlow API",
                Version = "v1",
                Description = "Smart Task Manager API with Kanban board"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
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
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
            });

            c.CustomSchemaIds(type => type.FullName);
        });

        // Min API
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        // Logging
        app.Services.GetRequiredService<ILogger<Program>>().LogInformation("TaskFlow API starting...");

        // Database Migration
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            // Seed roles
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in new[] { "User", "Admin" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed default admin user
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@taskflow.com";
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@1234!";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "TaskFlow",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    await userManager.AddToRoleAsync(admin, "User");
                }
            }
        }

        // Pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API V1");
            });
        }

        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        // Map Controllers
        app.MapControllers();

        // Minimal API feature endpoints
        TaskFlow.Features.Auth.Login.LoginEndpoint.MapLoginEndpoint(app);
        TaskFlow.Features.Auth.Register.RegisterEndpoint.MapRegisterEndpoint(app);
        TaskFlow.Features.Auth.ResetPassword.ResetPasswordEndpoint.MapResetPasswordEndpoints(app);
        TaskFlow.Features.Attachments.UploadAttachment.UploadAttachmentEndpoint.MapUploadAttachmentEndpoint(app);
        TaskFlow.Features.Attachments.GetAttachments.GetAttachmentsEndpoint.MapGetAttachmentsEndpoint(app);
        TaskFlow.Features.Attachments.DownloadAttachment.DownloadAttachmentEndpoint.MapDownloadAttachmentEndpoint(app);
        TaskFlow.Features.Attachments.DeleteAttachment.DeleteAttachmentEndpoint.MapDeleteAttachmentEndpoint(app);
        TaskFlow.Features.Comments.CreateComment.CreateCommentEndpoint.MapCreateCommentEndpoint(app);
        TaskFlow.Features.Comments.GetComments.GetCommentsEndpoint.MapGetCommentsEndpoint(app);
        TaskFlow.Features.Comments.DeleteComment.DeleteCommentEndpoint.MapDeleteCommentEndpoint(app);
        TaskFlow.Features.Notifications.GetNotifications.GetNotificationsEndpoint.MapGetNotificationsEndpoint(app);
        TaskFlow.Features.Notifications.MarkAsRead.MarkAsReadEndpoint.MapMarkAsReadEndpoint(app);

        // Map Hubs
        app.MapHub<TasksHub>("/hubs/tasks");
        app.MapHub<NotificationsHub>("/hubs/notifications");

        // Health check
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}