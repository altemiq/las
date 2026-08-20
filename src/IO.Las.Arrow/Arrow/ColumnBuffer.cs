// -----------------------------------------------------------------------
// <copyright file="ColumnBuffer.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Arrow;

/// <summary>
/// The column buffer.
/// </summary>
internal static class ColumnBuffer
{
#pragma warning disable IDE0072
    /// <summary>
    /// Creates the column buffer.
    /// </summary>
    /// <param name="typeId">The type of items in the buffer.</param>
    /// <param name="capacity">The initial capacity.</param>
    /// <returns>The column buffer.</returns>
    /// <exception cref="NotSupportedException">The type in <paramref name="typeId"/> is not supported.</exception>
    public static IColumnBuffer Create(ArrowTypeId typeId, int capacity) =>
        typeId switch
        {
            ArrowTypeId.Boolean => new PrimitiveColumnBuffer<bool, BooleanArray, BooleanArray.Builder>(capacity),
            ArrowTypeId.UInt8 => new PrimitiveColumnBuffer<byte, UInt8Array, UInt8Array.Builder>(capacity),
            ArrowTypeId.Int8 => new PrimitiveColumnBuffer<sbyte, Int8Array, Int8Array.Builder>(capacity),
            ArrowTypeId.UInt16 => new PrimitiveColumnBuffer<ushort, UInt16Array, UInt16Array.Builder>(capacity),
            ArrowTypeId.Int16 => new PrimitiveColumnBuffer<short, Int16Array, Int16Array.Builder>(capacity),
            ArrowTypeId.UInt32 => new PrimitiveColumnBuffer<uint, UInt32Array, UInt32Array.Builder>(capacity),
            ArrowTypeId.Int32 => new PrimitiveColumnBuffer<int, Int32Array, Int32Array.Builder>(capacity),
            ArrowTypeId.UInt64 => new PrimitiveColumnBuffer<ulong, UInt64Array, UInt64Array.Builder>(capacity),
            ArrowTypeId.Int64 => new PrimitiveColumnBuffer<long, Int64Array, Int64Array.Builder>(capacity),
            ArrowTypeId.Float => new PrimitiveColumnBuffer<float, FloatArray, FloatArray.Builder>(capacity),
            ArrowTypeId.Double => new PrimitiveColumnBuffer<double, DoubleArray, DoubleArray.Builder>(capacity),
            _ => throw new NotSupportedException(),
        };
#pragma warning restore IDE0072

    private sealed class PrimitiveColumnBuffer<T, TArray, TBuilder>(int capacity) : IColumnBuffer
        where T : struct
        where TArray : IArrowArray
        where TBuilder : IArrowArrayBuilder<T, TArray, TBuilder>, new()
    {
        private readonly List<T> buffer = [with(capacity)];

        public int Count => this.buffer.Count;

        public void Add(object? value) => this.buffer.Add(value switch
        {
            T t => t,
            null => default,
            { } v when v == DBNull.Value => default,
            { } v => (T)Convert.ChangeType(v, typeof(T), System.Globalization.CultureInfo.InvariantCulture),
        });

        public void Clear() => this.buffer.Clear();

        public IArrowArray BuildArray()
        {
            var builder = new TBuilder();
            _ = builder.AppendRange(this.buffer);
            return builder.Build(allocator: default);
        }
    }
}