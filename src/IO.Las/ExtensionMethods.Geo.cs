// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.Geo.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// GeoTIFF <see cref="VariableLengthRecord"/> extension methods.
/// </summary>
public static partial class ExtensionMethods
{
    /// <summary>
    /// Gets a value indicating whether the specified variable length record is a <c>GeoTIFF</c> tag.
    /// </summary>
    /// <param name="record">The variable length record.</param>
    /// <returns><see langword="true"/> is <paramref name="record"/> is a <c>GeoTIFF</c> tag; otherwise <see langword="false"/>.</returns>
    public static bool IsGeoTiff(this VariableLengthRecord record) => record is GeoAsciiParamsTag or GeoDoubleParamsTag or GeoKeyDirectoryTag;

    /// <summary>
    /// Removes the <c>GeoTIFF</c> tags from the list.
    /// </summary>
    /// <param name="records">The list of variable length records.</param>
    public static void RemoveGeoTiff(this IList<VariableLengthRecord> records)
    {
        for (var i = records.Count - 1; i >= 0; i--)
        {
            if (records[i].IsGeoTiff())
            {
                records.RemoveAt(0);
            }
        }
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets a value indicating whether the specified variable length record is a <c>WKT</c> tag.
    /// </summary>
    /// <param name="record">The variable length record.</param>
    /// <returns><see langword="true"/> is <paramref name="record"/> is a <c>WKT</c> tag; otherwise <see langword="false"/>.</returns>
    public static bool IsWkt(this VariableLengthRecord record) => record is OgcCoordinateSystemWkt or OgcMathTransformWkt;

    /// <summary>
    /// Removes the <c>WKT</c> tags from the list.
    /// </summary>
    /// <param name="records">The list of variable length records.</param>
    public static void RemoveWkt(this IList<VariableLengthRecord> records)
    {
        for (var i = records.Count - 1; i >= 0; i--)
        {
            if (records[i].IsWkt())
            {
                records.RemoveAt(0);
            }
        }
    }
#endif

    /// <summary>
    /// Gets the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="key">The GEO key.</param>
    /// <returns>The ASCII value.</returns>
    public static string GetAsciiValue(this ILasReader reader, GeoKey key)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return GetAsciiValue(reader.VariableLengthRecords, key);
    }

    /// <summary>
    /// Gets the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="key">The GEO key.</param>
    /// <returns>The ASCII value.</returns>
    public static string GetAsciiValue(this IReadOnlyCollection<VariableLengthRecord> records, GeoKey key) => GetAsciiValue(records.OfType<GeoKeyDirectoryTag>().First(), key, records.OfType<GeoAsciiParamsTag>().First());

    /// <summary>
    /// Gets the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyDirectory">The key directory.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="asciiParams">The ASCII parameters.</param>
    /// <returns>The ASCII value.</returns>
    public static string GetAsciiValue(this GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoAsciiParamsTag asciiParams)
    {
        return (keyDirectory, asciiParams) switch
        {
            (null, _) => throw new ArgumentNullException(nameof(keyDirectory)),
            (_, null) => throw new ArgumentNullException(nameof(asciiParams)),
            _ => GetAsciiValueImpl(keyDirectory, key, asciiParams),
        };

        static string GetAsciiValueImpl(GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoAsciiParamsTag asciiParams)
        {
            return keyDirectory[key] switch
            {
                { TiffTagLocation: GeoAsciiParamsTag.TagRecordId } geoKeyEntry => asciiParams[geoKeyEntry].TrimEnd('|'),
                _ => ThrowIsNotAValue<string>(nameof(System.Text.Encoding.UTF8), nameof(key)),
            };
        }
    }

