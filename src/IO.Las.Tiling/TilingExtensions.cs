// -----------------------------------------------------------------------
// <copyright file="TilingExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// <see cref="Tiling"/> extensions.
/// </summary>
public static class TilingExtensions
{
    /// <summary>
    /// Registers tiling VLRs.
    /// </summary>
    /// <param name="processor">The VLR processor.</param>
    public static void RegisterTiling(this VariableLengthRecordProcessor processor) => processor.Register(Tiling.LasToolsUserId, Tiling.TagRecordId, ProcessTiling);

    /// <summary>
    /// Registers tiling VLRs.
    /// </summary>
    /// <param name="processor">The VLR processor.</param>
    /// <returns><see langword="true" /> when the compression processors are successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processors, in which case nothing gets added.</returns>
    public static bool TryRegisterCompression(this VariableLengthRecordProcessor processor) => processor.TryRegister(Tiling.LasToolsUserId, Tiling.TagRecordId, ProcessTiling);

    private static Tiling ProcessTiling(VariableLengthRecordHeader header, ReadOnlySpan<byte> data) => new(header, data);
}