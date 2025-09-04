// -----------------------------------------------------------------------
// <copyright file="Common.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// Common compression values.
/// </summary>
internal static class Common
{
    /// <summary>
    /// The number return level matrix.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For LAS files with the return (r) and the number (n) of returns field correctly populated the mapping should really
    /// be only the following.
    /// </para>
    /// <code>
    ///  { 15, 15, 15, 15, 15, 15, 15, 15 },
    ///  { 15,  0, 15, 15, 15, 15, 15, 15 },
    ///  { 15,  1,  2, 15, 15, 15, 15, 15 },
    ///  { 15,  3,  4,  5, 15, 15, 15, 15 },
    ///  { 15,  6,  7,  8,  9, 15, 15, 15 },
    ///  { 15, 10, 11, 12, 13, 14, 15, 15 },
    ///  { 15, 15, 15, 15, 15, 15, 15, 15 },
    ///  { 15, 15, 15, 15, 15, 15, 15, 15 }
    /// </code>
    /// <para>
    /// However, some files start the numbering of r and n with 0, only have return counts r, or only have number of return
    /// counts n, or mix up the position of r and n. we therefore "complete" the table to also map those "undesired" r &amp; n
    /// combinations to different contexts.
    /// </para>
    /// </remarks>
    public static readonly byte[][] NumberReturnMap =
    [
        [15, 14, 13, 12, 11, 10, 9, 8],
        [14, 0, 1, 3, 6, 10, 10, 9],
        [13, 1, 2, 4, 7, 11, 11, 10],
        [12, 3, 4, 5, 8, 12, 12, 11],
        [11, 6, 7, 8, 9, 13, 13, 12],
        [10, 10, 11, 12, 13, 14, 14, 13],
        [9, 10, 11, 12, 13, 14, 15, 14],
        [8, 9, 10, 11, 12, 13, 14, 15],
    ];

    /// <summary>
    /// The number return level matrix.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For LAS files with the return (r) and the number (n) of returns field correctly populated the mapping should really
    /// be only the following.
    /// </para>
    /// <code>
    ///  {  0,  7,  7,  7,  7,  7,  7,  7 },
    ///  {  7,  0,  7,  7,  7,  7,  7,  7 },
    ///  {  7,  1,  0,  7,  7,  7,  7,  7 },
    ///  {  7,  2,  1,  0,  7,  7,  7,  7 },
    ///  {  7,  3,  2,  1,  0,  7,  7,  7 },
    ///  {  7,  4,  3,  2,  1,  0,  7,  7 },
    ///  {  7,  5,  4,  3,  2,  1,  0,  7 },
    ///  {  7,  6,  5,  4,  3,  2,  1,  0 }
    /// </code>
    /// <para>
    /// However, some files start the numbering of r and n with 0, only have return counts r, or only have number of return
    /// counts n, or mix up the position of r and n. we therefore "complete" the table to also map those "undesired" r &amp; n
    /// combinations to different contexts.
    /// </para>
    /// </remarks>
    public static readonly byte[][] NumberReturnLevel =
    [
        [0, 1, 2, 3, 4, 5, 6, 7],
        [1, 0, 1, 2, 3, 4, 5, 6],
        [2, 1, 0, 1, 2, 3, 4, 5],
        [3, 2, 1, 0, 1, 2, 3, 4],
        [4, 3, 2, 1, 0, 1, 2, 3],
        [5, 4, 3, 2, 1, 0, 1, 2],
        [6, 5, 4, 3, 2, 1, 0, 1],
        [7, 6, 5, 4, 3, 2, 1, 0],
    ];

