// -----------------------------------------------------------------------
// <copyright file="TilingExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// <see cref="Tiling"/> extensions.
/// </summary>
public static class TilingExtensions
{
    /// <summary>
    /// The <see cref="VariableLengthRecordProcessor"/> extensions.
    /// </summary>
    /// <param name="processor">The processor.</param>
    extension(VariableLengthRecordProcessor processor)
    {
        /// <summary>
        /// Registers tiling VLRs.
        /// </summary>
        public void RegisterTiling() => processor.Register(Tiling.LasToolsUserId, Tiling.TagRecordId, ProcessTiling);

        /// <summary>
        /// Registers tiling VLRs.
        /// </summary>
        /// <returns><see langword="true" /> when the compression processors are successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processors, in which case nothing gets added.</returns>
        public bool TryRegisterCompression() => processor.TryRegister(Tiling.LasToolsUserId, Tiling.TagRecordId, ProcessTiling);
    }

    private static Tiling ProcessTiling(VariableLengthRecordHeader header, ReadOnlySpan<byte> data) => new(header, data);
}