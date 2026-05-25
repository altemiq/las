// -----------------------------------------------------------------------
// <copyright file="UnionAttribute.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// Indicates that a class or struct is a union type, enabling compiler support for union behaviors.
/// </summary>
/// <remarks>
/// <para>
/// Any class or struct annotated with this attribute is recognized by the C# compiler as a union type.
/// Union types may support behaviors such as implicit conversions from case types, pattern matching
/// that unwraps the union's contents, and switch exhaustiveness checking.
/// </para>
/// </remarks>
/// <seealso cref="IUnion" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
[Microsoft.CodeAnalysis.Embedded]
internal sealed class UnionAttribute : Attribute;