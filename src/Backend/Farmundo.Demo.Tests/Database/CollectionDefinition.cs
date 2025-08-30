using Xunit;

namespace Farmundo.Demo.Tests.Database;

[CollectionDefinition("db")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
