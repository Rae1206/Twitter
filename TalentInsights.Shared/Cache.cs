namespace TalentInsights.Shared
{
    public class Cache<T>
    {
        private readonly Dictionary<string, T> _data = new Dictionary<string, T>();

        public T Add(string key, T data)
        {
            _data.Add(key, data);
            return data;
        }

        public T? Get(string key)
        {
            try
            {
                return _data[key];
            }
            catch (Exception)
            {
                return default;
            }
        }

        public List<T> Get()
        {
            return [.. _data.Values];
        }

        public bool Delete(string key)
        {
            return _data.Remove(key);
        }
    }
}
