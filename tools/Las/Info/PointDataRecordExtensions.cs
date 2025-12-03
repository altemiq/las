// -----------------------------------------------------------------------
// <copyright file="PointDataRecordExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// <see cref="IBasePointDataRecord"/> extensions.
/// </summary>
public static class PointDataRecordExtensions
{
    /// <summary>
    /// Gets a value indicating whether this is a first return.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <returns><see langword="true"/> if this is a first return; otherwise <see langword="false"/>.</returns>
    public static bool IsFirst(this IBasePointDataRecord record) => record.ReturnNumber.IsFirst();

    /// <summary>
    /// Gets a value indicating whether this is a first return.
    /// </summary>
    /// <param name="returnNumber">The return number.</param>
    /// <returns><see langword="true"/> if this is a first return; otherwise <see langword="false"/>.</returns>
    public static bool IsFirst(this byte returnNumber) => returnNumber <= 1;

    /// <summary>
    /// Gets a value indicating whether this is an intermediate return.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <returns><see langword="true"/> if this is an intermediate return; otherwise <see langword="false"/>.</returns>
    public static bool IsIntermediate(this IBasePointDataRecord record) => record.ReturnNumber.IsIntermediate(record.NumberOfReturns);

    /// <summary>
    /// Gets a value indicating whether this is an intermediate return.
    /// </summary>
    /// <param name="returnNumber">The return number.</param>
    /// <param name="numberOfReturns">The number of returns.</param>
    /// <returns><see langword="true"/> if this is an intermediate return; otherwise <see langword="false"/>.</returns>
    public static bool IsIntermediate(this byte returnNumber, byte numberOfReturns) => !returnNumber.IsFirst() && !returnNumber.IsLast(numberOfReturns);

    /// <summary>
    /// Gets a value indicating whether this is a last return.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <returns><see langword="true"/> if this is a last return; otherwise <see langword="false"/>.</returns>
    public static bool IsLast(this IBasePointDataRecord record) => record.ReturnNumber.IsLast(record.NumberOfReturns);

    /// <summary>
    /// Gets a value indicating whether this is a last return.
    /// </summary>
    /// <param name="returnNumber">The return number.</param>
    /// <param name="numberOfReturns">The number of returns.</param>
    /// <returns><see langword="true"/> if this is a last return; otherwise <see langword="false"/>.</returns>
    public static bool IsLast(this byte returnNumber, byte numberOfReturns) => returnNumber >= numberOfReturns;

    /// <summary>
    /// Gets a value indicating whether this is a single return.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <returns><see langword="true"/> if this is a single return; otherwise <see langword="false"/>.</returns>
    public static bool IsSingle(this IBasePointDataRecord record) => record.NumberOfReturns.IsSingle();

    /// <summary>
    /// Gets a value indicating whether this is a single return.
    /// </summary>
    /// <param name="numberOfReturns">The number of returns.</param>
    /// <returns><see langword="true"/> if this is a single return; otherwise <see langword="false"/>.</returns>
    public static bool IsSingle(this byte numberOfReturns) => numberOfReturns <= 1;
}