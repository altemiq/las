// -----------------------------------------------------------------------
// <copyright file="IUnion{TUnion}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// Represents a type of <see cref="IUnion"/>.
/// </summary>
/// <typeparam name="TUnion">The type of union.</typeparam>
/// <seealso cref="IUnion" />
internal interface IUnion<TUnion> : IUnion
    where TUnion : IUnion<TUnion>
{
    /// <summary>
    /// Creates a union from a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="union">The union if the creation was successful.</param>
    /// <returns>Whether the creation was successful.</returns>
    static abstract bool TryCreate(object? value, [Diagnostics.CodeAnalysis.NotNullWhen(true)] out TUnion union);
}