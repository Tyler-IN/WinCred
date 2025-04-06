using System.ComponentModel;

namespace WinCred;

[ExcludeFromCodeCoverage]
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
    internal static void Generic(string message,
        [CallerFilePath]
        string? file = null,
        [CallerLineNumber]
        int line = 0)
        => throw new Exception(message) {Data = {["File"] = file, ["Line"] = line}};

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint HResultFromWin32ErrorCode(uint errorCode)
    {
        return (errorCode & 0x80000000u) == 0
            ? (errorCode & 0x0000FFFFu) | 0x80070000u
            : errorCode;
    }

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    internal static void ExceptionFromHResult(uint hr)
        => throw Marshal.GetExceptionForHR(unchecked((int) hr));

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    internal static void Win32Exception(uint errorCode)
        => ExceptionFromHResult(HResultFromWin32ErrorCode(errorCode));

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    internal static void LastWin32Exception(bool fromPInvoke = true)
        => Win32Exception(WindowsHelpers.GetLastError(fromPInvoke));
}