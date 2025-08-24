using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;

namespace RitaminRelax
{
    public static class RRTools
    {
        public const string RRManagersPath = "/Root/IMS/Public/RRManagers";
        public const string BookingContainerPath = "/Root/Content/Bookings";

        public static Group RRManagers => Node.LoadAsync<Group>(RRManagersPath, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

        public static Node BookingContainer => Node.LoadAsync<Folder>(BookingContainerPath, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
