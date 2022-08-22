using System;
using ConferencePlanner.GraphQL.Data;
using ConferencePlanner.GraphQL.Logging;
using HotChocolate.Execution.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace ConferencePlanner.GraphQL.Imports;

public static class ImportRequestExecutorBuilderExtensions
{
    internal static readonly Lazy<IDriver> Neo4JDriver = new(
        () => GraphDatabase.Driver("bolt://127.0.0.100:7687", AuthTokens.Basic("neo4j", "Zi8umW2STmzdKB"), cfg =>
        {
            var factory = LoggerFactory.Create(logCfg =>
            {
                logCfg.SetMinimumLevel(LogLevel.Debug);
                logCfg.AddConsole();
            });

            cfg.WithLogger(new DriverLogger(factory.CreateLogger("neo4j")));
        }));
    
    public static IRequestExecutorBuilder EnsureRelationalDatabaseIsCreated(
        this IRequestExecutorBuilder builder) =>
        builder.ConfigureSchemaAsync(async (services, _, ct) =>
        {
            var factory =
                services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            await using var dbContext = await factory.CreateDbContextAsync(ct);

            if (await dbContext.Database.EnsureCreatedAsync(ct))
            {
                var importer = new DataImporter();
                await importer.LoadDataAsync(dbContext);
            }
        });
    
    public static IRequestExecutorBuilder EnsureGraphDatabaseIsCreated(
        this IRequestExecutorBuilder builder) =>
        builder.ConfigureSchemaAsync(async (services, _, ct) =>
        {
            const string seedCypher = @"
        CREATE (TheMatrix:Movie {Title:'The Matrix', Released:1999, Tagline:'Welcome to the Real World'})
        CREATE (Keanu:Person {Name:'Keanu Reeves', Born:1964})
        CREATE (Carrie:Person {Name:'Carrie-Anne Moss', Born:1967})
        CREATE (Laurence:Person {Name:'Laurence Fishburne', Born:1961})
        CREATE (Hugo:Person {Name:'Hugo Weaving', Born:1960})
        CREATE (LillyW:Person {Name:'Lilly Wachowski', Born:1967})
        CREATE (LanaW:Person {Name:'Lana Wachowski', Born:1965})
        CREATE (JoelS:Person {Name:'Joel Silver', Born:1952})
        CREATE
          (Keanu)-[:ACTED_IN {Roles:['Neo']}]->(TheMatrix),
          (Carrie)-[:ACTED_IN {Roles:['Trinity']}]->(TheMatrix),
          (Laurence)-[:ACTED_IN {Roles:['Morpheus']}]->(TheMatrix),
          (Hugo)-[:ACTED_IN {Roles:['Agent Smith']}]->(TheMatrix),
          (LillyW)-[:DIRECTED]->(TheMatrix),
          (LanaW)-[:DIRECTED]->(TheMatrix),
          (JoelS)-[:PRODUCED]->(TheMatrix)
        CREATE (Emil:Person {Name: 'Emil Eifrem', Born:1978})
        CREATE (Emil)-[:ACTED_IN {Roles:['Emil']}]->(TheMatrix)
        CREATE (TheMatrixReloaded:Movie {Title:'The Matrix Reloaded', Released:2003, Tagline:'Free your mind'})
        CREATE
          (Keanu)-[:ACTED_IN {Roles:['Neo']}]->(TheMatrixReloaded),
          (Carrie)-[:ACTED_IN {Roles:['Trinity']}]->(TheMatrixReloaded),
          (Laurence)-[:ACTED_IN {Roles:['Morpheus']}]->(TheMatrixReloaded),
          (Hugo)-[:ACTED_IN {Roles:['Agent Smith']}]->(TheMatrixReloaded),
          (LillyW)-[:DIRECTED]->(TheMatrixReloaded),
          (LanaW)-[:DIRECTED]->(TheMatrixReloaded),
          (JoelS)-[:PRODUCED]->(TheMatrixReloaded)
        CREATE (TheMatrixRevolutions:Movie {Title:'The Matrix Revolutions', Released:2003, Tagline:'Everything that has a beginning has an end'})
        CREATE
          (Keanu)-[:ACTED_IN {Roles:['Neo']}]->(TheMatrixRevolutions),
          (Carrie)-[:ACTED_IN {Roles:['Trinity']}]->(TheMatrixRevolutions),
          (Laurence)-[:ACTED_IN {Roles:['Morpheus']}]->(TheMatrixRevolutions),
          (Hugo)-[:ACTED_IN {Roles:['Agent Smith']}]->(TheMatrixRevolutions),
          (LillyW)-[:DIRECTED]->(TheMatrixRevolutions),
          (LanaW)-[:DIRECTED]->(TheMatrixRevolutions),
          (JoelS)-[:PRODUCED]->(TheMatrixRevolutions)";

            var neo4JDriver = Neo4JDriver.Value;
            var session = neo4JDriver.AsyncSession();
            await session.RunAsync("MATCH (n) DETACH DELETE n");
            var cursor = await session.RunAsync(seedCypher);
            await cursor.ConsumeAsync();
        });
}