using Microsoft.Extensions.Caching.Memory;

namespace MovieBot.Service.Services.BotConfigurations.ServiceHelpers;

public class StateService<T> : IStateService<T> where T : class, new()
{
    private readonly IMemoryCache cache;
    private readonly TimeSpan defaultExpiration = TimeSpan.FromMinutes(30);

    public StateService(IMemoryCache cache)
    {
        this.cache = cache;
    }

    public void SetState(string key, T state)
    {
        this.cache.Set(key, state, this.defaultExpiration);
    }

    public T GetState(string key)
    {
        if (!this.cache.TryGetValue(key, out T state))
        {
            state = new T();
            this.cache.Set(key, state, this.defaultExpiration);
        }
        return state;
    }

    public void ClearState(string key)
    {
        this.cache.Remove(key);
    }
}
