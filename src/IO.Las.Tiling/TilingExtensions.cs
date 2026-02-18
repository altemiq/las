// -----------------------------------------------------------------------
// <copyright file="TilingExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// <see cref="Tiling"/> extensions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "False positive")]
public static class TilingExtensions
{
    /// <summary>
    /// The <see cref="VariableLengthRecordProcessor"/> extensions.
    /// </summary>
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