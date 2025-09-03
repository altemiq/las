// -----------------------------------------------------------------------
// <copyright file="MinMax.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// Creation functions for <see cref="IMinMax{T}"/>.
/// </summary>
internal static class MinMax
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    /// <returns>The new instance.</returns>
    public static IMinMax<T> Create<T>() => (IMinMax<T>)Create(typeof(T));

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The new instance.</returns>
    public static IMinMax Create(ExtraBytesItem item) => item.HasScale
        ? new ComparableMinMax<double>(double.MinValue, double.MaxValue)
        : Create(item.DataType.ToType());
#endif

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The new instance.</returns>
    public static IMinMax Create(Type type) => type switch
    {
        { } t when t == typeof(int) => new GenericMinMax<int>(),
        { } t when t == typeof(short) => new GenericMinMax<short>(),
        { } t when t == typeof(ushort) => new GenericMinMax<ushort>(),
        { } t when t == typeof(uint) => new GenericMinMax<uint>(),
        { } t when t == typeof(ulong) => new GenericMinMax<ulong>(),
        { } t when t == typeof(double) => new GenericMinMax<double>(),
        { } t when t == typeof(float) => new GenericMinMax<float>(),
        { } t when t == typeof(byte) => new GenericMinMax<byte>(),
        { } t when t == typeof(sbyte) => new GenericMinMax<sbyte>(),
        { } t when t == typeof(DateTime) => new ComparableMinMax<DateTime>(DateTime.MinValue, DateTime.MaxValue),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:Use auto property", Justification = "This is for efficiency to not set via a property")]
    private class ComparableMinMax<T>(T minValue, T maxValue) : IMinMax<T>, IMinMax
        where T : IComparable<T>
    {
        private T minimum = maxValue;
        private T maximum = minValue;

        public T Minimum => this.minimum;

        public T Maximum => this.maximum;

        object IMinMax.Minimum => this.minimum;

        object IMinMax.Maximum => this.maximum;

        public void Update(T value) => this.UpdateCore(value);

        void IMinMax.Update(object value) => this.UpdateCore((T)value);

        private void UpdateCore(T value)
        {
            if (this.minimum.CompareTo(value) > 0)
            {
                this.minimum = value;
            }

            if (this.maximum.CompareTo(value) < 0)
            {
                this.maximum = value;
            }
        }
    }

    private sealed class GenericMinMax<T>() : ComparableMinMax<T>(T.MinValue, T.MaxValue)
        where T : IComparable<T>, System.Numerics.IMinMaxValue<T>;
}