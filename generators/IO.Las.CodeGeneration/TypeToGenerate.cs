// -----------------------------------------------------------------------
// <copyright file="TypeToGenerate.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

/// <summary>
/// The type to generate.
/// </summary>
/// <param name="namespace">The namespace.</param>
/// <param name="name">The name.</param>
public readonly struct TypeToGenerate(string? @namespace, string name)
    : IEquatable<TypeToGenerate>
{
    /// <summary>
    /// Gets the namespace.
    /// </summary>
    public string? Namespace { get; } = @namespace;

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The equal operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(TypeToGenerate left, TypeToGenerate right) => left.Equals(right);

    /// <summary>
    /// The not-equal operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(TypeToGenerate left, TypeToGenerate right) => !left.Equals(right);

    /// <inheritdoc/>
    public bool Equals(TypeToGenerate other) => StringComparer.Ordinal.Equals(this.Namespace, other.Namespace) && StringComparer.Ordinal.Equals(this.Name, other.Name);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TypeToGenerate other && this.Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            if (this.Namespace is { } ns)
            {
                return (StringComparer.Ordinal.GetHashCode(ns) * 397) ^ StringComparer.Ordinal.GetHashCode(this.Name);
            }

            return StringComparer.Ordinal.GetHashCode(this.Name);
        }
    }
}