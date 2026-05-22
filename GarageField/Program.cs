using Amazon.S3;
using GarageField.Data;
using GarageField.Repositories.Implementations;
using GarageField.Repositories.Interfaces;
using GarageField.Services.InspectionFileServices;
using GarageField.Services.InspectionServices;
using GarageField.Services.Storage;
using GarageField.Services.StorageServices;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var accessKey = config["GarageSettings:AccessKey"];
    var secretKey = config["GarageSettings:SecretKey"];
    var serviceUrl = config["GarageSettings:ServiceURL"];

    var s3Config = new AmazonS3Config
    {
        ServiceURL = serviceUrl,
        ForcePathStyle = true,
        UseHttp = true,
        AuthenticationRegion = "garage"
    };

    return new AmazonS3Client(accessKey, secretKey, s3Config);
});

builder.Services.AddScoped<IFileStorageService, GarageStorageService>();
builder.Services.AddScoped<InspectionService>();
builder.Services.AddScoped<InspectionFileService>();
builder.Services.AddScoped<BucketService>();

builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddScoped<IInspectionFileRepository, InspectionFileRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();