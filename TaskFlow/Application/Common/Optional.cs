using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskFlow.Application.Common;

/// <summary>
/// Distinguishes "field not sent" from "field sent as null" in PATCH-like DTOs.
/// Note: unset Optional values are serialized as null. For proper PATCH semantics,
/// apply [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] to DTO properties
/// so unset fields are omitted from JSON requests.
/// </summary>
public readonly struct Optional<T>
{
    public bool HasValue { get; }
    public T? Value { get; }

    public Optional(T? value)
    {
        HasValue = true;
        Value = value;
    }

    public static Optional<T> Unset => default;

    public bool TryGetValue(out T? value)
    {
        value = Value;
        return HasValue;
    }

    public override bool Equals(object? obj) => obj is Optional<T> other && HasValue == other.HasValue && Equals(Value, other.Value);
    public override int GetHashCode() => HashCode.Combine(HasValue, Value);
}

public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new Optional<T>(value);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}

public class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var inner = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)Activator.CreateInstance(typeof(OptionalJsonConverter<>).MakeGenericType(inner))!;
    }
}
