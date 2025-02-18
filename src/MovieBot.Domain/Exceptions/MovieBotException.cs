namespace MovieBot.Domain.Exceptions;

public class MovieBotException:Exception
{
    public int StatusCode { get; set; }
    public MovieBotException(int StatusCode,string message) : base(message)
    {
        this.StatusCode = StatusCode;
    }
}
