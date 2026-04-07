// -----------------------------------------------------------------------
// <copyright file="UnionAttribute.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// The attribute for <see cref="IUnion"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal sealed class UnionAttribute : Attribute;