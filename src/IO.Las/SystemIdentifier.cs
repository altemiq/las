// -----------------------------------------------------------------------
// <copyright file="SystemIdentifier.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The system identifier.
/// </summary>
public sealed class SystemIdentifier : IEquatable<SystemIdentifier>
{
    private static readonly char[] SpaceSeparator = [' '];

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemIdentifier"/> class.
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <param name="model">The model.</param>
    public SystemIdentifier(Platform platform, Model model) => (this.Platform, this.Model) = (platform, model);

    /// <summary>
    /// Gets the platform.
    /// </summary>
    public Platform Platform { get; }

    /// <summary>
    /// Gets the model.
    /// </summary>
    public Model Model { get; }

    /// <summary>
    /// Parses the system identifier.
    /// </summary>
    /// <param name="s">The input string.</param>
    /// <returns>The input identifier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="s"/> is the incorrect length.</exception>
    public static SystemIdentifier Parse(string s) => s switch
    {
        null => throw new ArgumentNullException(nameof(s)),
        { Length: not 5 } => throw new ArgumentOutOfRangeException(nameof(s)),
        var value => new SystemIdentifier(Platform.Parse(value[0]), Model.Parse(value[1..])),
    };

    /// <summary>
    /// Parses multiple system identifiers.
    /// </summary>
    /// <param name="s">The input string.</param>
    /// <returns>The parsed system identifiers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="s"/> is the incorrect length.</exception>
    public static IEnumerable<SystemIdentifier> ParseMultiple(string s)
    {
        return s switch
        {
            null => throw new ArgumentNullException(nameof(s)),
            { Length: < 5 } => throw new ArgumentOutOfRangeException(nameof(s)),
            var value => Core(value),
        };

        static IEnumerable<SystemIdentifier> Core(string s)
        {
            foreach (var item in s.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return Parse(item);
            }
        }
    }

    /// <summary>
    /// Tries to parse the system identifier from the specified identifier.
    /// </summary>
    /// <param name="s">The identifier.</param>
    /// <param name="systemIdentifier">The system identifier.</param>
    /// <returns><see langword="true"/> if the system identifier was successfully parsed; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string s, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out SystemIdentifier? systemIdentifier)
    {
        if (s is { Length: 5 } value
            && Platform.TryParse(value[0], out var platform)
            && Model.TryParse(value[1..], out var model))
        {
            systemIdentifier = new(platform, model);
            return true;
        }

        systemIdentifier = default;
        return false;
    }

    /// <summary>
    /// Tries to parse multiple system identifiers from the specified identifier.
    /// </summary>
    /// <param name="s">The identifier.</param>
    /// <param name="systemIdentifiers">The system identifiers.</param>
    /// <returns><see langword="true"/> if the system identifiers were successfully parsed; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string s, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out SystemIdentifier[]? systemIdentifiers)
    {
        if (s is { Length: >= 5 } value)
        {
            var identifiers = new List<SystemIdentifier>();

            foreach (var item in value.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Platform.TryParse(item[0], out var platform) && Model.TryParse(item[1..], out var model))
                {
                    identifiers.Add(new(platform, model));
                }
                else
                {
                    systemIdentifiers = default;
                    return false;
                }
            }

            systemIdentifiers = [.. identifiers];
            return true;
        }

        systemIdentifiers = default;
        return false;
    }

    /// <inheritdoc/>
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is SystemIdentifier systemIdentifier && this.Equals(systemIdentifier);

    /// <inheritdoc/>
    public bool Equals(SystemIdentifier? other) => other is not null && this.Platform.Equals(other.Platform) && this.Model.Equals(other.Model);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.Platform, this.Model);

    /// <summary>
    /// Converts the value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the value of this instance.</returns>>
    public override string ToString() => string.Concat(this.Platform.Code, this.Model.Code);
}