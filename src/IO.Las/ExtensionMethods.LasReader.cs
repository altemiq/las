// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.LasReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable S2325, SA1101

/// <content>
/// The <see cref="ILasReader"/> extensions.
/// </content>
public static partial class ExtensionMethods
{
    /// <summary>
    /// The <see cref="ILasReader"/> extensions.
    /// </summary>
    extension(ILasReader reader)
    {
#if LAS1_4_OR_GREATER
        /// <summary>
        /// Copies the contents the current reader to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="vlrPredicate">The <see cref="VariableLengthRecord"/> predicate.</param>
        /// <param name="evlrPredicate">The <see cref="ExtendedVariableLengthRecord"/> predicate.</param>
        public void CopyTo(ILasWriter writer, Func<VariableLengthRecord, bool>? vlrPredicate = default, Func<ExtendedVariableLengthRecord, bool>? evlrPredicate = default)
        {
            IEnumerable<VariableLengthRecord> variableLengthRecords = reader.VariableLengthRecords;
            if (vlrPredicate is not null)
            {
                variableLengthRecords = variableLengthRecords.Where(vlrPredicate);
            }

            writer.Write(reader.Header, variableLengthRecords);

            while (reader.ReadPointDataRecord() is { PointDataRecord: not null } pointDataRecord)
            {
                // change the point data record if required
                writer.Write(pointDataRecord.PointDataRecord, pointDataRecord.ExtraBytes);
            }

            if (writer is LasWriter flushable)
            {
                flushable.Flush();
            }

            IEnumerable<ExtendedVariableLengthRecord> extendedVariableLengthRecords = reader.ExtendedVariableLengthRecords;
            if (evlrPredicate is not null)
            {
                extendedVariableLengthRecords = extendedVariableLengthRecords.Where(evlrPredicate);
            }

            foreach (var extendedVariableLengthRecord in extendedVariableLengthRecords)
            {
                writer.Write(extendedVariableLengthRecord);
            }
        }

        /// <summary>
        /// Copies the contents the current reader to the specified writer asynchronously.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="vlrPredicate">The <see cref="VariableLengthRecord"/> predicate.</param>
        /// <param name="evlrPredicate">The <see cref="ExtendedVariableLengthRecord"/> predicate.</param>
        /// <param name="cancellationToken">The token for cancelling the task.</param>
        /// <returns>The asynchronous task for copying the contents to <paramref name="writer"/>.</returns>
        public async Task CopyToAsync(ILasWriter writer, Func<VariableLengthRecord, bool>? vlrPredicate = default, Func<ExtendedVariableLengthRecord, bool>? evlrPredicate = default, CancellationToken cancellationToken = default)
        {
            IEnumerable<VariableLengthRecord> variableLengthRecords = reader.VariableLengthRecords;
            if (vlrPredicate is not null)
            {
                variableLengthRecords = variableLengthRecords.Where(vlrPredicate);
            }

            writer.Write(reader.Header, variableLengthRecords);

            while (await reader.ReadPointDataRecordAsync(cancellationToken).ConfigureAwait(false) is { PointDataRecord: not null } pointDataRecord)
            {
                await writer.WriteAsync(pointDataRecord.PointDataRecord, pointDataRecord.ExtraBytes, cancellationToken).ConfigureAwait(false);
            }

            if (writer is LasWriter flushable)
            {
                flushable.Flush();
            }

            IEnumerable<ExtendedVariableLengthRecord> extendedVariableLengthRecords = reader.ExtendedVariableLengthRecords;
            if (evlrPredicate is not null)
            {
                extendedVariableLengthRecords = extendedVariableLengthRecords.Where(evlrPredicate);
            }

            foreach (var extendedVariableLengthRecord in extendedVariableLengthRecords)
            {
                writer.Write(extendedVariableLengthRecord);
            }
        }
#else
        /// <summary>
        /// Copies the contents the current reader to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="vlrPredicate">The <see cref="VariableLengthRecord"/> predicate.</param>
        public void CopyTo(ILasWriter writer, Func<VariableLengthRecord, bool>? vlrPredicate = default)
        {
            IEnumerable<VariableLengthRecord> vlrs = reader.VariableLengthRecords;
            if (vlrPredicate is not null)
            {
                vlrs = vlrs.Where(vlrPredicate);
            }

            writer.Write(reader.Header, vlrs);

            while (reader.ReadPointDataRecord() is { PointDataRecord: not null } pointDataRecord)
            {
                writer.Write(pointDataRecord.PointDataRecord, pointDataRecord.ExtraBytes);
            }

            if (writer is LasWriter flushable)
            {
                flushable.Flush();
            }
        }

        /// <summary>
        /// Copies the contents the current reader to the specified writer asynchronously.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="vlrPredicate">The <see cref="VariableLengthRecord"/> predicate.</param>
        /// <param name="cancellationToken">The token for cancelling the task.</param>
        /// <returns>The asynchronous task for copying the contents to <paramref name="writer"/>.</returns>
        public async Task CopyToAsync(ILasWriter writer, Func<VariableLengthRecord, bool>? vlrPredicate = default, CancellationToken cancellationToken = default)
        {
            IEnumerable<VariableLengthRecord> variableLengthRecords = reader.VariableLengthRecords;
            if (vlrPredicate is not null)
            {
                variableLengthRecords = variableLengthRecords.Where(vlrPredicate);
            }

            writer.Write(reader.Header, variableLengthRecords);

            while (await reader.ReadPointDataRecordAsync(cancellationToken).ConfigureAwait(false) is { PointDataRecord: not null } pointDataRecord)
            {
                await writer.WriteAsync(pointDataRecord.PointDataRecord, pointDataRecord.ExtraBytes, cancellationToken).ConfigureAwait(false);
            }

            if (writer is LasWriter flushable)
            {
                flushable.Flush();
            }
        }
#endif
    }
}