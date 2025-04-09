namespace WinCred.Test;

public abstract class TestFixtureWithAllocationScope
{
    private static readonly ConcurrentDictionary<Type, AllocationScope> TestAllocScopes = new();
    private AllocationScope _allocScope;

    [ExcludeFromCodeCoverage]
    public static void OneTimeSetup(Type t)
    {
        if (t is null) throw new ArgumentNullException(nameof(t));
        if (!TestAllocScopes.TryAdd(t, AllocationScope.Create(t.FullName!)))
            throw new InvalidOperationException("Concurrent allocation scope with the same test not allowed!");
    }

    [ExcludeFromCodeCoverage]
    public static void OneTimeTeardown(Type t)
    {
        if (t is null) throw new ArgumentNullException(nameof(t));
        if (TestAllocScopes.TryRemove(t, out var scope))
            scope.Dispose();
    }

    [SetUp]
    [ExcludeFromCodeCoverage]
    public virtual void Setup()
    {
        var t = TestContext.CurrentContext.Test;
        _allocScope = AllocationScope.Create(t.FullName);
    }

    [TearDown]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("ReSharper", "NUnitAssertMigration")]
    public virtual unsafe void Teardown()
    {
        TestContext.Out.WriteLine("Before collecting garbage;");
        var sb = new StringBuilder();
        TestContext.Out.Write(_allocScope.Report(sb, out var bytesAllocated, out var bytesFreed).ToString());
        TestContext.Out.WriteLine();
        
        if (bytesAllocated == bytesFreed)
        {
            TestContext.Out.WriteLine("No leaks detected.");
            return;
        }
        
        Assert.Warn($"Pre-finalization memory leak detected: {bytesAllocated - bytesFreed} bytes");

        TestContext.Out.WriteLine("Collecting garbage...");
        var started = Stopwatch.GetTimestamp();
        for (var i = 0; i < 3; ++i)
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        var elapsed = Stopwatch.GetElapsedTime(started);
        TestContext.Out.WriteLine($"Garbage collection took {elapsed.TotalSeconds:F3}s");
        TestContext.Out.WriteLine();

        TestContext.Out.WriteLine("After collecting garbage;");
        sb.Clear();
        TestContext.Out.Write(_allocScope.Report(sb, out bytesAllocated, out bytesFreed).ToString());
        _allocScope.ForAllOutstanding(static p => MemoryHelpers.Free((void*) p));
        _allocScope.Dispose();

        if (bytesAllocated == bytesFreed)
            TestContext.Out.WriteLine("No leaks detected.");
        else
            Assert.Fail($"Memory leak detected: {bytesAllocated - bytesFreed} bytes");
    }
}