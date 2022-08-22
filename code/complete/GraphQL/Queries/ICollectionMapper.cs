using System;
using System.Collections;

namespace ConferencePlanner.GraphQL.Queries;

internal interface ICollectionMapper
{
    IEnumerable MapValues(IEnumerable fromList, Type toInstanceOfType);
}