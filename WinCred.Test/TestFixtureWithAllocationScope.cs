﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace WinCred.Test;

public abstract class TestFixtureWithAllocationScope
{
    private static readonly ConcurrentDictionary<Type, AllocationScope> TestAllocScopes = new();
    private AllocationScope _allocScope;

    public static void OneTimeSetup(Type t)
    {
        if (t is null) throw new ArgumentNullException(nameof(t));
        if (!TestAllocScopes.TryAdd(t, AllocationScope.Create(t.FullName!)))
            throw new InvalidOperationException("Concurrent allocation scope with the same test not allowed!");
    }

    public static void OneTimeTeardown(Type t)
    {
        if (t is null) throw new ArgumentNullException(nameof(t));
        if (TestAllocScopes.TryRemove(t, out var scope))
            scope.Dispose();
    }

    [SetUp]
    public virtual void Setup()
    {
        var t = TestContext.CurrentContext.Test;
        _allocScope = AllocationScope.Create(t.FullName);
    }

    [TearDown]
    public virtual unsafe void Teardown()
    {
        TestContext.WriteLine("Before collecting garbage;");
        var sb = new StringBuilder();
        TestContext.Write(_allocScope.Report(sb, out var bytesAllocated, out var bytesFreed).ToString());
        TestContext.WriteLine();
        
        if (bytesAllocated == bytesFreed)
        {
            TestContext.WriteLine("No leaks detected.");
            return;
        }

        TestContext.WriteLine("Collecting garbage...");
        var started = Stopwatch.GetTimestamp();
        for (var i = 0; i < 3; ++i)
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        var elapsed = Stopwatch.GetElapsedTime(started);
        TestContext.WriteLine($"Garbage collection took {elapsed.TotalSeconds:F3}s");
        TestContext.WriteLine();

        TestContext.WriteLine("After collecting garbage;");
        sb.Clear();
        TestContext.Write(_allocScope.Report(sb, out bytesAllocated, out bytesFreed).ToString());
        _allocScope.ForAllOutstanding(static p => MemoryHelpers.Free((void*) p));
        _allocScope.Dispose();

        TestContext.WriteLine(bytesAllocated == bytesFreed
            ? "No leaks detected."
            : $"Memory leak detected: {bytesAllocated - bytesFreed} bytes");
    }
}