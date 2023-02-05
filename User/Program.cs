using Microsoft.EntityFrameworkCore;
using User.Clients;
using User.Middlewares;
using User.Repositories;
using User.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Services.AddDbContext<UserDbContext>(opt => opt.UseInMemoryDatabase("User"));
builder.Services.AddDbContext<UserDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("UserDb")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IClockService, ClockService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IIszrClient, IszrClient>();
builder.Services.Configure<IszrClientConfiguration>(builder.Configuration.GetSection("IszrClient"));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

#if DEBUG
// Migrace by neměly běžet při spuštění aplikace. Lepší je např. migrace v CI/CD, či přes ruční aplikace SQL skriptů
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    dataContext.Database.Migrate();
}
#endif

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }