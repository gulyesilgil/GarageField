using Amazon.S3;
using GarageField.Data;
using GarageField.Services.CleanupServices;
using GarageField.Services.InspectionFileServices;
using GarageField.Services.InspectionServices;
using GarageField.Services.StorageServices;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



// ==============================
// 🔥 1. FILE SIZE LIMITS (CRITICAL)
// ==============================

// Kestrel (server)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

// IIS
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = long.MaxValue;
});

// Multipart form (EN KRİTİK)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});



// ==============================
// 🔥 2. DATABASE
// ==============================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



// ==============================
// 🔥 3. GARAGE (S3 CLIENT)
// ==============================

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var accessKey = config["GarageSettings:AccessKey"];
    var secretKey = config["GarageSettings:SecretKey"];
    var serviceUrl = config["GarageSettings:ServiceURL"];

    var s3Config = new AmazonS3Config
    {
        ServiceURL = serviceUrl,
        ForcePathStyle = true,          // Garage için ZORUNLU
        UseHttp = true,                 // local
        AuthenticationRegion = "garage" // garage.toml ile aynı
    };

    return new AmazonS3Client(accessKey, secretKey, s3Config);
});



// ==============================
// 🔥 4. SERVICES
// ==============================

builder.Services.AddScoped<IFileStorageService, GarageStorageService>();

builder.Services.AddScoped<InspectionService>();
builder.Services.AddScoped<InspectionFileService>();
builder.Services.AddScoped<StorageCleanupService>();
builder.Services.AddScoped<BucketService>();



// ==============================
// 🔥 5. WEB API
// ==============================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// ==============================
// 🔥 6. MIDDLEWARE
// ==============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();