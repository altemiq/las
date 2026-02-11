namespace Altemiq.IO.Las;

public class ExtensionMethodTests
{
    [Test]
    public async Task MoveToUnseekableForwardsOnly()
    {
        var stream = new UnseekableStream();

        await Assert.That(() => stream.MoveToPositionForwardsOnly(500)).ThrowsNothing();
        await Assert.That(() => stream.MoveToPositionForwardsOnly(400)).ThrowsNothing();
        await Assert.That(stream.Position).IsEqualTo(500);
    }

    [Test]
    public async Task MoveToUnseekableForwardsOnlyAsync()
    {
        var stream = new UnseekableStream();

        await Assert.That(async () => await stream.MoveToPositionForwardsOnlyAsync(500)).ThrowsNothing();
        await Assert.That(async () => await stream.MoveToPositionForwardsOnlyAsync(400)).ThrowsNothing();
        await Assert.That(stream.Position).IsEqualTo(500);
    }

    [Test]
    public async Task MoveToUnseekableAbsolute()
    {
        var stream = new UnseekableStream();

        await Assert.That(() => stream.MoveToPositionAbsolute(500)).ThrowsNothing();
        await Assert.That(() => stream.MoveToPositionAbsolute(400)).Throws<InvalidOperationException>();
        await Assert.That(stream.Position).IsEqualTo(500);
    }

    [Test]
    public async Task MoveToUnseekableAbsoluteAsync()
    {
        var stream = new UnseekableStream();

        await Assert.That(async () => await stream.MoveToPositionAbsoluteAsync(500)).ThrowsNothing();
        await Assert.That(async () => await stream.MoveToPositionAbsoluteAsync(400)).Throws<InvalidOperationException>();
        await Assert.That(stream.Position).IsEqualTo(500);
    }


    private sealed class UnseekableStream(Stream stream) : Stream
    {
        public UnseekableStream()
            : this(CreateAndFillStream(1024))
        {
        }

        public override void Flush() => stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);

        public override void SetLength(long value) => stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

        public override long Position
        {
            get => stream.Position;
            set => throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
            }

            base.Dispose(disposing);
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public override async ValueTask DisposeAsync()
        {
            await stream.DisposeAsync();
            await base.DisposeAsync();
        }
#endif

        private static MemoryStream CreateAndFillStream(int size)
        {
            var stream = new MemoryStream(size);
            var random = new Random();
            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(size);
            random.NextBytes(buffer);
            stream.Write(buffer, 0, size);
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
            stream.Position = 0;
            return stream;
        }
    }
}