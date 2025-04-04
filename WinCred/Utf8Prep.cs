namespace WinCred;

public readonly ref struct Utf8Prep
{
    public readonly ReadOnlySpan<char> Chars;
    public readonly int Utf8Length;

    public Utf8Prep(ReadOnlySpan<char> chars)
    {
        Chars = chars;
        Utf8Length = chars.IsEmpty
            ? 0
            : chars[^1] == 0
                ? Encoding.UTF8.GetByteCount(chars[..^1])
                : Encoding.UTF8.GetByteCount(chars);
    }

    public readonly unsafe ref T OnStack<T>(ReadOnlySpanActionWithRefState<byte, T> action, ref T state,
        bool nullTerminate = false)
    {
        var isNullTerminated = Chars[^1] == 0;
        if (isNullTerminated) nullTerminate = false;
        Span<byte> bytes = stackalloc byte[Utf8Length + (nullTerminate ? 1 : 0)];
        var (pBytes, _) = bytes;
        var (pChars, charCount) = Chars;
        var wroteBytes = Encoding.UTF8.GetBytes(pChars, charCount, pBytes, Utf8Length);
        if (nullTerminate)
            pBytes[wroteBytes] = 0; // null terminate
        action(bytes, ref state);
        return ref state;
    }

    public readonly unsafe ReadOnlySpan<byte> OnGcHeap(bool nullTerminate = false)
    {
        var isNullTerminated = Chars[^1] == 0;
        if (isNullTerminated) nullTerminate = false;
        var bytes = new byte[Utf8Length + (nullTerminate ? 1 : 0)];
        var (pChars, charCount) = Chars;
        fixed (byte* pBytes = bytes)
        {
            var wroteBytes = Encoding.UTF8.GetBytes(pChars, charCount, pBytes, Utf8Length);
            if (nullTerminate)
                pBytes[wroteBytes] = 0; // null terminate
        }

        return bytes;
    }


    public readonly unsafe ReadOnlySpan<byte> OnGlobalMem(bool nullTerminate = false)
    {
        var isNullTerminated = Chars[^1] == 0;
        if (isNullTerminated) nullTerminate = false;
        var length = Utf8Length + (nullTerminate ? 1 : 0);
        var bytes = MemoryHelpers.New<byte>(out var pBytes, length);
        var (pChars, charCount) = Chars;
        var wroteBytes = Encoding.UTF8.GetBytes(pChars, charCount, pBytes, Utf8Length);
        if (nullTerminate)
            pBytes[wroteBytes] = 0; // null terminate
        return bytes;
    }
}