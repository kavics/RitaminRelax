using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;

namespace RitaminRelax
{
    public static class RRTools
    {
        public const string RRManagersPath = "/Root/IMS/Public/RRManagers";

        public static Group RRManagers => Node.LoadAsync<Group>(RRManagersPath, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
