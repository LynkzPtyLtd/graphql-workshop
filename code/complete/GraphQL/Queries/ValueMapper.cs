using System;
using System.Collections;
using System.Collections.Generic;
using Neo4j.Driver;
using ServiceStack;

namespace ConferencePlanner.GraphQL.Queries;

internal static class ValueMapper
{
    public static T MapValue<T>(object cypherValue)
    {
        var targetType = typeof(T);

        if (typeof(IEnumerable).IsAssignableFrom(targetType))
        {
            if (cypherValue is not IEnumerable enumerable) throw new InvalidOperationException();

            if (targetType == typeof(string)) return enumerable.As<T>();

            var elementType = targetType.GetGenericArguments()[0];
            var genericType = typeof(CollectionMapper<>).MakeGenericType(elementType);
            var collectionMapper = (ICollectionMapper)genericType.CreateInstance();

            return (T)collectionMapper.MapValues(enumerable, targetType);
        }

        return cypherValue switch
        {
            INode node => node.Properties.FromObjectDictionary<T>(),
            IRelationship relationship => relationship.Properties.FromObjectDictionary<T>(),
            IReadOnlyDictionary<string, object> map => map.FromObjectDictionary<T>(),
            IEnumerable =>
                throw new NotImplementedException(),
            _ => cypherValue.As<T>()
        };
    }
}