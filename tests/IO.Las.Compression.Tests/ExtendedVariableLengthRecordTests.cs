// -----------------------------------------------------------------------
// <copyright file="ExtendedVariableLengthRecordTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

public class ExtendedVariableLengthRecordTests
{
    [Test]
    [Arguments(typeof(MemoryStream))]
    [Arguments(typeof(LasMultipleMemoryStream))]
    [Arguments(typeof(ForwardOnlyMemoryStream))]
    [Arguments(typeof(ForwardOnlyLasMultipleMemoryStream))]
    public async Task WriteExtendedVariableLengthRecord(Type type)
    {
        ExtendedVariableLengthRecord evlr = new UnknownExtendedVariableLengthRecord(
            new()
            {
                UserId = "MINE",
                RecordId = 1234,
                Description = "This is my EVLR",
                RecordLengthAfterHeader = 0
            },
            []);
        HeaderBlockBuilder headerBuilder = new();
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using Stream stream = Activator.CreateInstance(type) as Stream ?? throw new InvalidCastException();
        using (LasWriter writer = new(stream, true))
        {
            writer.Write(headerBuilder.HeaderBlock);

            // write a point
            IBasePointDataRecord pt =
#if LAS1_5_OR_GREATER
                new ExtendedGpsPointDataRecord
                {
                    X = default,
                    Y = default,
                    Z = default,
                    ReturnNumber = 1,
                    NumberOfReturns = 1,
                    ScanDirectionFlag = default,
                    Classification = default,
                    EdgeOfFlightLine = default,
                    GpsTime = default,
                    PointSourceId = default,
                    ScanAngle = default,
                };
#else
                new PointDataRecord
                {
                    X = default,
                    Y = default,
                    Z = default,
                    ReturnNumber = 1,
                    NumberOfReturns = 1,
                    ScanDirectionFlag = default,
                    Classification = default,
                    EdgeOfFlightLine = default,
                    PointSourceId = default,
                    ScanAngleRank = default,
                };
#endif
            await writer.WriteAsync(pt);
            headerBuilder.Add(pt);
            await writer.WriteAsync(pt);
            headerBuilder.Add(pt);

            writer.Write(evlr);

            stream.Position = 0;
            writer.Write(headerBuilder.HeaderBlock);
        }

        stream.Position = 0;
        using LasReader reader = new(stream);
#if LAS1_5_OR_GREATER
        _ = await Assert.That(reader.Header.NumberOfPointRecords).IsEqualTo(2UL);
        _ = await Assert.That(reader.Header.LegacyNumberOfPointRecords).IsEqualTo(0U);
#else
        _ = await Assert.That(reader.Header.LegacyNumberOfPointRecords).IsEqualTo(2U);
#endif
        _ = await Assert.That(reader.ExtendedVariableLengthRecords).HasCount().EqualTo(1)
            .And.Satisfies(x => x.First(), x => x.IsEqualTo(evlr, ExtendedVariableLengthRecordComparer.Instance));
    }

    internal sealed class ForwardOnlyMemoryStream : MemoryStream
    {
        public override bool CanSeek => false;
    }

    internal sealed class ForwardOnlyLasMultipleMemoryStream : LasMultipleMemoryStream
    {
        public override bool CanSeek => false;
    }
}