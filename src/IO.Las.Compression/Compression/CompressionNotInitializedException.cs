// -----------------------------------------------------------------------
// <copyright file="CompressionNotInitializedException.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// Represents an error that the compression has not been initialized.
/// </summary>
#if NETSTANDARD2_0_OR_GREATER || NETFRAMEWORK || NET
[Serializable]
#endif
public class CompressionNotInitializedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompressionNotInitializedException"/> class.
    /// </summary>
    public CompressionNotInitializedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressionNotInitializedException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string)"/>
    public CompressionNotInitializedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressionNotInitializedException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string, Exception)"/>
    public CompressionNotInitializedException(string message, Exception inner)
        : base(message, inner)
    {
    }

#if NETSTANDARD2_0_OR_GREATER || NETFRAMEWORK || NET
    /// <summary>
    /// Initializes a new instance of the <see cref="CompressionNotInitializedException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
#if NET5_0_OR_GREATER
    [Obsolete("Legacy serialization infrastructure APIs marked obsolete", DiagnosticId = "SYSLIB0051")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "This will be removed when removed from the underlying class")]
#endif
    protected CompressionNotInitializedException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
    }
#endif
}