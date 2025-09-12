// -----------------------------------------------------------------------
// <copyright file="LasStreamDictionary.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Las"/> <see cref="Stream"/> <see cref="System.Collections.IDictionary"/>.
/// </summary>
internal sealed class LasStreamDictionary(IComparer<string> comparer) : IDictionary<string, Stream>
{
    private readonly IList<string> keys = [];
    private readonly IList<Stream> values = [];

    /// <inheritdoc/>
    public ICollection<string> Keys => this.keys;

    /// <inheritdoc/>
    public ICollection<Stream> Values => this.values;

    /// <inheritdoc/>
    public int Count => this.keys.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public Stream this[string key]
    {
        get => this.values[this.keys.IndexOf(key)];
        set => this.values[this.keys.IndexOf(key)] = value;
    }

    /// <inheritdoc/>
    public void Add(string key, Stream value)
    {
        // find the index quickly
        for (var i = 0; i < this.keys.Count; i++)
        {
            if (comparer.Compare(key, this.keys[i]) >= 0)
            {
                continue;
            }

            // insert this before
            this.keys.Insert(i, key);
            this.values.Insert(i, value);
            return;
        }

        this.keys.Add(key);
        this.values.Add(value);
    }

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, Stream>>.Add(KeyValuePair<string, Stream> item) => this.Add(item.Key, item.Value);

    /// <inheritdoc/>
    public void Clear()
    {
        this.keys.Clear();
        this.values.Clear();
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, Stream>>.Contains(KeyValuePair<string, Stream> item)
    {
        var index = this.keys.IndexOf(item.Key);
        return index >= 0 && this.values[index] == item.Value;
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key) => this.keys.Contains(key, StringComparer.Ordinal);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, Stream>>.CopyTo(KeyValuePair<string, Stream>[] array, int arrayIndex)
    {
        for (var i = 0; i < this.keys.Count; i++)
        {
            array[arrayIndex + i] = new(this.keys[i], this.values[i]);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, Stream>> GetEnumerator() => new Enumerator(this.keys, this.values);

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        if (this.keys.IndexOf(key) is not (>= 0 and var index))
        {
            return false;
        }

        this.keys.RemoveAt(index);
        this.values.RemoveAt(index);
        return true;
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, Stream>>.Remove(KeyValuePair<string, Stream> item) => this.Remove(item.Key);

    /// <inheritdoc/>
#if NETSTANDARD || NETFRAMEWORK
    public bool TryGetValue(string key, out Stream value)
#else
    public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Stream value)
#endif
    {
        var index = this.keys.IndexOf(key);
        if (index >= 0)
        {
            value = this.values[index];
            return true;
        }

        value = default!;
        return false;
    }

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    private struct Enumerator(IList<string> keys, IList<Stream> values) : IEnumerator<KeyValuePair<string, Stream>>
    {
        private int index = -1;

        readonly KeyValuePair<string, Stream> IEnumerator<KeyValuePair<string, Stream>>.Current => new(keys[this.index], values[this.index]);

        readonly object System.Collections.IEnumerator.Current => new KeyValuePair<string, Stream>(keys[this.index], values[this.index]);

        readonly void IDisposable.Dispose()
        {
        }

        bool System.Collections.IEnumerator.MoveNext() => ++this.index < keys.Count;

        void System.Collections.IEnumerator.Reset() => this.index = -1;
    }
}