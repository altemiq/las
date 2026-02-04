// -----------------------------------------------------------------------
// <copyright file="LazMultipleMemoryStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAZ <see cref="MemoryStream"/> <see cref="MultipleStream"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Meziantou", "MA0053:Make class sealed", Justification = "This is used in other projects.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Meziantou", "MA0182:Avoid unused internal types", Justification = "This is used in other projects.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required for automated cleanup")]
internal class LazMultipleMemoryStream : LasMultipleMemoryStream
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LazMultipleMemoryStream"/> class.
    /// </summary>
    public LazMultipleMemoryStream()
        : this(LazStreams.Comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LazMultipleMemoryStream"/> class.
    /// </summary>
    /// <param name="comparer">The <see cref="IComparer{T}"/> implementation to use when comparing keys.</param>
    protected LazMultipleMemoryStream(IComparer<string> comparer)
        : this(new LasStreamDictionary(comparer))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LazMultipleMemoryStream"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    protected LazMultipleMemoryStream(IDictionary<string, Stream> dictionary)
        : base(dictionary)
    {
    }
}