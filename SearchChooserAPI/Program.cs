using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Scalar.AspNetCore;
using SearchChooserAPI.Data;
using SearchChooserAPI.Models.Res;
using SearchChooserAPI.Services;
using SearchChooserAPI.Services.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var odataBuilder = new ODataConventionModelBuilder();
var doctorsEntity = odataBuilder.EntitySet<DoctorSearchResponse>("DoctorsOData").EntityType;
doctorsEntity.HasKey(d => d.DoctorId);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault;
    })
    .AddOData(options => options.Select().Filter().OrderBy().Count().SetMaxTop(null)
        .AddRouteComponents("odata", odataBuilder.GetEdmModel()));

// Configure SQL Server
builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDoctorService, DoctorService>();

// AI Query Service
builder.Services.Configure<LmStudioOptions>(builder.Configuration.GetSection("LmStudio"));
builder.Services.AddHttpClient<ILmStudioService, LmStudioService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<LmStudioOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
        client.Timeout = TimeSpan.FromMinutes(3);
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-migrate/create and seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SearchDbContext>();
        DataSeeder.SeedData(context);
        Console.WriteLine("==================================================");
        Console.WriteLine("Database created and seeded successfully in SQL Server!");
        Console.WriteLine("==================================================");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"An error occurred seeding the database: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
        Console.ResetColor();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Dynamic Search Chooser API Reference")
                .WithTheme(ScalarTheme.Mars)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Search Chooser API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
