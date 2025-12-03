// -----------------------------------------------------------------------
// <copyright file="ILasReaderFormatter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The <see cref="LasReader"/> formatter.
/// </summary>
internal interface ILasReaderFormatter
{
    /// <summary>
    /// Appends the header.
    /// </summary>
    /// <param name="reader">The reader with the header to append.</param>
    /// <returns>The formatter.</returns>
    ILasReaderFormatter AppendHeader(LasReader reader);

    /// <summary>
    /// Appends the <see cref="VariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="reader">The reader with the VLRs to append.</param>
    /// <returns>The formatter.</returns>
    ILasReaderFormatter AppendVariableLengthRecords(LasReader reader);

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Appends the <see cref="ExtendedVariableLengthRecord"/> instances.
    /// </summary>
    /// <param name="reader">The reader with the EVLRs to append.</param>
    /// <returns>The formatter.</returns>
    ILasReaderFormatter AppendExtendedVariableLengthRecords(LasReader reader);
#endif

    /// <summary>
    /// Appends the statistics.
    /// </summary>
    /// <param name="reader">The reader with statistics to append.</param>
    /// <param name="statisticsFunc">The statistics to append.</param>
    /// <returns>The formatter.</returns>
    ILasReaderFormatter AppendStatistics(LasReader reader, Func<LasReader, Statistics> statisticsFunc);

    /// <summary>
    /// Appends the returns.
    /// </summary>
    /// <param name="reader">The reader with returns to append.</param>
    /// <param name="statisticsFunc">The statistics to append.</param>
    /// <returns>The formatter.</returns>
    ILasReaderFormatter AppendReturns(LasReader reader, Func<LasReader, Statistics> statisticsFunc);

    /// <summary>
    /// Appends the histograms.
    /// </summary>
    /// <param name="reader">The reader with histograms to append.</param>
    /// <param name="statisticsFunc">The statistics to append.</param>
    /// <returns>The formatter.</returns>
    ILasReaderFormatter AppendHistograms(LasReader reader, Func<LasReader, Statistics> statisticsFunc);
}