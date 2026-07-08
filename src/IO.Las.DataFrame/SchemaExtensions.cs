// -----------------------------------------------------------------------
// <copyright file="SchemaExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// <see cref="Apache.Arrow.Schema"/> extensions.
/// </summary>
internal static class SchemaExtensions
{
    /// <content>
    /// <see cref="Apache.Arrow.Schema"/> extensions.
    /// </content>
    /// <param name="schema">The schema.</param>
    extension(Apache.Arrow.Schema schema)
    {
        /// <summary>
        /// Converts the <see cref="Apache.Arrow.Schema"/> to be <see cref="Polars"/> compatible.
        /// </summary>
        /// <returns>The <see cref="Polars"/> compatible schema.</returns>
        /// <exception cref="System.Diagnostics.UnreachableException">The arrow type is <see langword="null"/>.</exception>
        public Apache.Arrow.Schema ToPolarsCompatibleSchema()
        {
            // update this for Polars supported types (i.e. no byte, sbyte, ushort, short)
            var schemaBuilder = new Apache.Arrow.Schema.Builder();
            _ = schemaBuilder.Metadata(schema.Metadata);
            foreach (var field in schema.FieldsList)
            {
                _ = schemaBuilder.Field(builder =>
                {
                    _ = builder
                        .Name(field.Name)
                        .DataType(MapDataType(field.DataType))
                        .Nullable(field.IsNullable);
                    if (field.HasMetadata)
                    {
                        _ = builder.Metadata(field.Metadata);
                    }
                });
            }

            return schemaBuilder.Build();

            static Apache.Arrow.Types.IArrowType MapDataType(Apache.Arrow.Types.IArrowType arrowType)
            {
                ArgumentNullException.ThrowIfNull(arrowType);
                return arrowType switch
                {
                    { TypeId: Apache.Arrow.Types.ArrowTypeId.UInt8 or Apache.Arrow.Types.ArrowTypeId.UInt16 } => Apache.Arrow.Types.UInt32Type.Default,
                    { TypeId: Apache.Arrow.Types.ArrowTypeId.Int8 or Apache.Arrow.Types.ArrowTypeId.Int16 } => Apache.Arrow.Types.Int32Type.Default,
                    { } d => d,
                    _ => throw new System.Diagnostics.UnreachableException(),
                };
            }
        }
    }
}