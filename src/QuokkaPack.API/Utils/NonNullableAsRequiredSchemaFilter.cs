using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
namespace QuokkaPack.API.Utils
{
    public class NonNullableAsRequiredSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null || context?.Type == null) return;

            var props = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var (jsonName, _) in schema.Properties)
            {
                var propInfo = props.FirstOrDefault(p =>
                    string.Equals(ToCamel(p.Name), jsonName, StringComparison.Ordinal));

                if (propInfo == null) continue;

                var t = propInfo.PropertyType;
                var isNonNullableValueType = t.IsValueType && Nullable.GetUnderlyingType(t) == null;
                var isRefType = !t.IsValueType;
                var isNullableRef = IsNullableReference(propInfo);

                if (isNonNullableValueType || (isRefType && !isNullableRef))
                {
                    if (!schema.Required.Contains(jsonName))
                        schema.Required.Add(jsonName);
                }
            }
        }

        private static string ToCamel(string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];

        // Uses compiler metadata from Nullable Reference Types to detect string? vs string
        private static bool IsNullableReference(PropertyInfo prop)
        {
            var nullableAttr = prop.GetCustomAttribute<System.Runtime.CompilerServices.NullableAttribute>();
            if (nullableAttr?.NullableFlags?.Length > 0) return nullableAttr.NullableFlags[0] == 2;

            var ctx = prop.DeclaringType?.GetCustomAttribute<System.Runtime.CompilerServices.NullableContextAttribute>();
            return ctx?.Flag == 2; // 2 => annotated nullable
        }
    }

}
