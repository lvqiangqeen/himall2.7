using Enyim.Caching;

namespace Himall.Strategy
{
    class MemcachedClientService
    {
        private static readonly MemcachedClientService _instance = new MemcachedClientService();
        private readonly MemcachedClient _client;

        private MemcachedClientService()
        {
            this._client = new MemcachedClient("memcached");
        }

        public MemcachedClient Client
        {
            get { return this._client; }
        }

        public static MemcachedClientService Instance
        {
            get { return _instance; }
        }
    }
}
