// -----------------------------------------------------------------------
// <copyright file="Options.To.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// To options.
/// </content>
internal static partial class Options
{
    /// <summary>
    /// To options.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "This is by design")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass", Justification = "This is by design")]
    public static class To
    {
        /// <summary>
        /// The output option.
        /// </summary>
        public static readonly Option<FileInfo> Output = new("-o", "--output") { Description = Tool.Properties.Resources.Option_OutputDescription, HelpName = "OUTPUT" };
    }
}