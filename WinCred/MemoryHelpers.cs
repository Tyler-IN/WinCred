namespace WinCred;

[PublicAPI]
[SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
public static class MemoryHelpers
{
    public static unsafe void Clear(byte* start, nuint size)
    {
        if (size > uint.MaxValue)
        {
            while (size > uint.MaxValue)
            {
                Unsafe.InitBlockUnaligned(start, 0, uint.MaxValue);
                start += uint.MaxValue;
                size -= uint.MaxValue;
            }
        }

        // ReSharper disable once RedundantOverflowCheckingContext
        Unsafe.InitBlockUnaligned(start, 0, unchecked((uint) size));
    }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Clear<T>(T* start, nuint count = 1)
        => Clear((byte*) start, count * (uint) sizeof(T));

    public unsafe ref struct ChunkEnumerator<T>
    {
        private readonly T* _start;
        private readonly nuint _size;
        private readonly int _chunkSize;
        private T* _current;

        public ChunkEnumerator(T* start, nuint size, int chunkSize)
        {
            if (chunkSize <= 0)
                ExceptionHelper.ArgumentOutOfRange(nameof(chunkSize), "Must be positive.");
            _start = start;
            _size = size;
            _chunkSize = chunkSize;
            _current = null;
        }

        public bool MoveNext()
        {
            if (_current == null)
                _current = _start;
            else
                _current += _chunkSize;
            return (nuint) (_current - _start) < _size;
        }

        public Span<T> Current
        {
            get
            {
                if (_current == null)
                    ExceptionHelper.InvalidOperation(
                        "The enumerator is not positioned within the memory range."
                        + " (Did you forget to call MoveNext() before touching Current?)");

                var current = _current;
                var size = Min((nuint) _chunkSize, _size - ((nuint) current - (nuint) _start));
                return new Span<T>(current, (int) size);
            }
        }

        private static nuint Min(nuint a, nuint b) => a < b ? a : b;
    }

    public static unsafe ChunkEnumerator<T> Chunk<T>(T* start, nuint size, int chunkSize = int.MaxValue)
        => new(start, size, chunkSize);

    public static unsafe T* New<T>(
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        var ptr = (T*) Marshal.AllocHGlobal(sizeof(T));
        if (!AllocationScope.TrackAlloc(ptr, 1, file, line))
            ExceptionHelper.Generic("Failed to track allocation!");
        return ptr;
    }

    public static unsafe Span<T> New<T>(int count,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        if (count <= 0)
            ExceptionHelper.ArgumentOutOfRange(nameof(count), "Count must be positive.");
        var ptr = (T*) Marshal.AllocHGlobal(count * sizeof(T));
        if (!AllocationScope.TrackAlloc(ptr, (uint) count, file, line))
            ExceptionHelper.Generic("Failed to track allocation!");
        return new Span<T>(ptr, count);
    }

    public static unsafe Span<T> New<T>(out T* ptr, int count,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        if (count <= 0)
            ExceptionHelper.ArgumentOutOfRange(nameof(count), "Count must be positive.");
        ptr = (T*) Marshal.AllocHGlobal(count * sizeof(T));
        if (!AllocationScope.TrackAlloc(ptr, (uint) count, file, line))
            ExceptionHelper.Generic("Failed to track allocation!");
        return new Span<T>(ptr, count);
    }

    public static ReadOnlySpan<T> Duplicate<T>(ReadOnlySpan<T> span,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        if (span.IsEmpty)
            return ReadOnlySpan<T>.Empty;
        var dup = New<T>(span.Length, file, line);
        span.CopyTo(dup);
        return dup;
    }

    public static unsafe ReadOnlySpan<T> NewCopy<T>(ReadOnlySpan<T> span, int newLength,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
        => NewCopy(span, newLength, out _, file, line);

    public static unsafe ReadOnlySpan<T> NewCopy<T>(ReadOnlySpan<T> span, int newLength, out T* pNew,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        pNew = null;
        if (span.IsEmpty)
        {
            return newLength <= 0
                ? ReadOnlySpan<T>.Empty
                : New(out pNew, newLength, file, line);
        }

        switch (newLength)
        {
            case 0:
                return ReadOnlySpan<T>.Empty;
            case < 0:
                ExceptionHelper.ArgumentOutOfRange(nameof(newLength), "New length must be positive.");
                break;
        }

        if (span.Length == newLength)
            return span;

        fixed (T* pOld = span)
        {
            New(out pNew, newLength, file, line);
            Unsafe.CopyBlockUnaligned(pNew, pOld,
                (uint) (sizeof(T) * span.Length));
            return new ReadOnlySpan<T>(pNew, newLength);
        }
    }

    public static unsafe void Free(void* p,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        AllocationScope.TrackFree(p, file, line);
        Marshal.FreeHGlobal((nint) p);
    }


    public static unsafe void FreeIfNotNull(void* p,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        if (p is null) return;
        AllocationScope.TrackFree(p, file, line);
        Marshal.FreeHGlobal((nint) p);
    }

    public static unsafe void FreeAndNull<T>(ref T* p,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        Free(p, file, line);
        p = default!;
    }

    public static unsafe void Free<T>(T* ptr,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        AllocationScope.TrackFree(ptr, file, line);
        Marshal.FreeHGlobal((nint) ptr);
    }

    public static unsafe void Free<T>(ReadOnlySpan<T> span,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        if (span.IsEmpty) return;
        fixed (T* p = span) Free(p, file, line);
    }

    public static unsafe void Deconstruct<T>(this Span<T> span, out T* p, out int size)
    {
        size = span.Length;
        fixed (T* pSpan = span) p = pSpan;
    }

    public static unsafe void Deconstruct<T>(this ReadOnlySpan<T> span, out T* p, out int size)
    {
        size = span.Length;
        fixed (T* pSpan = span) p = pSpan;
    }
#pragma warning restore CS8500


    public static unsafe ReadOnlySpan<byte> NullTerminatedToReadOnlySpan(byte* pChars)
    {
        if (pChars is null)
            return ReadOnlySpan<byte>.Empty;
        var length = NullTerminatedLength(pChars);
        return length == 0
            ? ReadOnlySpan<byte>.Empty
            : new ReadOnlySpan<byte>(pChars, length);
    }

    public static unsafe ReadOnlySpan<char> NullTerminatedToReadOnlySpan(char* pChars)
    {
        if (pChars is null)
            return ReadOnlySpan<char>.Empty;
        var length = NullTerminatedLength(pChars);
        return length == 0
            ? ReadOnlySpan<char>.Empty
            : new ReadOnlySpan<char>(pChars, length);
    }

    public static unsafe ReadOnlySpan<byte> NullTerminatedToReadOnlySpan(byte* pChars, int max)
    {
        if (pChars is null)
            return ReadOnlySpan<byte>.Empty;
        var length = NullTerminatedLength(pChars, max);
        return length == 0
            ? ReadOnlySpan<byte>.Empty
            : new ReadOnlySpan<byte>(pChars, length);
    }


    public static unsafe ReadOnlySpan<char> NullTerminatedToReadOnlySpan(char* pChars, int max)
    {
        if (pChars is null)
            return ReadOnlySpan<char>.Empty;
        var length = NullTerminatedLength(pChars, max);
        return length == 0
            ? ReadOnlySpan<char>.Empty
            : new ReadOnlySpan<char>(pChars, length);
    }

    public static unsafe ReadOnlySpan<byte> DuplicateNullTerminated(byte* pChars)
    {
        var length = unchecked((uint) NullTerminatedLength(pChars));
        //var pNewChars = (byte*) Marshal.AllocHGlobal((nint) length + 1);
        New<byte>(out var pNewChars, (int) length + 1);
        Unsafe.CopyBlockUnaligned(pNewChars, pChars, length);
        pNewChars[length] = 0;
        return new ReadOnlySpan<byte>(pNewChars, (int) length);
    }

    public static unsafe ReadOnlySpan<char> DuplicateNullTerminated(char* pChars)
    {
        var length = unchecked((uint) NullTerminatedLength(pChars));
        //var pNewChars = (char*) Marshal.AllocHGlobal((nint) length + 1);
        New<char>(out var pNewChars, (int) length + 1);
        Unsafe.CopyBlockUnaligned(pNewChars, pChars,
            length * sizeof(char));
        pNewChars[length] = default;
        return new ReadOnlySpan<char>(pNewChars, (int) length);
    }

    public static unsafe ReadOnlySpan<byte> DuplicateNullTerminated(byte* pChars, int max, bool includeNull = false)
    {
        var length = unchecked((uint) NullTerminatedLength(pChars, max));
        //var pNewChars = (byte*) Marshal.AllocHGlobal((nint) length + 1);
        New<byte>(out var pNewChars, (int) length + 1);
        Unsafe.CopyBlockUnaligned(pNewChars, pChars, length);
        pNewChars[length] = 0;
        var newCharsLength = (int) (includeNull ? length + 1 : length);
        return new ReadOnlySpan<byte>(pNewChars, newCharsLength);
    }

    public static unsafe ReadOnlySpan<char> DuplicateNullTerminated(char* pChars, int max, bool includeNull = false,
        [CallerFilePath]
        string? file = "",
        [CallerLineNumber]
        int line = 0)
    {
        if (pChars is null)
            return ReadOnlySpan<char>.Empty;
        switch (max)
        {
            case 0:
                return ReadOnlySpan<char>.Empty;
            case < 0:
                ExceptionHelper.ArgumentOutOfRange(nameof(max), "Maximum length must be non-negative.");
                break;
        }

        var length = unchecked((uint) NullTerminatedLength(pChars, max));
        //var pNewChars = (char*) Marshal.AllocHGlobal((nint) length + 1);
        New<char>(out var pNewChars, (int) length + 1, file, line);
        Unsafe.CopyBlockUnaligned(pNewChars, pChars,
            length * sizeof(char));
        pNewChars[length] = default;
        var newCharsLength = (int) (includeNull ? length + 1 : length);
        return new ReadOnlySpan<char>(pNewChars, newCharsLength);
    }

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
#pragma warning disable CS9081 // A result of a stackalloc expression of this type in this context may be exposed outside of the containing method
    public static unsafe ReadOnlySpan<byte> Utf8(string? credentials, bool nullTerminate = true)
    {
        if (string.IsNullOrEmpty(credentials))
            return ReadOnlySpan<byte>.Empty;
        var chars = (ReadOnlySpan<char>) credentials;
        var byteCount = Encoding.UTF8.GetByteCount(chars);
        var nullTermSize = nullTerminate ? 1 : 0;
        var onStackLimit = 2048 - nullTermSize;
        var onStack = byteCount < onStackLimit;
        var bytes = onStack
            ? stackalloc byte[byteCount + nullTermSize]
            : New<byte>(byteCount + nullTermSize);
        var (pBytes, _) = bytes;
        var (pChars, charCount) = chars;
        var wroteBytes = Encoding.UTF8.GetBytes(pChars, charCount, pBytes, byteCount);
        pBytes[wroteBytes + nullTermSize] = 0; // null terminate
        return onStack
            ? NewCopy<byte>(bytes, wroteBytes + 1)
            : bytes;
    }
#pragma warning restore CS9081
#pragma warning restore CS9080
    public static unsafe ReadOnlySpan<char> NullTerminateByCopy(string? target)
    {
        if (string.IsNullOrEmpty(target))
            return ReadOnlySpan<char>.Empty;
        if (target[^1] == 0)
            return target;
        var copy = NewCopy<char>(target, target.Length + 1);
        var (pCopy, copyLength) = copy;
        pCopy[copyLength - 1] = default; // null terminate
        return copy;
    }

    public static unsafe ReadOnlySpan<char> NullTerminateByCopy(ReadOnlySpan<char> target)
    {
        if (target.IsEmpty)
            return ReadOnlySpan<char>.Empty;
        if (target[^1] == 0)
            return target;
        var copy = NewCopy(target, target.Length + 1);
        var (pCopy, copyLength) = copy;
        pCopy[copyLength - 1] = default; // null terminate
        return copy;
    }

    public static unsafe string? NullTerminatedToString(byte* bytes, int maxSize, bool utf8 = true)
    {
        if (bytes is null)
            return null;
        var length = NullTerminatedLength(bytes, maxSize);
        if (length == 0)
            return string.Empty;

        return string.Create(length, ((nint) bytes, maxSize, utf8), static (chars, x) =>
        {
            var (bytes, maxSize, utf8) = x;
            var pBytes = (byte*) bytes;
            var (pChars, charsLength) = chars;
            var bytesLength = NullTerminatedLength(pBytes, maxSize);
            if (utf8)
                Encoding.UTF8.GetChars(pBytes, bytesLength, pChars, charsLength);
            else
                Unsafe.CopyBlockUnaligned(pChars, pBytes, unchecked((uint) bytesLength));
        });
    }

    private static readonly int PageSize
        = Environment.SystemPageSize;

    public static unsafe int NullTerminatedLength(byte* str)
    {
        if (str == null)
            ExceptionHelper.ArgumentNull(nameof(str));

        var current = str;
        while (true)
        {
            // Calculate the next page boundary.
            var nextBoundary = (byte*) (((ulong) current + (ulong) PageSize)
                                        & ~((ulong) PageSize - 1));

            // Seek within the current page.
            if (ScanToNullTerminator(ref current, nextBoundary))
                return (int) (current - str);

            // Check if the memory at the new page is accessible.
            if (WindowsHelpers.IsPageValid(current))
                continue;

            // Next page is not valid.
            break;
        }

        return (int) (current - str);
    }

    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    public static unsafe int NullTerminatedLength(byte* str, int max)
    {
        if (str == null)
            ExceptionHelper.ArgumentNull(nameof(str));
        if (max < 0)
            ExceptionHelper.ArgumentOutOfRange(nameof(max), "Maximum length must be non-negative.");

        var current = str;
        var maxPtr = current + max;

        while (current < maxPtr)
        {
            // Calculate the next page boundary.
            var nextBoundary = (byte*) (((ulong) current + (ulong) PageSize)
                                        & ~((ulong) PageSize - 1));
            if (nextBoundary > maxPtr)
                nextBoundary = maxPtr;

            // Seek within the current page.
            if (ScanToNullTerminator(ref current, nextBoundary))
                return (int) (current - str);

            // If we've reached the max, stop.
            if (current >= maxPtr)
                break;

            // Check if the memory at the new page is accessible.
            if (WindowsHelpers.IsPageValid(current))
                continue;

            // Next page is not valid.
            break;
        }

        return (int) (current - str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool ScanToNullTerminator
        (ref byte* str, byte* limit)
    {
        var index = new ReadOnlySpan<byte>(str, (int) (limit - str)).IndexOf((byte) 0);
        if (index == -1)
        {
            str = limit;
            return false;
        }

        str += index;
        return true;
    }

    public static unsafe int NullTerminatedLength(char* str)
    {
        if (str == null)
            ExceptionHelper.ArgumentNull(nameof(str));

        var current = str;
        while (true)
        {
            // Compute the next page boundary for the underlying byte pointer.
            var currentByte = (byte*) current;
            var nextByteBoundary = (byte*) (((nuint) currentByte + (nuint) PageSize)
                                            & ~((nuint) PageSize - 1));
            // Cast back to char*.
            var nextBoundary = (char*) nextByteBoundary;

            // Seek within the current page.
            if (ScanToNullTerminator(ref current, nextBoundary))
                return (int) (current - str);

            // Check if the memory at the new page is accessible.
            if (WindowsHelpers.IsPageValid(currentByte))
                continue;

            // Next page is not valid.
            break;
        }

        return (int) (current - str);
    }

    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    public static unsafe int NullTerminatedLength(char* str, int max)
    {
        if (str == null)
            ExceptionHelper.ArgumentNull(nameof(str));
        if (max < 0)
            ExceptionHelper.ArgumentOutOfRange(nameof(max), "Maximum length must be non-negative.");

        var current = str;
        var maxPtr = str + max;

        while (current < maxPtr)
        {
            // Compute the next page boundary for the underlying byte pointer.
            var currentByte = (byte*) current;

            var nextByteBoundary = (byte*) (((nuint) currentByte + (nuint) PageSize)
                                            & ~((nuint) PageSize - 1));

            var nextBoundary = (char*) nextByteBoundary;
            if (nextBoundary > maxPtr)
                nextBoundary = maxPtr;

            // Seek within the current page.
            if (ScanToNullTerminator(ref current, nextBoundary))
                return (int) (current - str);

            // If we've reached the max, stop.
            if (current >= maxPtr)
                break;

            // Check if the memory at the new page is accessible.
            if (WindowsHelpers.IsPageValid(currentByte))
                continue;

            // Next page is not valid.
            break;
        }

        return (int) (current - str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe bool ScanToNullTerminator(ref char* str, char* limit)
    {
        var index = new ReadOnlySpan<char>(str, (int) (limit - str))
            .IndexOf('\0');
        if (index == -1)
        {
            str = limit;
            return false;
        }

        str += index;
        return true;
    }
}