using Microsoft.Extensions.Caching.Distributed;

namespace VotingApp
{
    public class CacheManager
    {
        private const string VOTE1_KEY = "vote1";
        private const string VOTE2_KEY = "vote2";
        
        private readonly IDistributedCache _cache;

        public CacheManager(IDistributedCache cache)
        {
            this._cache = cache;
        }

        public int Vote1
        {
            get
            {
                var value = _cache.GetString(VOTE1_KEY);
                if(string.IsNullOrEmpty(value))
                {
                    value = "0";
                    SetCache(VOTE1_KEY, value);
                    return 0;
                }
                return int.Parse(value);
            }
        }

        public int Vote2
        {
            get
            {
                var value = _cache.GetString(VOTE2_KEY);
                if(string.IsNullOrEmpty(value))
                {
                    value = "0";
                    SetCache(VOTE2_KEY, value);
                    return 0;
                }
                return int.Parse(value);
            }
        }

        public void IncrementVote1()
        {
            SetCache(VOTE1_KEY, (Vote1 + 1).ToString()); 
        }

        public void IncrementVote2()
        {
            SetCache(VOTE2_KEY, (Vote2 + 1).ToString()); 
        }

        public void Reset()
        {
            SetCache(VOTE1_KEY, "0");
            SetCache(VOTE2_KEY, "0");
        }

        private void SetCache(string key, string value)
        {
            _cache.SetString(key, value); 
        }
    }
}