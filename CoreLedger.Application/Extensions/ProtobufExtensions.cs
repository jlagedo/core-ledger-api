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
}
