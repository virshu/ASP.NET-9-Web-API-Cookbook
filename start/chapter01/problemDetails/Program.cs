using System.Diagnostics;
using ProblemDetailsDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Bogus;
using ProblemDetailsDemo.Services;
using ProblemDetailsDemo.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

SqliteConnection connection = new("DataSource=:memory:");
connection.Open();

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options => 
    options.CustomizeProblemDetails = context =>
    {
        HttpContext httpContext = context.HttpContext;
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["supportContact"] = "support@rabinovich.org";

        if (context.ProblemDetails.Status == StatusCodes.Status401Unauthorized)
        {
            context.ProblemDetails.Title = "Unauthorized Access!";
            context.ProblemDetails.Detail = "You don't have permissions to do what you are trying to do.";
        }
        else
        {
            context.ProblemDetails.Title = "An unexpected error occurred";
            context.ProblemDetails.Detail = "An unexpected error occurred. Please try again later.";
        }
    });
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
    
builder.Services.AddScoped<IProductsService, ProductReadService>();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    
    if (!context.Products.Any())
    {
        Faker<Product>? productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Finance.Amount(50, 2000))
            .RuleFor(p => p.CategoryId, f => f.Random.Int(1, 5));
        context.Products.AddRange(productFaker.Generate(10000));
        context.SaveChanges();
    }
}

app.MapControllers();

app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "2.0");
    });
}

app.Run();
