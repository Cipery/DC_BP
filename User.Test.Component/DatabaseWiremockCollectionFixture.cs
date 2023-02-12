using User.Test.Component.Utils;
using User.Test.Unit;
using Xunit;

namespace User.Test.Component;

[CollectionDefinition(CollectionName)]
public class DatabaseWiremockCollectionFixture : ICollectionFixture<UserWebApplicationFactory>,
    IClassFixture<IszrApiServerFixture>
{
    public const string CollectionName = "DatabaseWiremockCollection";
}