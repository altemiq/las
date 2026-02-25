// -----------------------------------------------------------------------
// <copyright file="ColumnBuffer.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The column buffer.
/// </summary>
internal abstract class ColumnBuffer
{
    /// <summary>
    /// Gets the count.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Creates the column buffer.
    /// </summary>
    /// <param name="typeId">The type of items in the buffer.</param>
    /// <param name="capacity">The initial capacity.</param>
    /// <returns>The column buffer.</returns>
    /// <exception cref="NotSupportedException">The type in <paramref name="typeId"/> is not supported.</exception>
    public static ColumnBuffer Create(ArrowTypeId typeId, int capacity) =>
        typeId switch
        {
            ArrowTypeId.Boolean => new BooleanColumnBuffer(capacity),
            ArrowTypeId.UInt8 => new UInt8ColumnBuffer(capacity),
            ArrowTypeId.Int8 => new Int8ColumnBuffer(capacity),
            ArrowTypeId.UInt16 => new UInt16ColumnBuffer(capacity),
            ArrowTypeId.Int16 => new Int16ColumnBuffer(capacity),
            ArrowTypeId.UInt32 => new UInt32ColumnBuffer(capacity),
            ArrowTypeId.Int32 => new Int32ColumnBuffer(capacity),
            ArrowTypeId.UInt64 => new UInt64ColumnBuffer(capacity),
            ArrowTypeId.Int64 => new Int64ColumnBuffer(capacity),
            ArrowTypeId.Float => new SingleColumnBuffer(capacity),
            ArrowTypeId.Double => new DoubleColumnBuffer(capacity),
            _ => throw new NotSupportedException(),
        };

    /// <summary>
    /// Adds an item to the buffer.
    /// </summary>
    /// <param name="value">The value to add.</param>
    public abstract void Add(object? value);

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Build the <see cref="IArrowArray"/> from the buffer.
    /// </summary>
    /// <returns>The <see cref="IArrowArray"/>.</returns>
    public abstract IArrowArray BuildArray();

    private abstract class PrimitiveColumnBuffer<T, TArray, TBuilder>(int capacity) : ColumnBuffer, IEnumerable<T?>
        where T : struct
        where TArray : IArrowArray
        where TBuilder : class, IArrowArrayBuilder<TArray>
    {
        private readonly List<T?> buffer = new(capacity);

        public override int Count => this.buffer.Count;

        public override void Add(object? value)
        {
            if (value is T t)
            {
                this.buffer.Add(t);
            }
            else if (value is null || value == DBNull.Value)
            {
                this.buffer.Add(default);
            }
            else
            {
                // try to cast it
                this.buffer.Add((T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        public override void Clear() => this.buffer.Clear();

        public override IArrowArray BuildArray()
        {
            var builder = this.GetArrayBuilder();
            foreach (var v in this)
            {
                if (v.HasValue)
                {
                    builder.Append(v.Value);
                }
                else
                {
                    builder.AppendNull();
                }
            }

            return builder.Build(allocator: default);
        }

        public IEnumerator<T?> GetEnumerator() => this.buffer.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        protected abstract IArrowArrayBuilder<T, TArray, TBuilder> GetArrayBuilder();
    }

    private sealed class BooleanColumnBuffer(int capacity) : PrimitiveColumnBuffer<bool, BooleanArray, BooleanArray.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<bool, BooleanArray, BooleanArray.Builder> GetArrayBuilder() => new BooleanArray.Builder();
    }

    private sealed class UInt8ColumnBuffer(int capacity) : PrimitiveColumnBuffer<byte, UInt8Array, UInt8Array.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<byte, UInt8Array, UInt8Array.Builder> GetArrayBuilder() => new UInt8Array.Builder();
    }

    private sealed class Int8ColumnBuffer(int capacity) : PrimitiveColumnBuffer<sbyte, Int8Array, Int8Array.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<sbyte, Int8Array, Int8Array.Builder> GetArrayBuilder() => new Int8Array.Builder();
    }

    private sealed class UInt16ColumnBuffer(int capacity) : PrimitiveColumnBuffer<ushort, UInt16Array, UInt16Array.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<ushort, UInt16Array, UInt16Array.Builder> GetArrayBuilder() => new UInt16Array.Builder();
    }

    private sealed class Int16ColumnBuffer(int capacity) : PrimitiveColumnBuffer<short, Int16Array, Int16Array.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<short, Int16Array, Int16Array.Builder> GetArrayBuilder() => new Int16Array.Builder();
    }

    private sealed class UInt32ColumnBuffer(int capacity) : PrimitiveColumnBuffer<uint, UInt32Array, UInt32Array.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<uint, UInt32Array, UInt32Array.Builder> GetArrayBuilder() => new UInt32Array.Builder();
    }

    private sealed class Int32ColumnBuffer(int capacity) : PrimitiveColumnBuffer<int, Int32Array, Int32Array.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<int, Int32Array, Int32Array.Builder> GetArrayBuilder() => new Int32Array.Builder();
    }

    private sealed class UInt64ColumnBuffer(int capacity) : PrimitiveColumnBuffer<ulong, UInt64Array, UInt64Array.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<ulong, UInt64Array, UInt64Array.Builder> GetArrayBuilder() => new UInt64Array.Builder();
    }

    private sealed class Int64ColumnBuffer(int capacity) : PrimitiveColumnBuffer<long, Int64Array, Int64Array.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<long, Int64Array, Int64Array.Builder> GetArrayBuilder() => new Int64Array.Builder();
    }

    private sealed class SingleColumnBuffer(int capacity) : PrimitiveColumnBuffer<float, FloatArray, FloatArray.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<float, FloatArray, FloatArray.Builder> GetArrayBuilder() => new FloatArray.Builder();
    }

    private sealed class DoubleColumnBuffer(int capacity) : PrimitiveColumnBuffer<double, DoubleArray, DoubleArray.Builder>(capacity)
    {
        protected override IArrowArrayBuilder<double, DoubleArray, DoubleArray.Builder> GetArrayBuilder() => new DoubleArray.Builder();
    }
}