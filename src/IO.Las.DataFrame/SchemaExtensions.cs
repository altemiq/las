// -----------------------------------------------------------------------
// <copyright file="SchemaExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable SA1101

/// <summary>
/// <see cref="Apache.Arrow.Schema"/> extensions.
/// </summary>
internal static class SchemaExtensions
{
    extension(Apache.Arrow.Schema schema)
    {
        public Apache.Arrow.Schema ToPolarsCompatibleSchema()
        {
            // update this for Polars supported types (i.e. no byte, sbyte, ushort, short)
            var schemaBuilder = new Apache.Arrow.Schema.Builder();
            schemaBuilder.Metadata(schema.Metadata);
            foreach (var field in schema.FieldsList)
            {
                schemaBuilder.Field(builder =>
                {
                    builder
                        .Name(field.Name)
                        .DataType(MapDataType(field.DataType))
                        .Nullable(field.IsNullable);
                    if (field.HasMetadata)
                    {
                        builder.Metadata(field.Metadata);
                    }
                });
            }

            return schemaBuilder.Build();

            Apache.Arrow.Types.IArrowType MapDataType(Apache.Arrow.Types.IArrowType arrowType)
            {
                return arrowType switch
                {
                    { TypeId: Apache.Arrow.Types.ArrowTypeId.UInt8 or Apache.Arrow.Types.ArrowTypeId.UInt16 } => Apache.Arrow.Types.UInt32Type.Default,
                    { TypeId: Apache.Arrow.Types.ArrowTypeId.Int8 or Apache.Arrow.Types.ArrowTypeId.Int16 } => Apache.Arrow.Types.Int32Type.Default,
                    { } d => d,
                };
            }
        }
    }
}