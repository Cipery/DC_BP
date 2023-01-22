using Microsoft.EntityFrameworkCore;
using User.Clients;
using User.Middlewares;
using User.Repositories;
using User.Services;
using User.Services.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Services.AddDbContext<UserDbContext>(opt => opt.UseInMemoryDatabase("User"));
builder.Services.AddDbContext<UserDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("UserDb")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpClient();

// builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IIszrClient, IszrClient>();
builder.Services.Configure<IszrClientConfiguration>(builder.Configuration.GetSection("IszrClient"));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    dataContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthorization();

// app.MapControllers();

app.MapGet("/users/{id}", async (Guid id, IUserService userService) => Results.Ok(await userService.GetUser(id)));

app.MapPost("/user", async (CreateUserModel user, IUserService userService) =>
{
    var id = await userService.CreateUser(user);
    return Results.Created($"/user/{id}", null);
});

app.MapPut("/user/{id}", async (Guid id, UpdateUserModel updateUser, IUserService userService) =>
{
    if (id != updateUser.Id)
    {
        return Results.BadRequest();
    }
    
    await userService.UpdateUser(updateUser);
    return Results.NoContent();
});

app.MapDelete("/user/{id}", async (Guid id, IUserService userService) =>
{
    await userService.DeleteUser(id);
    return Results.Ok();
});

app.MapGet("/user/age", async (Guid id, IUserService userService) => Results.Ok(new GetUserAgeModel
{
    Age = await userService.GetUserAge(id)
}));

app.Run();