using NLog;

namespace weather.Logging;

public interface IContext
{
    ILogger Logger { get; }
}

public class Context : IContext
{
    public ILogger Logger { get; }

    public Context()
    {
        LogManager.Setup().LoadConfiguration(builder =>
            builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile(fileName: "logs.txt"));
        Logger = LogManager.GetCurrentClassLogger();
    }
}

public static class ContextManager
{
    public static IContext Context { get; }

    static ContextManager() => Context = new Context();
}