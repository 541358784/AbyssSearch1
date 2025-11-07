using YooAsset;

namespace Wonderland.Utility
{
    public class RemoteServices : IRemoteServices
    {
        private readonly string _hostUrl;

        public RemoteServices(string hostUrl)
        {
            _hostUrl = hostUrl;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_hostUrl}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_hostUrl}/{fileName}";
        }
    }
}