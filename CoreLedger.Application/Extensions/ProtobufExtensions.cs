using ProtoBuf;

namespace CoreLedger.Application.Extensions;

/// <summary>
///     Extension methods for Protobuf serialization operations.
/// </summary>
public static class ProtobufExtensions
{
    /// <summary>
    ///     Serializes a message to Protobuf binary format.
    /// </summary>
    /// <typeparam name="T">The type of the message to serialize.</typeparam>
    /// <param name="message">The message instance to serialize.</param>
    /// <returns>Byte array containing the serialized message in Protobuf format.</returns>
    public static byte[] SerializeToProtobuf<T>(this T message)
    {
        using var memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, message);
        return memoryStream.ToArray();
    }

    /// <summary>
    ///     Deserializes a Protobuf binary payload to a strongly-typed message.
    /// </summary>
    /// <typeparam name="T">The type of the message to deserialize.</typeparam>
    /// <param name="payload">Byte array containing the Protobuf-serialized message.</param>
    /// <returns>Deserialized message instance.</returns>
    /// <exception cref="ArgumentException">Thrown when payload is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
    public static T DeserializeFromProtobuf<T>(this byte[] payload)
    {
        if (payload == null || payload.Length == 0)
            throw new ArgumentException("Payload cannot be null or empty.", nameof(payload));

        try
        {
            using var memoryStream = new MemoryStream(payload);
            return Serializer.Deserialize<T>(memoryStream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize Protobuf payload to type {typeof(T).Name}. " +
                $"Payload size: {payload.Length} bytes.", ex);
        }
    }
}
