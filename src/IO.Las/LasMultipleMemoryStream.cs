// -----------------------------------------------------------------------
// <copyright file="LasMultipleMemoryStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Las"/> <see cref="MultipleMemoryStream"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Meziantou", "MA0053:Make class sealed", Justification = "This is used in other projects.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required for automated cleanup")]
internal class LasMultipleMemoryStream : MultipleStream
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LasMultipleMemoryStream"/> class.
    /// </summary>
    public LasMultipleMemoryStream()
        : this(LasStreams.Comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasMultipleMemoryStream"/> class.
    /// </summary>
    /// <param name="comparer">The <see cref="IComparer{T}"/> implementation to use when comparing keys.</param>
    protected LasMultipleMemoryStream(IComparer<string> comparer)
        : this(new LasStreamDictionary(comparer))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasMultipleMemoryStream"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    protected LasMultipleMemoryStream(IDictionary<string, Stream> dictionary)
        : base(dictionary)
    {
    }

    /// <inheritdoc/>
    public override long Position
    {
        get => base.Position;
        set
        {
            if (value is 0)
            {
                this.Reset();
            }

            base.Position = value;
        }
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        if (origin is SeekOrigin.Begin && offset is 0L)
        {
            this.Reset();
        }

        return base.Seek(offset, origin);
    }

    /// <inheritdoc/>
    protected override Stream CreateStream(string name) => new MemoryStream();
}