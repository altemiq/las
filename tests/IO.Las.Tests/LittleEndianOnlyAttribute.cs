namespace Altemiq.IO.Las;

public class LittleEndianOnlyAttribute() : SkipAttribute("Cannot map memory if architecture is big endian")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context) => Task.FromResult(!BitConverter.IsLittleEndian);
}