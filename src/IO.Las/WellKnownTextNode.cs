// -----------------------------------------------------------------------
// <copyright file="WellKnownTextNode.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The well-known text node.
/// </summary>
public readonly struct WellKnownTextNode
{
    private const char StartChar = '[';

    private const byte StartByte = (byte)StartChar;

    private const char EndChar = ']';

    private const byte EndByte = (byte)EndChar;

    private const char SeparatorChar = ',';

    private const byte SeparatorByte = (byte)SeparatorChar;

    /// <summary>
    /// Initializes a new instance of the <see cref="WellKnownTextNode"/> class.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <param name="value">The string value.</param>
    public WellKnownTextNode(string id, string value)
        : this(id, new WellKnownTextValue(value))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WellKnownTextNode"/> class.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <param name="values">The values.</param>
    public WellKnownTextNode(string id, params IEnumerable<WellKnownTextValue> values) => (this.Id, this.Values) = (id, values);

    /// <summary>
    /// Gets the ID.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the values.
    /// </summary>
    public IEnumerable<WellKnownTextValue> Values { get; }

    /// <summary>
    /// Parses a <see cref="WellKnownTextNode"/> from the span of bytes.
    /// </summary>
    /// <param name="value">The span of bytes.</param>
    /// <returns>The well known text node.</returns>
    public static WellKnownTextNode Parse(ReadOnlySpan<byte> value)
    {
        // get the start and end
        var startValue = value.IndexOf(StartByte);
        var endValue = value.LastIndexOf(EndByte);

        // get the name
        var id = System.Text.Encoding.UTF8.GetString(TrimWriteSpace(value[..startValue]));

        // get the name
        var list = new List<WellKnownTextValue>();
        startValue++;
        value = value[startValue..endValue];
        var split = new SpanSplitEnumerator<byte>(value, StartByte, EndByte, SeparatorByte);
        while (split.MoveNext())
        {
            var item = value[split.Current];

            if (item.IndexOf(StartByte) >= 0 && item.IndexOf(EndByte) >= 0)
            {
                list.Add(Parse(item));
            }
            else if (System.Buffers.Text.Utf8Parser.TryParse(item, out double doubleValue, out _))
            {
                list.Add(doubleValue);
            }
            else if (item[0] == '\"' && item[^1] == '\"')
            {
                list.Add(System.Text.Encoding.UTF8.GetString(TrimValue(item, (byte)'\"')));
            }
            else
            {
                list.Add(new WellKnownTextLiteral(System.Text.Encoding.UTF8.GetString(item)));
            }
        }

        return new(id, list);

        static ReadOnlySpan<byte> TrimWriteSpace(ReadOnlySpan<byte> span)
        {
            return Trim(span, b => char.IsWhiteSpace((char)b));
        }

        static ReadOnlySpan<byte> TrimValue(ReadOnlySpan<byte> span, byte value)
        {
            return Trim(span, b => b == value);
        }

        static ReadOnlySpan<T> Trim<T>(ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (var i = 0; i < span.Length; i++)
            {
                if (predicate(span[i]))
                {
                    continue;
                }

                for (var j = span.Length - 1; j >= i; j--)
                {
                    if (predicate(span[j]))
                    {
                        continue;
                    }

                    return span[i..(j + 1)];
                }
            }

            return default;
        }
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <summary>
    /// Parses a <see cref="WellKnownTextNode"/> from the span of characters.
    /// </summary>
    /// <param name="value">The span of characters.</param>
    /// <returns>The well known text node.</returns>
    public static WellKnownTextNode Parse(ReadOnlySpan<char> value)
    {
        // get the start and end
        var startValue = value.IndexOf(StartChar);
        var endValue = value.LastIndexOf(EndChar);

        // get the name
        var id = value[..startValue].Trim().ToString();

        // get the name
        var list = new List<WellKnownTextValue>();
        startValue++;
        value = value[startValue..endValue];
        var split = new SpanSplitEnumerator<char>(value, StartChar, EndChar, SeparatorChar);
        while (split.MoveNext())
        {
            var item = value[split.Current].Trim();

            if (item.IndexOf(StartChar) >= 0 && item.IndexOf(EndChar) >= 0)
            {
                list.Add(Parse(item));
            }
            else if (double.TryParse(item, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
            {
                list.Add(doubleValue);
            }
            else if (item[0] == '\"' && item[^1] == '\"')
            {
                list.Add(item.Trim('\"').ToString());
            }
            else
            {
                list.Add(new WellKnownTextLiteral(new(item)));
            }
        }

        return new(id, list);
    }
#endif

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new System.Text.StringBuilder();

        builder.Append(this.Id);

        builder.Append('[');
        var first = true;
        foreach (var value in this.Values)
        {
            if (!first)
            {
                builder.Append(',');
            }

            first = false;
            builder.Append(value.ToString());
        }

        builder.Append(']');

        return builder.ToString();
    }

    /// <summary>
    /// Copies the contents of this instance into a destination <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
    /// <returns>The number of bytes written.</returns>
    public int CopyTo(Span<byte> destination)
    {
        var bytesWritten = System.Text.Encoding.ASCII.GetBytes(this.Id, destination);
        var d = destination[bytesWritten..];

        d[0] = StartByte;
        bytesWritten++;
        d = destination[bytesWritten..];

        foreach (var value in this.Values)
        {
            bytesWritten += value.CopyTo(d);
            d = destination[bytesWritten..];
        }

        d[0] = EndByte;

        return bytesWritten + 1;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private ref struct SpanSplitEnumerator<T>(ReadOnlySpan<T> span, T start, T end, T separator)
        where T : IEquatable<T>
    {
        private readonly T start = start;
        private readonly T end = end;
        private readonly T separator = separator;
        private readonly ReadOnlySpan<T> buffer = span;

        private int startCurrent = 0;
        private int endCurrent = 0;
        private int startNext = 0;

        /// <summary>
        /// Gets the current element of the enumeration.
        /// </summary>
        /// <returns>Returns a <see cref="Range"/> instance that indicates the bounds of the current element withing the source span.</returns>
        public readonly Range Current => new(this.startCurrent, this.endCurrent);

        /// <summary>
        /// Advances the enumerator to the next element of the enumeration.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the enumeration.</returns>
        public bool MoveNext()
        {
            if (this.startNext > this.buffer.Length)
            {
                return false;
            }

            var slice = this.buffer[this.startNext..];
            this.startCurrent = this.startNext;

            var separatorIndex = -1;
            var open = 0;
            for (var i = 1; i < slice.Length; i++)
            {
                if (slice[i].Equals(this.start))
                {
                    open++;
                }
                else if (slice[i].Equals(this.end))
                {
                    open--;
                }
                else if (slice[i].Equals(this.separator) && open is 0)
                {
                    separatorIndex = i;
                }

                if (separatorIndex >= 0)
                {
                    break;
                }
            }

            var elementLength = separatorIndex != -1 ? separatorIndex : slice.Length;

            this.endCurrent = this.startCurrent + elementLength;
            this.startNext = this.endCurrent + 1;

            return true;
        }
    }
}