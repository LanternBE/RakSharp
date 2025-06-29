namespace RakSharp.Utils;

public static class Logger {
    
    public enum LogLevel {
        Debug,
        Info,
        Warn,
        Error
    }
    
    public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

    private static readonly Lock Lock = new();

    public static void LogDebug(string message) => Log(LogLevel.Debug, message);
    public static void LogInfo(string message) => Log(LogLevel.Info, message);
    public static void LogWarn(string message) => Log(LogLevel.Warn, message);
    public static void LogError(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);
    public static void LogError(Exception ex) => Log(LogLevel.Error, string.Empty, ex);

    private static void Log(LogLevel level, string message, Exception? ex = null) {
        
        if (level < MinimumLevel)
            return;

        lock (Lock) {
            
            var (prefix, color) = GetPrefixAndColor(level);
            Console.ForegroundColor = color;
            
            Console.Write($"{DateTime.Now:d} {DateTime.Now:HH:mm:ss:ms} ");
            Console.Write(prefix);
            
            Console.ResetColor();
            if (message is not "")
                Console.WriteLine($" {message}");

            if (ex == null) 
                return;
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Exception] {ex.GetType().Name}: {ex.Message}");
            
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }

    private static (string Prefix, ConsoleColor Color) GetPrefixAndColor(LogLevel level) {
        
        return level switch {
            LogLevel.Debug => ("[DEBUG]", ConsoleColor.Cyan),
            LogLevel.Info  => ("[INFO ]", ConsoleColor.Green),
            LogLevel.Warn  => ("[WARN ]", ConsoleColor.Yellow),
            LogLevel.Error => ("[ERROR]", ConsoleColor.Red),
            _ => ("[UNKNOWN]", ConsoleColor.Gray)
        };
    }
}
