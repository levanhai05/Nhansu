using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QLNhanSu.Data;
using QLNhanSu.Middleware;
using QLNhanSu.Models;
using QLNhanSu.Repositories;
using QLNhanSu.Services;
using Serilog;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog (đọc từ appsettings.json: Console + file Logs/log-YYYYMMDD.txt)
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddControllers();

// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Repository + Service cho Auth
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileService, FileService>();

// Đọc cấu hình JwtSettings từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("Chưa cấu hình JwtSettings:SecretKey");

var issuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("Chưa cấu hình JwtSettings:Issuer");

var audience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException("Chưa cấu hình JwtSettings:Audience");

// Cấu hình xác thực JWT
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

        ValidIssuer = issuer,
        ValidAudience = audience,
        RoleClaimType = ClaimTypes.Role,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey))
    };
});

// Cấu hình Swagger (có nút Authorize)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectAPIdemo", Version = "v1" });

    // Hiển thị chú thích /// trên Swagger
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"));

    // Định nghĩa khung nhập Token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập trực tiếp chuỗi Token thu được sau khi Login."
    });

    // Áp dụng khung nhập Token cho toàn bộ API
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Tự tạo database nếu chưa tồn tại + seed đủ 500 tài khoản mẫu
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    const int TotalUsers = 500;

    // Tải sẵn user hiện có để không phải query từng lần
    var existing = db.Users.ToDictionary(u => u.Username);

    void Upsert(string username, string email, string role, string password, DateTime createdAt)
    {
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        if (existing.TryGetValue(username, out var user))
        {
            user.Email = email;
            user.Role = role;
            user.PasswordSalt = salt;
            user.PasswordHash = hash;
        }
        else
        {
            var newUser = new User
            {
                Username = username,
                Email = email,
                Role = role,
                PasswordSalt = salt,
                PasswordHash = hash,
                CreatedAt = createdAt
            };
            db.Users.Add(newUser);
            existing[username] = newUser;
        }
    }

    Upsert("admin", "admin@example.com", "Admin", "Admin@123", DateTime.UtcNow);
    Upsert("hr01", "hr01@example.com", "HR", "123456", DateTime.UtcNow);

    var rnd = new Random(42);
    var roles = new[] { "User", "User", "User", "HR", "Admin" };
    for (int i = 1; i <= TotalUsers - 2; i++)
    {
        Upsert(
            $"user{i:D2}",
            $"user{i:D2}@example.com",
            roles[rnd.Next(roles.Length)],
            "123456",
            DateTime.UtcNow.AddDays(-rnd.Next(0, 365)));
    }

    db.SaveChanges();
}

// Logging ngoài cùng để ghi đúng status code sau khi ExceptionHandling đã set response lỗi
app.UseRequestLogging();
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Xác thực JWT trước khi kiểm tra quyền
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();