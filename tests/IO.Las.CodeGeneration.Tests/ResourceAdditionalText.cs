namespace Altemiq.IO.Las.CodeGeneration;

public class ResourceAdditionalText : Microsoft.CodeAnalysis.AdditionalText
{
    private readonly byte[] bytes;
    private readonly int length;

    private ResourceAdditionalText(string path, byte[] bytes, int length)
    {
        this.Path = path;
        this.bytes = bytes;
        this.length = length;
    }

    public override string Path { get; }

    public static Microsoft.CodeAnalysis.AdditionalText Create(string resourceName)
    {
        var stream = typeof(ResourceAdditionalText).Assembly.GetManifestResourceStream(typeof(ResourceAdditionalText), resourceName)
                     ?? throw new KeyNotFoundException();
        var bytes = new byte[stream.Length];
        var readBytes = stream.Read(bytes, 0, bytes.Length);
        return new ResourceAdditionalText(resourceName, bytes, readBytes);
    }

    public override Microsoft.CodeAnalysis.Text.SourceText GetText(CancellationToken cancellationToken = default)
        => Microsoft.CodeAnalysis.Text.SourceText.From(this.bytes, this.length, System.Text.Encoding.UTF8);
}