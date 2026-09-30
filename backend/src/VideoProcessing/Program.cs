using VideoProcessing.Api;
using VideoProcessing.Contracts.Internal;
using VideoProcessing.Messaging;
using VideoProcessing.Processing;
using VideoProcessing.Storage;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
builder.Services.Configure<FFmpegOptions>(builder.Configuration.GetSection(FFmpegOptions.SectionName));

// Cliente da Education API (start/complete/fail), com retry automático para falhas transitórias de rede.
var educationApi = builder.Configuration.GetSection(EducationApiOptions.SectionName).Get<EducationApiOptions>()
    ?? new EducationApiOptions();
var internalApi = builder.Configuration.GetSection(InternalApiOptions.SectionName).Get<InternalApiOptions>()
    ?? new InternalApiOptions();
internalApi.Validate();

builder.Services.AddHttpClient<IEducationApiClient, EducationApiClient>(client =>
    {
        client.BaseAddress = new Uri(educationApi.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Add(InternalApi.KeyHeader, internalApi.Key);
    })
    .AddStandardResilienceHandler();

builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
builder.Services.AddSingleton<IFFmpegService, FFmpegService>();
builder.Services.AddScoped<VideoProcessor>();

builder.Services.AddHostedService<VideoUploadedConsumer>();

var host = builder.Build();
host.Run();
