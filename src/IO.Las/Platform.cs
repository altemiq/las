// -----------------------------------------------------------------------
// <copyright file="Platform.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The platform.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly partial record struct Platform
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Platform"/> struct.
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <param name="type">The type.</param>
    /// <param name="code">The code.</param>
    public Platform(PlatformId platform, PlatformType type, char code) => (this.Id, this.Type, this.Code) = (platform, type, code);

    /// <summary>
    /// Gets the ID.
    /// </summary>
    public PlatformId Id { get; }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public PlatformType Type { get; }

    /// <summary>
    /// Gets the code.
    /// </summary>
    internal char Code { get; }

    /// <summary>
    /// Converts the value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the value of this instance.</returns>>
    public override string ToString() => $"{this.Id.GetDescription()},{this.Type},{this.Code}";

    /// <summary>
    /// Parses the platform from specified identifier.
    /// </summary>
    /// <param name="c">The identifier.</param>
    /// <returns>The platform.</returns>
    /// <exception cref="KeyNotFoundException">The platform could not be found.</exception>
    public static partial Platform Parse(char c);

    /// <summary>
    /// Tries to parse the platform from the specified identifier.
    /// </summary>
    /// <param name="c">The identifier.</param>
    /// <param name="platform">The platform.</param>
    /// <returns><see langword="true"/> if the platform was successfully parsed; otherwise <see langword="false"/>.</returns>
    public static partial bool TryParse(char c, out Platform platform);
}