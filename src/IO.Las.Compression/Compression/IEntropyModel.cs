// -----------------------------------------------------------------------
// <copyright file="IEntropyModel.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The entropy model.
/// </summary>
internal interface IEntropyModel
{
    /// <summary>
    /// Initializes the entropy model.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>The return.</returns>
    bool Initialize(uint[]? table = null);

    /// <summary>
    /// Updates this instance.
    /// </summary>
    void Update();
}