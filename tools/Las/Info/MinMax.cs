// -----------------------------------------------------------------------
// <copyright file="MinMax.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

using System.Runtime.Intrinsics;

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
        { } t when t == typeof(Vector128<int>) => new Vector128MinMax<int>(),
        { } t when t == typeof(Vector256<int>) => new Vector256MinMax<int>(),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:Use auto property", Justification = "This is for efficiency to not set via a property")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter", Justification = "This is for efficiency to not set via a property")]
    private sealed class Vector128MinMax<T> : IMinMax<Vector128<T>>, IMinMax
        where T : System.Numerics.IMinMaxValue<T>
    {
        private Vector128<T> minimum = Vector128.Create(T.MaxValue);
        private Vector128<T> maximum = Vector128.Create(T.MinValue);

        public Vector128<T> Minimum => this.minimum;

        public Vector128<T> Maximum => this.maximum;

        object IMinMax.Minimum => this.minimum;

        object IMinMax.Maximum => this.maximum;

        public void Update(Vector128<T> value) => this.UpdateCore(value);

        void IMinMax.Update(object value) => this.UpdateCore((Vector128<T>)value);

        public bool IsDefault() => Vector128.All(this.minimum, T.MaxValue) && Vector128.All(this.maximum, T.MinValue);

        bool IMinMax.IsDefault() => Vector128.All(this.minimum, T.MaxValue) && Vector128.All(this.maximum, T.MinValue);

        private void UpdateCore(Vector128<T> value)
        {
            this.minimum = Vector128.Min(this.minimum, value);
            this.maximum = Vector128.Min(this.maximum, value);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:Use auto property", Justification = "This is for efficiency to not set via a property")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter", Justification = "This is for efficiency to not set via a property")]
    private sealed class Vector256MinMax<T> : IMinMax<Vector256<T>>, IMinMax
        where T : System.Numerics.IMinMaxValue<T>
    {
        private Vector256<T> minimum = Vector256.Create(T.MaxValue);
        private Vector256<T> maximum = Vector256.Create(T.MinValue);

        public Vector256<T> Minimum => this.minimum;

        public Vector256<T> Maximum => this.maximum;

        object IMinMax.Minimum => this.minimum;

        object IMinMax.Maximum => this.maximum;

        public void Update(Vector256<T> value) => this.UpdateCore(value);

        void IMinMax.Update(object value) => this.UpdateCore((Vector256<T>)value);

        public bool IsDefault() => Vector256.All(this.minimum, T.MaxValue) && Vector256.All(this.maximum, T.MinValue);

        bool IMinMax.IsDefault() => Vector256.All(this.minimum, T.MaxValue) && Vector256.All(this.maximum, T.MinValue);

        private void UpdateCore(Vector256<T> value)
        {
            this.minimum = Vector256.Min(this.minimum, value);
            this.maximum = Vector256.Max(this.maximum, value);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:Use auto property", Justification = "This is for efficiency to not set via a property")]
    private sealed class ComparableMinMax<T>(T minValue, T maxValue) : IMinMax<T>, IMinMax
        where T : IComparable<T>
    {
        private readonly T defaultMinimum = maxValue;
        private readonly T defaultMaximum = minValue;

        private T minimum = maxValue;
        private T maximum = minValue;

        public T Minimum => this.minimum;

        public T Maximum => this.maximum;

        object IMinMax.Minimum => this.minimum;

        object IMinMax.Maximum => this.maximum;

        public void Update(T value) => this.UpdateCore(value);

        void IMinMax.Update(object value) => this.UpdateCore((T)value);

        public bool IsDefault() => this.minimum.Equals(this.defaultMinimum) && this.maximum.Equals(this.defaultMaximum);

        bool IMinMax.IsDefault() => this.minimum.Equals(this.defaultMinimum) && this.maximum.Equals(this.defaultMaximum);

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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:Use auto property", Justification = "This is for efficiency to not set via a property")]
    private sealed class GenericMinMax<T> : IMinMax<T>, IMinMax
        where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
    {
        private T minimum = T.MaxValue;
        private T maximum = T.MinValue;

        public T Minimum => this.minimum;

        public T Maximum => this.maximum;

        object IMinMax.Minimum => this.minimum;

        object IMinMax.Maximum => this.maximum;

        public void Update(T value) => this.UpdateCore(value);

        void IMinMax.Update(object value) => this.UpdateCore((T)value);

        public bool IsDefault() => this.minimum.Equals(T.MaxValue) && this.maximum.Equals(T.MinValue);

        bool IMinMax.IsDefault() => this.minimum.Equals(T.MaxValue) && this.maximum.Equals(T.MinValue);

        private void UpdateCore(T value)
        {
            this.minimum = T.Min(this.minimum, value);
            this.maximum = T.Max(this.maximum, value);
        }
    }
}