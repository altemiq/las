// -----------------------------------------------------------------------
// <copyright file="IUnion.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// Represents a union.
/// </summary>
public interface IUnion
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    object? Value { get; }
}

#if NET6_0_OR_GREATER
/// <summary>
/// Represents a type of union.
/// </summary>
/// <typeparam name="TUnion">The type of union.</typeparam>
public interface IUnion<TUnion> : IUnion
    where TUnion : IUnion<TUnion>
{
    /// <summary>
    /// Creates a union from a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="union">The union if the creation was successful.</param>
    /// <returns>Whether the creation was successful.</returns>
    [Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "This is by design")]
    static abstract bool TryCreate(object? value, [Diagnostics.CodeAnalysis.NotNullWhen(true)] out TUnion union);
}
#endif