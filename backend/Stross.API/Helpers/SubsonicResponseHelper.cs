using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Stross.Application.Shared.Helpers;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.SubsonicModels.JsonConverters;

namespace Stross.API.Helpers;

public static class SubsonicResponseHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter()
        },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        // https://github.com/dotnet/runtime/issues/108237
        TypeInfoResolverChain =
        {
            new DefaultJsonTypeInfoResolver().WithAddedModifier(ContractModifier_Collection)
        }
    };

    private static void ContractModifier_Collection(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (JsonPropertyInfo property in jsonTypeInfo.Properties)
        {
            if (property.PropertyType.IsValueType)
                continue;

            bool isCollection =
                property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                Array.Exists(
                    property.PropertyType.GetInterfaces(),
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));

            if (!isCollection)
                continue;

            // Check for WrappedArrayJsonConverter attribute
            WrappedArrayJsonConverterAttribute? wrappedArrayAttr = property.AttributeProvider?.GetCustomAttributes(typeof(WrappedArrayJsonConverterAttribute), false)
                .OfType<WrappedArrayJsonConverterAttribute>()
                .FirstOrDefault();

            if (wrappedArrayAttr != null)
            {
                // Apply the wrapped array converter
                Type elementType = property.PropertyType.GetGenericArguments()[0];
                Type converterType = typeof(WrappedArrayJsonConverterFactory).GetNestedType("WrappedArrayJsonConverterImpl`1", BindingFlags.Public)!
                    .MakeGenericType(elementType);
                property.CustomConverter = (JsonConverter?)Activator.CreateInstance(converterType, wrappedArrayAttr.ItemName);

                // Also set ShouldSerialize to only serialize when array has elements
                Type wrappedCollectionType = typeof(ICollection<>).MakeGenericType(elementType);
                ParameterExpression wrappedParam = Expression.Parameter(wrappedCollectionType, "value");
                MemberExpression wrappedCountProperty = Expression.Property(wrappedParam, "Count");
                LambdaExpression wrappedLambda = Expression.Lambda(
                    typeof(Func<,>).MakeGenericType(wrappedCollectionType, typeof(int)),
                    wrappedCountProperty,
                    wrappedParam);
                Delegate wrappedGetCount = wrappedLambda.Compile();
                property.ShouldSerialize = (_, value) => { return wrappedGetCount.DynamicInvoke(value) is int count && count > 0; };
                continue;
            }

            Type genericType = property.PropertyType.GetGenericArguments().FirstOrDefault() ?? property.PropertyType.GetElementType()!;
            Type collectionType = typeof(ICollection<>).MakeGenericType(genericType);

            ParameterExpression param = Expression.Parameter(collectionType, "value");
            MemberExpression countProperty = Expression.Property(param, "Count");
            LambdaExpression lambda = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(collectionType, typeof(int)),
                countProperty,
                param);
            Delegate getCount = lambda.Compile();
            property.ShouldSerialize = (_, value) => { return getCount.DynamicInvoke(value) is int count && count > 0; };
        }
    }

    internal static IResult CreateSubsonicResult(SubsonicBaseResponse response)
    {
        if (response.Format == SubsonicResponseFormat.Xml)
        {
            string xmlContent = XmlSerializationHelper.SerializeSubsonicResponse(response);

            return Results.Content(xmlContent, "application/xml", Encoding.UTF8);
        }

        return Results.Json(response, JsonOptions);
    }
}