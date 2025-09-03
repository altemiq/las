// -----------------------------------------------------------------------
// <copyright file="LasFormatProvider.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The LAS format provider.
/// </summary>
/// <param name="baseFormatProvider">The base format provider.</param>
/// <param name="reader">The reader.</param>
internal sealed class LasFormatProvider(IFormatProvider? baseFormatProvider, ILasReader reader) : IFormatProvider, ICustomFormatter
{
    /// <inheritdoc/>
    object? IFormatProvider.GetFormat(Type? formatType) => formatType == typeof(ICustomFormatter)
        ? this
        : baseFormatProvider?.GetFormat(formatType);

    /// <inheritdoc/>
    string ICustomFormatter.Format(string? format, object? arg, IFormatProvider? formatProvider) => arg switch
    {
        GeoKeyEntry { TiffTagLocation: GeoDoubleParamsTag.TagRecordId } keyEntry => reader.GetDoubleValue(keyEntry).ToString(format, formatProvider),
        GeoKeyEntry { TiffTagLocation: GeoAsciiParamsTag.TagRecordId } keyEntry => reader.GetAsciiValue(keyEntry),
        IFormattable formattable => formattable.ToString(format, formatProvider),
#if NETFRAMEWORK || NETCOREAPP || NETSTANDARD1_3_OR_GREATER
        IConvertible convertible => convertible.ToString(formatProvider),
#endif
        not null when arg.ToString() is { } argString => argString,
        _ => throw new ArgumentException("Failed to format string", nameof(arg)),
    };
}