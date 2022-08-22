using System.Collections.Generic;
using HotChocolate.Data;
using HotChocolate.Data.Neo4J;
using HotChocolate.Types;

namespace ConferencePlanner.GraphQL.Models;

public class Person
{
    public string Name { get; set; }

    [Neo4JRelationship("ACTED_IN")]
    [UseOffsetPaging]
    [UseFiltering]
    [UseSorting]
    public List<Movie> ActedIn { get; set; }
}