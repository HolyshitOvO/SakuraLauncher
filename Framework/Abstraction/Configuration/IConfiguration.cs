using Hake.Extension.ValueRecord;

namespace CandyLauncher.Abstraction.Base
{
    public interface IConfiguration
    {
        RecordBase Root { get; }

        QuickConfig Options { get; }
    }
}
