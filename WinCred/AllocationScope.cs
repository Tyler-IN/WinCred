using System.Collections.Concurrent;
using System.Threading;

namespace WinCred;

public readonly struct Empty;

[PublicAPI, ExcludeFromCodeCoverage]
public sealed class AllocationScope : IDisposable
{
    [ThreadStatic]
    private static AllocationScope? _current;

    public static AllocationScope? Current => _current;

    public static ConcurrentDictionary<AllocationScope, Empty> ActiveScopes { get; } = new();


    private readonly string _name;
    private readonly bool _isolate;
    private readonly AllocationScope? _ancestor;
    private readonly ConcurrentDictionary<nint, AllocationRecord> _outstandingAllocs = new();
    private readonly ConcurrentBag<AllocationRecord> _freedAllocs = new();

    public static void Clear()
    {
        _current = null;
    }

    private AllocationScope(string name, bool isolate = false)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (name is null) ExceptionHelper.ArgumentNull(nameof(name));

        _name = name;
        _ancestor = _current;
        _isolate = isolate;
        _current = this;
        ActiveScopes.TryAdd(this, default);
    }

    public static AllocationScope Create(string name, bool isolate = false)
        => new(name, isolate);

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    private unsafe bool _Track(void* p, nuint size, Type? type, string? file, int line)
    {
        if (p is null) ExceptionHelper.ArgumentNull(nameof(p));
        if (size <= 0) ExceptionHelper.ArgumentOutOfRange(nameof(size), "Size must be positive.");
        var tracked = _outstandingAllocs.TryAdd((nint) p, new(p, size, type, file, line));
        if (!tracked) return false;
        if (!_isolate && _ancestor is not null)
            return _ancestor._Track(p, size, type, file, line);
        return true;
    }

    private static bool IsFinalizer()
    {
        if (Thread.CurrentThread.Name != "Finalizer")
            return false;

        var st = new StackTrace();
        var frames = st.GetFrames();
        if (frames is null || frames.Length == 0)
            return false;

        var lastFrame = frames[^1];
        return lastFrame.GetMethod() is {Name: "Finalize"};
    }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public static unsafe bool TrackAlloc<T>(T* p, nuint count, string? file, int line)
    {
        var current = _current;
        if (current is null)
        {
            if (!IsFinalizer())
                // untracked
                return true;

            ExceptionHelper.InvalidOperation("Do not call TrackAlloc from the finalizer thread.");
            return false;
        }

        var type = typeof(T);
        var size = (nuint) sizeof(T) * count;
        return current._Track(p, size, type, file, line);
    }

#pragma warning restore CS8500

    private unsafe void _TrackFree(void* p, string? file, int line)
    {
        var found = _outstandingAllocs.TryRemove((nint) p, out var record);
        if (!found)
            throw new Exception("Tracking of free failed; outstanding allocation not found.")
                {Data = {["File"] = file, ["Line"] = line}};
        record.Free(file, line);
        _freedAllocs.Add(record);

        _ancestor?._TrackFree(p, file, line);
    }

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    public static unsafe void TrackFree(void* p, string? file, int line)
    {
        if (p is null) ExceptionHelper.ArgumentNull(nameof(p));

        var current = _current;
        if (current == null)
        {
            if (!IsFinalizer())
                // untracked
                return;

            foreach (var scope in ActiveScopes.Keys)
            {
                foreach (var allocPtr in scope._outstandingAllocs.Keys)
                {
                    if (allocPtr != (nint) p) continue;
                    scope._TrackFree(p, file, line);
                    return;
                }
            }

            ExceptionHelper.Generic("Cannot find previously allocated pointer.");
            return;
        }

        current._TrackFree(p, file, line);
    }

    ~AllocationScope()
        => Dispose();

    public void Dispose()
    {
        if (_current != this)
            return;
        GC.SuppressFinalize(this);

        _current = _ancestor;
        ActiveScopes.TryRemove(this, out _);
    }

    public void ForAllOutstanding(Action<nint> fn)
    {
        foreach (var p in _outstandingAllocs.Keys)
            fn(p);
    }

    public override string ToString()
        => Report(new StringBuilder(), out _, out _).ToString();

    public StringBuilder Report(StringBuilder sb, out long bytesAllocated, out long bytesFreed)
    {
        bytesAllocated = 0;
        bytesFreed = 0;
        var freed = 0;
        sb.AppendLine($"Allocation Report {_name}");
        sb.AppendLine("  Freed:");
        foreach (var alloc in _freedAllocs)
        {
            sb.Append("    ");
            sb.Append(in alloc);
            sb.AppendLine();
            var size = (int) alloc.Size;
            bytesAllocated += size;
            bytesFreed += size;
            freed++;
        }

        sb.AppendLine("  Outstanding:");
        foreach (var (_, alloc) in _outstandingAllocs)
        {
            sb.Append("    ");
            sb.Append(in alloc);
            sb.AppendLine();
            bytesAllocated += (int) alloc.Size;
        }

        sb.AppendLine($"{bytesAllocated} bytes allocated, {bytesFreed} bytes freed");
        var total = _freedAllocs.Count + _outstandingAllocs.Count;
        var outstanding = total - freed;
        sb.AppendLine($"{freed} of {total} freed ({outstanding} outstanding)");
        return sb;
    }
}