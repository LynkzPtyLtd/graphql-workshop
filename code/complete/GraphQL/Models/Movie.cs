using System.Collections.Generic;
using HotChocolate.Data;
using HotChocolate.Data.Neo4J;
using HotChocolate.Types;

namespace ConferencePlanner.GraphQL.Models;

public class Movie
{
    public string Title { get; set; }

    [Neo4JRelationship("ACTED_IN", RelationshipDirection.Incoming)]
    [UseOffsetPaging]
    [UseFiltering]
    [UseSorting]
    public List<Person> Actors { get; set; }

    [Neo4JRelationship("PRODUCED", RelationshipDirection.Incoming)]
    [UseOffsetPaging]
    [UseFiltering]
    [UseSorting]
    public List<Person> Producers { get; set; }
}