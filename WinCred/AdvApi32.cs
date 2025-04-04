namespace WinCred;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static unsafe partial class AdvApi32
{
    private const string Library = "advapi32";

    [Flags]
    private enum CredEnumerateFlags : uint
    {
        Default = 0,
        AllCredentials = 1,
    }

    [DllImport(Library, EntryPoint = "CredWriteW", SetLastError = true)]
    private static extern bool _CredWrite(CREDENTIAL* Credential, CredInputFlags Flags);

    [DllImport(Library, EntryPoint = "CredReadW", SetLastError = true)]
    private static extern bool _CredRead(char* TargetName, CredType Type, uint Flags, CREDENTIAL** Credential);

    [DllImport(Library, EntryPoint = "CredDeleteW", SetLastError = true)]
    private static extern bool _CredDelete(char* TargetName, CredType Type, uint Flags);

    [DllImport(Library, EntryPoint = "CredEnumerateW", SetLastError = true)]
    private static extern bool _CredEnumerate(char* Filter, CredEnumerateFlags Flags, uint* Count,
        CREDENTIAL*** Credentials);

    [DllImport(Library, EntryPoint = "CredFree")]
    internal static extern void CredFree(void* Buffer);

    internal static bool CredWrite(in CREDENTIAL credential, CredInputFlags flags = CredInputFlags.None)
        => _CredWrite((CREDENTIAL*) Unsafe.AsPointer(ref Unsafe.AsRef(credential)), flags);

    public static bool CredWrite(Credential credential, CredInputFlags flags = CredInputFlags.None)
        => CredWrite(in credential.Data, flags);

    private static bool CredRead(ReadOnlySpan<char> targetName, CredType type, out CREDENTIAL* credential)
    {
        if (!targetName.IsEmpty && targetName[^1] != 0)
            ExceptionHelper.ArgumentException(nameof(targetName), "Must be null-terminated.");
        fixed (char* tn = targetName)
        fixed (CREDENTIAL** pc = &credential)
            return _CredRead(tn, type, 0, pc);
    }

    [MustDisposeResource, MustUseReturnValue]
    public static ReadOnlyCredential? CredRead(ReadOnlySpan<char> targetName, CredType type)
    {
        var success = CredRead(targetName, type, out var credential);
        if (!success || credential is null)
            return null;
        return new ReadOnlyCredential(credential);
    }

    public static bool TryRead(ReadOnlySpan<char> targetName, CredType type,
        [NotNullWhen(true), MustDisposeResource]
        out ReadOnlyCredential? credential)
    {
        var success = CredRead(targetName, type, out var cred);
        if (!success || cred is null)
        {
            credential = null;
            return false;
        }

        credential = new ReadOnlyCredential(cred);
        return true;
    }

    public static bool CredDelete(ReadOnlySpan<char> targetName, CredType type)
    {
        if (!targetName.IsEmpty && targetName[^1] != 0)
            ExceptionHelper.ArgumentException(nameof(targetName), "Must be null-terminated.");
        fixed (char* tn = targetName)
            return _CredDelete(tn, type, 0);
    }

    private static bool CredEnumerate(ReadOnlySpan<char> filter, out uint count, ref CREDENTIAL** credentials)
    {
        if (!filter.IsEmpty && filter[^1] != 0)
            ExceptionHelper.ArgumentException(nameof(filter), "Must be null-terminated.");
        fixed (char* f = filter)
        fixed (uint* pCount = &count)
        fixed (CREDENTIAL*** pCredentials = &credentials)
            return _CredEnumerate(f, CredEnumerateFlags.Default, pCount, pCredentials);
    }

    private static bool CredEnumerateAll(out uint count, ref CREDENTIAL** credentials)
    {
        fixed (uint* pCount = &count)
        fixed (CREDENTIAL*** pCredentials = &credentials)
            return _CredEnumerate(null, CredEnumerateFlags.AllCredentials, pCount, pCredentials);
    }

    private static ReadOnlySpan<ReadOnlyPtr<CREDENTIAL>> CredEnumerate(ReadOnlySpan<char> filter, out uint count)
    {
        CREDENTIAL** credentials = null;
        var success = CredEnumerate(filter, out count, ref credentials);
        if (!success || credentials is null)
            return ReadOnlySpan<ReadOnlyPtr<CREDENTIAL>>.Empty;
        return new ReadOnlySpan<ReadOnlyPtr<CREDENTIAL>>(credentials, (int) count);
    }

    private static ReadOnlySpan<ReadOnlyPtr<CREDENTIAL>> CredEnumerateAll(out uint count)
    {
        CREDENTIAL** credentials = null;
        var success = CredEnumerateAll(out count, ref credentials);
        if (!success || credentials is null)
            return ReadOnlySpan<ReadOnlyPtr<CREDENTIAL>>.Empty;
        return new ReadOnlySpan<ReadOnlyPtr<CREDENTIAL>>(credentials, (int) count);
    }

    public static ReadOnlyCredentials? CredEnumerate(ReadOnlySpan<char> filter)
    {
        CREDENTIAL** credentials = null;
        var success = CredEnumerate(filter, out var count, ref credentials);
        if (!success || credentials is null)
            return null;
        return new ReadOnlyCredentials(credentials, count);
    }

    public static ReadOnlyCredentials? CredEnumerate()
    {
        CREDENTIAL** credentials = null;
        var success = CredEnumerateAll(out var count, ref credentials);
        if (!success || credentials is null)
            return null;
        return new ReadOnlyCredentials(credentials, count);
    }

    internal static void CredFree(ref CREDENTIAL credential)
    {
        if (Unsafe.IsNullRef(ref credential)) return;
        CredFree(Unsafe.AsPointer(ref credential));
    }

    internal static void CredFree(ReadOnlySpan<CREDENTIAL> credentials)
    {
        if (credentials.IsEmpty) return;
        fixed (CREDENTIAL* p = credentials)
            CredFree(p);
    }
}