    /// <summary>
    /// Gets the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <returns>The ASCII value.</returns>
    public static string GetAsciiValue(this ILasReader reader, GeoKeyEntry keyEntry)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return GetAsciiValue(reader.VariableLengthRecords, keyEntry);
    }

    /// <summary>
    /// Gets the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <returns>The ASCII value.</returns>
    public static string GetAsciiValue(this IEnumerable<VariableLengthRecord> records, GeoKeyEntry keyEntry) => GetAsciiValue(keyEntry, records.OfType<GeoAsciiParamsTag>().First());

    /// <summary>
    /// Gets the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="asciiParams">The ASCII parameters.</param>
    /// <returns>The ASCII value.</returns>
    public static string GetAsciiValue(this GeoKeyEntry keyEntry, GeoAsciiParamsTag asciiParams) => (asciiParams, keyEntry) switch
    {
        (null, _) => throw new ArgumentNullException(nameof(asciiParams)),
        (not null, { TiffTagLocation: GeoAsciiParamsTag.TagRecordId }) => asciiParams[keyEntry].TrimEnd('|'),
        _ => ThrowIsNotAValue<string>(nameof(System.Text.Encoding.UTF8), nameof(keyEntry)),
    };

    /// <summary>
    /// Tries to get the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the string from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetAsciiValue(this ILasReader reader, GeoKey key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return TryGetAsciiValue(reader.VariableLengthRecords, key, out value);
    }

    /// <summary>
    /// Tries to get the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the string from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetAsciiValue(this IReadOnlyCollection<VariableLengthRecord> records, GeoKey key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => TryGetAsciiValue(records.OfType<GeoKeyDirectoryTag>().First(), key, records.OfType<GeoAsciiParamsTag>().First(), out value);

    /// <summary>
    /// Tries to get the ASCII value associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="keyDirectory">The key directory.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="asciiParams">The ASCII parameters.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the string from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetAsciiValue(this GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoAsciiParamsTag asciiParams, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
    {
        ArgumentNullException.ThrowIfNull(keyDirectory);
        ArgumentNullException.ThrowIfNull(asciiParams);

        if (keyDirectory[key] is { TiffTagLocation: GeoAsciiParamsTag.TagRecordId } geoKeyEntry)
        {
            value = asciiParams[geoKeyEntry].TrimEnd('|');
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the ASCII value associated with the <see cref="GeoKeyEntry"/> from the <see cref="ILasReader"/>.
    /// A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the string from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetAsciiValue(this ILasReader reader, GeoKeyEntry keyEntry, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return TryGetAsciiValue(reader.VariableLengthRecords, keyEntry, out value);
    }

    /// <summary>
    /// Tries to get the ASCII value associated with the <see cref="GeoKeyEntry"/> from the <see cref="VariableLengthRecord"/> instances.
    /// A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the string from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetAsciiValue(this IEnumerable<VariableLengthRecord> records, GeoKeyEntry keyEntry, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => TryGetAsciiValue(keyEntry, records.OfType<GeoAsciiParamsTag>().First(), out value);

    /// <summary>
    /// Tries to get the ASCII value associated with the <see cref="GeoKeyEntry"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="asciiParams">The ASCII parameters.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the string from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetAsciiValue(this GeoKeyEntry keyEntry, GeoAsciiParamsTag asciiParams, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
    {
        ArgumentNullException.ThrowIfNull(asciiParams);

        if (keyEntry is { TiffTagLocation: GeoAsciiParamsTag.TagRecordId })
        {
            value = asciiParams[keyEntry].TrimEnd('|');
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="key">The GEO key.</param>
    /// <returns>The double-precision value.</returns>
    public static double GetDoubleValue(this ILasReader reader, GeoKey key)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return GetDoubleValue(reader.VariableLengthRecords, key);
    }

    /// <summary>
    /// Gets the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="key">The GEO key.</param>
    /// <returns>The double-precision value.</returns>
    public static double GetDoubleValue(this IReadOnlyCollection<VariableLengthRecord> records, GeoKey key) => GetDoubleValue(records.OfType<GeoKeyDirectoryTag>().First(), key, records.OfType<GeoDoubleParamsTag>().First());

    /// <summary>
    /// Gets the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyDirectory">The key directory.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="doubleParams">The double-precision parameters.</param>
    /// <returns>The double-precision value.</returns>
    public static double GetDoubleValue(this GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoDoubleParamsTag doubleParams)
    {
        return (keyDirectory, doubleParams) switch
        {
            (null, _) => throw new ArgumentNullException(nameof(keyDirectory)),
            (_, null) => throw new ArgumentNullException(nameof(doubleParams)),
            _ => GetDoubleValueImpl(keyDirectory, key, doubleParams),
        };

        static double GetDoubleValueImpl(GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoDoubleParamsTag doubleParams)
        {
            return keyDirectory[key] switch
            {
                { TiffTagLocation: GeoDoubleParamsTag.TagRecordId } geoKeyEntry => doubleParams[geoKeyEntry.ValueOffset],
                _ => ThrowIsNotAValue<double>(nameof(Double), nameof(key)),
            };
        }
    }

    /// <summary>
    /// Gets the double-precision value associated with the <see cref="GeoKeyEntry"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <returns>The double-precision value.</returns>
    public static double GetDoubleValue(this ILasReader reader, GeoKeyEntry keyEntry)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return GetDoubleValue(reader.VariableLengthRecords, keyEntry);
    }

    /// <summary>
    /// Gets the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <returns>The double-precision value.</returns>
    public static double GetDoubleValue(this IEnumerable<VariableLengthRecord> records, GeoKeyEntry keyEntry) => GetDoubleValue(keyEntry, records.OfType<GeoDoubleParamsTag>().First());

    /// <summary>
    /// Gets the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="doubleParams">The double-precision parameters.</param>
    /// <returns>The double-precision value.</returns>
    public static double GetDoubleValue(this GeoKeyEntry keyEntry, GeoDoubleParamsTag doubleParams) => (doubleParams, keyEntry) switch
    {
        (null, _) => throw new ArgumentNullException(nameof(doubleParams)),
        (not null, { TiffTagLocation: GeoDoubleParamsTag.TagRecordId }) => doubleParams[keyEntry.ValueOffset],
        _ => ThrowIsNotAValue<double>(nameof(Double), nameof(keyEntry)),
    };

    /// <summary>
    /// Tries to get the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the double-precision floating-point number from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValue(this ILasReader reader, GeoKey key, out double value)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return TryGetDoubleValue(reader.VariableLengthRecords, key, out value);
    }

    /// <summary>
    /// Tries to get the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the double-precision floating-point number from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValue(this IReadOnlyCollection<VariableLengthRecord> records, GeoKey key, out double value) => TryGetDoubleValue(records.OfType<GeoKeyDirectoryTag>().First(), key, records.OfType<GeoDoubleParamsTag>().First(), out value);

    /// <summary>
    /// Tries to get the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyDirectory">The key directory.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="doubleParams">The double-precision parameters.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the double-precision floating-point number from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValue(this GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoDoubleParamsTag doubleParams, out double value)
    {
        ArgumentNullException.ThrowIfNull(keyDirectory);
        ArgumentNullException.ThrowIfNull(doubleParams);

        if (keyDirectory[key] is { TiffTagLocation: GeoDoubleParamsTag.TagRecordId } geoKeyEntry)
        {
            value = doubleParams[geoKeyEntry.ValueOffset];
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the double-precision floating-point number from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValue(this ILasReader reader, GeoKeyEntry keyEntry, out double value)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return TryGetDoubleValue(reader.VariableLengthRecords, keyEntry, out value);
    }

    /// <summary>
    /// Tries to get the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the double-precision floating-point number from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValue(this IEnumerable<VariableLengthRecord> records, GeoKeyEntry keyEntry, out double value) => TryGetDoubleValue(keyEntry, records.OfType<GeoDoubleParamsTag>().First(), out value);

    /// <summary>
    /// Tries to get the double-precision value associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="doubleParams">The double-precision parameters.</param>
    /// <param name="value">When this method returns and if the conversion succeeded, contains the double-precision floating-point number from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValue(this GeoKeyEntry keyEntry, GeoDoubleParamsTag doubleParams, out double value)
    {
        ArgumentNullException.ThrowIfNull(doubleParams);

        if (keyEntry is { TiffTagLocation: GeoDoubleParamsTag.TagRecordId })
        {
            value = doubleParams[keyEntry.ValueOffset];
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="key">The GEO key.</param>
    /// <returns>The double-precision values.</returns>
    public static IEnumerable<double> GetDoubleValues(this ILasReader reader, GeoKey key)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return GetDoubleValues(reader.VariableLengthRecords, key);
    }

    /// <summary>
    /// Gets the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="key">The GEO key.</param>
    /// <returns>The double-precision values.</returns>
    public static IEnumerable<double> GetDoubleValues(this IReadOnlyCollection<VariableLengthRecord> records, GeoKey key) => GetDoubleValues(records.OfType<GeoKeyDirectoryTag>().First(), key, records.OfType<GeoDoubleParamsTag>().First());

    /// <summary>
    /// Gets the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyDirectory">The key directory.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="doubleParams">The double-precision parameters.</param>
    /// <returns>The double-precision values.</returns>
    public static IEnumerable<double> GetDoubleValues(this GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoDoubleParamsTag doubleParams)
    {
        return (keyDirectory, doubleParams) switch
        {
            (null, _) => throw new ArgumentNullException(nameof(keyDirectory)),
            (_, null) => throw new ArgumentNullException(nameof(doubleParams)),
            _ => GetDoubleValuesImpl(keyDirectory, key, doubleParams),
        };

        static IEnumerable<double> GetDoubleValuesImpl(GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoDoubleParamsTag doubleParams)
        {
            return keyDirectory[key] switch
            {
                { TiffTagLocation: GeoDoubleParamsTag.TagRecordId } geoKeyEntry => doubleParams.GetValues(geoKeyEntry.ValueOffset, geoKeyEntry.Count),
                _ => ThrowIsNotAValue<IEnumerable<double>>(nameof(IEnumerable<>), nameof(key)),
            };
        }
    }

    /// <summary>
    /// Gets the double-precision values associated with the <see cref="GeoKeyEntry"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <returns>The double-precision values.</returns>
    public static IEnumerable<double> GetDoubleValues(this ILasReader reader, GeoKeyEntry keyEntry)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return GetDoubleValues(reader.VariableLengthRecords, keyEntry);
    }

    /// <summary>
    /// Gets the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <returns>The double-precision values.</returns>
    public static IEnumerable<double> GetDoubleValues(this IEnumerable<VariableLengthRecord> records, GeoKeyEntry keyEntry) => GetDoubleValues(keyEntry, records.OfType<GeoDoubleParamsTag>().First());

    /// <summary>
    /// Gets the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="doubleParams">The double-precision parameters.</param>
    /// <returns>The double-precision values.</returns>
    public static IEnumerable<double> GetDoubleValues(this GeoKeyEntry keyEntry, GeoDoubleParamsTag doubleParams) => (doubleParams, keyEntry) switch
    {
        (null, _) => throw new ArgumentNullException(nameof(doubleParams)),
        (not null, { TiffTagLocation: GeoDoubleParamsTag.TagRecordId }) => doubleParams.GetValues(keyEntry.ValueOffset, keyEntry.Count),
        _ => ThrowIsNotAValue<IEnumerable<double>>(nameof(IEnumerable<>), nameof(keyEntry)),
    };

    /// <summary>
    /// Tries to get the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="values">When this method returns and if the conversion succeeded, contains the double-precision floating-point numbers from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValues(this ILasReader reader, GeoKey key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IEnumerable<double>? values)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return TryGetDoubleValues(reader.VariableLengthRecords, key, out values);
    }

    /// <summary>
    /// Tries to get the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="values">When this method returns and if the conversion succeeded, contains the double-precision floating-point numbers from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValues(this IReadOnlyCollection<VariableLengthRecord> records, GeoKey key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IEnumerable<double>? values) => TryGetDoubleValues(records.OfType<GeoKeyDirectoryTag>().First(), key, records.OfType<GeoDoubleParamsTag>().First(), out values);

    /// <summary>
    /// Tries to get the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyDirectory">The key directory.</param>
    /// <param name="key">The GEO key.</param>
    /// <param name="doubleParams">The double-precision parameters.</param>
    /// <param name="values">When this method returns and if the conversion succeeded, contains the double-precision floating-point numbers from the <paramref name="key"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValues(this GeoKeyDirectoryTag keyDirectory, GeoKey key, GeoDoubleParamsTag doubleParams, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IEnumerable<double>? values)
    {
        ArgumentNullException.ThrowIfNull(keyDirectory);
        ArgumentNullException.ThrowIfNull(doubleParams);

        if (keyDirectory[key] is { TiffTagLocation: GeoDoubleParamsTag.TagRecordId } geoKeyEntry)
        {
            values = doubleParams.GetValues(geoKeyEntry.ValueOffset, geoKeyEntry.Count);
            return true;
        }

        values = default;
        return false;
    }

    /// <summary>
    /// Tries to get the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="ILasReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="values">When this method returns and if the conversion succeeded, contains the double-precision floating-point numbers from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValues(this ILasReader reader, GeoKeyEntry keyEntry, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IEnumerable<double>? values)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return TryGetDoubleValues(reader.VariableLengthRecords, keyEntry, out values);
    }

    /// <summary>
    /// Tries to get the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="values">When this method returns and if the conversion succeeded, contains the double-precision floating-point numbers from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValues(this IEnumerable<VariableLengthRecord> records, GeoKeyEntry keyEntry, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IEnumerable<double>? values) => TryGetDoubleValues(keyEntry, records.OfType<GeoDoubleParamsTag>().First(), out values);

    /// <summary>
    /// Tries to get the double-precision values associated with the <see cref="GeoKey"/> from the <see cref="GeoKeyDirectoryTag"/> and <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="keyEntry">The GEO key entry.</param>
    /// <param name="doubleParams">The double-precision parameters.</param>
    /// <param name="values">When this method returns and if the conversion succeeded, contains the double-precision floating-point numbers from the <paramref name="keyEntry"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keyEntry"/> was successfully extracted; otherwise <see langword="false"/>.</returns>
    public static bool TryGetDoubleValues(this GeoKeyEntry keyEntry, GeoDoubleParamsTag doubleParams, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IEnumerable<double>? values)
    {
        ArgumentNullException.ThrowIfNull(doubleParams);

        if (keyEntry is { TiffTagLocation: GeoDoubleParamsTag.TagRecordId })
        {
            values = doubleParams.GetValues(keyEntry.ValueOffset, keyEntry.Count);
            return true;
        }

        values = default;
        return false;
    }

    private static double[] GetValues(this GeoDoubleParamsTag doubleParams, int startIndex, int count)
    {
        var values = new double[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = doubleParams[startIndex + i];
        }

        return values;
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static T ThrowIsNotAValue<T>(string expected, string paramName) => throw new ArgumentException(string.Format(Properties.Resources.Culture, Properties.Resources.IsNotAValue, nameof(GeoKeyEntry), expected), paramName);
}