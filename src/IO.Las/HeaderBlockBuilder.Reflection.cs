// -----------------------------------------------------------------------
// <copyright file="HeaderBlockBuilder.Reflection.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// Reflection based methods.
/// </content>
public partial class HeaderBlockBuilder
{
    /// <summary>
    /// Creates a <see cref="HeaderBlockBuilder"/> from the specified typeof <see cref="PointDataFormat"/>.
    /// </summary>
    /// <typeparam name="T">The type of point data record.</typeparam>
    /// <returns>The header block builder.</returns>
    /// <exception cref="InvalidOperationException">Invalid point data type.</exception>
#if NET7_0_OR_GREATER
    public static HeaderBlockBuilder FromPointType<T>()
#else
    public static HeaderBlockBuilder FromPointType<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields)] T>()
#endif
        where T : IBasePointDataRecord =>
#if NET7_0_OR_GREATER
        new(T.Id);
#else
        System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(T)).GetDeclaredField(nameof(PointDataRecord.Id))?.GetValue(null) is byte pointTypeId
            ? new(pointTypeId)
            : throw new InvalidOperationException();
#endif
}