    /// <summary>
    /// The number return map with 6 contexts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For LAS points with correctly populated return numbers (1 &lt;= r &lt;= n) and number of returns of given pulse (1 &lt;= n &lt;= 15) the return mapping that
    /// serializes the possible combinations into one number should be the following.
    /// </para>
    /// <code>
    ///  { ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,   0, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,   1,   2, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,   3,   4,   5, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,   6,   7,   8,   9, ---, ---, ---, ---, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,  10,  11,  12,  13,  14, ---, ---, ---, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,  15,  16,  17,  18,  19,  20, ---, ---, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,  21,  22,  23,  24,  25,  26,  27, ---, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,  28,  29,  30,  31,  32,  33,  34,  35, ---, ---, ---, ---, ---, ---, --- },
    ///  { ---,  36,  37,  38,  39,  40,  41,  42,  43,  44, ---, ---, ---, ---, ---, --- },
    ///  { ---,  45,  46,  47,  48,  49,  50,  51,  52,  53,  54, ---, ---, ---, ---, --- },
    ///  { ---,  55,  56,  57,  58,  59,  60,  61,  62,  63,  64,  65, ---, ---, ---, --- },
    ///  { ---,  66,  67,  68,  69,  70,  71,  72,  73,  74,  75,  76,  77, ---, ---, --- },
    ///  { ---,  78,  89,  80,  81,  82,  83,  84,  85,  86,  87,  88,  89,  90, ---, --- },
    ///  { ---,  91,  92,  93,  94,  95,  96,  97,  98,  99, 100, 101, 102, 103, 104, --- },
    ///  { ---, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119 }
    /// </code>
    /// <para>
    /// We drastically simplify the number of return combinations that we want to distinguish down to 16 as higher returns will not have significant entropy differences.
    /// </para>
    /// <code>
    ///  { --, --, --, --, --, --, --, --, --, --, --, --, --, --, --, -- },
    ///  { --,  0, --, --, --, --, --, --, --, --, --, --, --, --, --, -- },
    ///  { --,  1,  2, --, --, --, --, --, --, --, --, --, --, --, --, -- },
    ///  { --,  3,  4,  5, --, --, --, --, --, --, --, --, --, --, --, -- },
    ///  { --,  6,  7,  8,  9, --, --, --, --, --, --, --, --, --, --, -- },
    ///  { --, 10, 11, 12, 13, 14, --, --, --, --, --, --, --, --, --, -- },
    ///  { --, 10, 11, 12, 13, 14, 15, --, --, --, --, --, --, --, --, -- },
    ///  { --, 10, 11, 12, 12, 13, 14, 15, --, --, --, --, --, --, --, -- },
    ///  { --, 10, 11, 12, 12, 13, 13, 14, 15, --, --, --, --, --, --, -- },
    ///  { --, 10, 11, 11, 12, 12, 13, 13, 14, 15, --, --, --, --, --, -- },
    ///  { --, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, --, --, --, --, -- },
    ///  { --, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, --, --, --, -- },
    ///  { --, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, --, --, -- },
    ///  { --, 10, 10, 11, 11, 12, 12, 12, 13, 13, 14, 14, 15, 15, --, -- },
    ///  { --, 10, 10, 11, 11, 12, 12, 12, 13, 13, 13, 14, 14, 15, 15, -- },
    ///  { --, 10, 10, 11, 11, 12, 12, 12, 13, 13, 13, 14, 14, 14, 15, 15 }
    /// </code>
    /// <para>
    /// However, as some files start the numbering of r and n with 0, only have return counts r, only have number of return per pulse n, or mix up position of r and n, we complete
    /// the table to also map those "undesired" r and n combinations to different contexts.
    /// </para>
    /// </remarks>
    public static readonly byte[][] NumberReturnMap6Context =
    [
        [0, 1, 2, 3, 4, 5, 3, 4, 4, 5, 5, 5, 5, 5, 5, 5],
        [1, 0, 1, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3],
        [2, 1, 2, 4, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3],
        [3, 3, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
        [4, 3, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
        [5, 3, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
        [3, 3, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4],
        [4, 3, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4],
        [4, 3, 4, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4],
        [5, 3, 4, 4, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4],
        [5, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4],
        [5, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 4, 4, 4],
        [5, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 4, 4],
        [5, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 4],
        [5, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5],
        [5, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5],
    ];

    /// <summary>
    /// The matrix for number return level with 8 contexts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For LAS points with return number (1 &lt;= r &lt;= n) and a number of returns of given pulse (1 &lt;= n &lt;= 15) the level of penetration counted in number
    /// of returns should really simply be n - r with all invalid combinations being mapped to 15 like shown below.
    /// </para>
    /// <code>
    ///  {  0, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  0, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  1,  0, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  2,  1,  0, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  3,  2,  1,  0, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  4,  3,  2,  1,  0, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  5,  4,  3,  2,  1,  0, 15, 15, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  6,  5,  4,  3,  2,  1,  0, 15, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  7,  6,  5,  4,  3,  2,  1,  0, 15, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  8,  7,  6,  5,  4,  3,  2,  1,  0, 15, 15, 15, 15, 15, 15 }
    ///  { 15,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0, 15, 15, 15, 15, 15 }
    ///  { 15, 10,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0, 15, 15, 15, 15 }
    ///  { 15, 11, 10,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0, 15, 15, 15 }
    ///  { 15, 12, 11, 10,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0, 15, 15 }
    ///  { 15, 13, 12, 11, 10,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0, 15 }
    ///  { 15, 14, 13, 12, 11, 10,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0 }
    /// </code>
    /// <para>
    /// However, some files start the numbering of r and n with 0, only have return counts r, or only have number of returns of given pulse n, or
    /// mix up the position of r and n. we therefore "complete" the table to also map those "undesired" r &amp; n combinations to different contexts.
    /// </para>
    /// <para>
    /// We also stop the enumeration of the levels of penetration at 7 and map all higher penetration levels also to 7 in order to keep the total
    /// number of contexts reasonably small.
    /// </para>
    /// </remarks>
    public static readonly byte[][] NumberReturnLevel8Context =
    [
        [0, 1, 2, 3, 4, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7],
        [1, 0, 1, 2, 3, 4, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7],
        [2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 7, 7, 7, 7, 7, 7],
        [3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 7, 7, 7, 7, 7],
        [4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 7, 7, 7, 7],
        [5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 7, 7, 7],
        [6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 7, 7],
        [7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 7],
        [7, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7],
        [7, 7, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6],
        [7, 7, 7, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5],
        [7, 7, 7, 7, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4],
        [7, 7, 7, 7, 7, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3],
        [7, 7, 7, 7, 7, 7, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2],
        [7, 7, 7, 7, 7, 7, 7, 7, 6, 5, 4, 3, 2, 1, 0, 1],
        [7, 7, 7, 7, 7, 7, 7, 7, 7, 6, 5, 4, 3, 2, 1, 0],
    ];
}