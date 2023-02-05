using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using User.Repositories;
using Xunit.Abstractions;

namespace User.Test.Component.Utils;

// Zdroje:
// https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#customize-webapplicationfactory
// https://www.meziantou.net/how-to-get-asp-net-core-logs-in-the-output-of-xunit-tests.htm
public class UserWebApplicationFactory : WebApplicationFactory<Program>
{
    private ITestOutputHelper _testOutputHelper;

    public UserWebApplicationFactory WithTestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureLogging(loggingBuilder =>
        {
            loggingBuilder.AddXUnit(_testOutputHelper);
        });
        
        builder.ConfigureServices(services =>
        {
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetService<UserDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();
        });
    }
    
}