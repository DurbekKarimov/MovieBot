namespace MovieBot.Service.Services.BotConfigurations.ServiceHelpers;

public interface IStateService<T> where T : class
{
    void SetState(string key, T state);
    T GetState(string key);
    void ClearState(string key);
}
