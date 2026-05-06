public static class LasExtensions
{
    extension(Altemiq.IO.Las.LasReader reader)
    {
        public ValueTask DisposeAsync()
        {
            reader.Dispose();
            return default;
        }
    }

    extension(Altemiq.IO.Las.LasWriter writer)
    {
        public ValueTask DisposeAsync()
        {
            writer.Dispose();
            return default;
        }
    }
}