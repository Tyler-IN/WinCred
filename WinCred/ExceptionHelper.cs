namespace WinCred;

public static class ExceptionHelper
{
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    internal static void ArgumentException(string name, string message,
        [CallerFilePath]
        string? file = null,
        [CallerLineNumber]
        int line = 0)
        => throw new ArgumentException(message, name) {Data = {["File"] = file, ["Line"] = line}};

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    public static void ArgumentNull(string name,
        [CallerFilePath]
        string? file = null,
        [CallerLineNumber]
        int line = 0)
        => throw new ArgumentNullException(name) {Data = {["File"] = file, ["Line"] = line}};

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    internal static void ArgumentOutOfRange(string name, string message,
        [CallerFilePath]
        string? file = null,
        [CallerLineNumber]
        int line = 0)
        => throw new ArgumentOutOfRangeException(name, message) {Data = {["File"] = file, ["Line"] = line}};

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    internal static void InvalidOperation(string message,
        [CallerFilePath]
        string? file = null,
        [CallerLineNumber]
        int line = 0)
        => throw new InvalidOperationException(message) {Data = {["File"] = file, ["Line"] = line}};

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    internal static void ObjectDisposed(string name, string message,
        [CallerFilePath]
        string? file = null,
        [CallerLineNumber]
        int line = 0)
        => throw new ObjectDisposedException(name, message) {Data = {["File"] = file, ["Line"] = line}};

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    public static void Generic(string message,
        [CallerFilePath]
        string? file = null,
        [CallerLineNumber]
        int line = 0)
        => throw new Exception(message) {Data = {["File"] = file, ["Line"] = line}};
}