// -----------------------------------------------------------------------
// <copyright file="Model.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The model.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly partial record struct Model
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Model"/> struct.
    /// </summary>
    /// <param name="brand">The brand.</param>
    /// <param name="modelName">The model name.</param>
    /// <param name="code">The code.</param>
    public Model(Brand brand, string modelName, string code) => (this.Brand, this.Name, this.Code) = (brand, modelName, code);

    /// <summary>
    /// Gets the brand.
    /// </summary>
    public Brand Brand { get; }

    /// <summary>
    /// Gets the model name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the code.
    /// </summary>
    internal string Code { get; }

    /// <summary>
    /// Converts the value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the value of this instance.</returns>>
    public override readonly string ToString() => $"{this.Brand.GetDescription()},{this.Name},{this.Code}";

    /// <summary>
    /// Parses the model from specified identifier.
    /// </summary>
    /// <param name = "s">The identifier.</param>
    /// <returns>The model.</returns>
    /// <exception cref = "KeyNotFoundException">The model could not be found.</exception>
    public static partial Model Parse(string s);

    /// <summary>
    /// Tries to parse the model from the specified identifier.
    /// </summary>
    /// <param name = "s">The identifier.</param>
    /// <param name = "model">The model.</param>
    /// <returns><see langword="true"/> if the model was successfully parsed; otherwise <see langword="false"/>.</returns>
    public static partial bool TryParse(string s, out Model model);
}