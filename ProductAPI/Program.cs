using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using ProductAPI.Models.Constants;
using Products.Data.DBContext;
using Polly;
using ProductAPI.Services.Interfaces;
using ProductAPI.Services;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddPooledDbContextFactory<ProductDBContext>(
    o => o.UseSqlServer(builder.Configuration.GetConnectionString("ProductInfoDatabase")));

builder.Services.AddHttpClient(HttpClientConstants.ColourAPI, c => c.BaseAddress = new Uri(HttpClientConstants.ColourAPIURI))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(2, TimeSpan.FromMinutes(2)));
builder.Services.AddHttpClient(HttpClientConstants.SizeAPI, c => c.BaseAddress = new Uri(HttpClientConstants.SizeAPIURI))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(2, TimeSpan.FromMinutes(2)));

builder.Services.AddAzureClients(builder =>
{
    builder.AddClient<QueueClient, QueueClientOptions>((options, _, _) =>
    {
        options.MessageEncoding = QueueMessageEncoding.Base64;
        var credential = new DefaultAzureCredential();
        var queueUri = new Uri("https://productcreated.queue.core.windows.net/productcreatedevent");
        return new QueueClient(queueUri, credential, options);
    });
});

builder.Services.AddMemoryCache();

builder.Services.AddTransient<IProductsService, ProductsService>();
builder.Services.AddTransient<IQueueService, QueueService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
public partial class Program { }