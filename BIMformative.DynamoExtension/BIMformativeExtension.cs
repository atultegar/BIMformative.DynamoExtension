using Dynamo.Extensions;
namespace BIMformative.DynamoExtension
{
    public class BIMformativeExtension : IExtension
    {
        public string UniqueId => "bimformative.extension.core";

        public string Name => "BIMformative";

        public void Dispose()
        {
        }

        public void Ready(ReadyParams p)
        {
            // No-op for now
        }

        public void Shutdown()
        {
        }

        public void Startup(StartupParams sp)
        {
        }
    }
}
