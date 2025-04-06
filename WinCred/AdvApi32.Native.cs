namespace WinCred;

internal static unsafe partial class AdvApi32
{
    private const string Library = "advapi32";

    [DllImport(Library, EntryPoint = "CredWriteW", SetLastError = true)]
    private static extern int _CredWrite(CREDENTIAL* credential, CredentialInputFlags flags);

    [DllImport(Library, EntryPoint = "CredReadW", SetLastError = true)]
    private static extern int _CredRead(char* targetName, CredentialType type, uint flags, CREDENTIAL** credential);

    [DllImport(Library, EntryPoint = "CredDeleteW", SetLastError = true)]
    private static extern int _CredDelete(char* targetName, CredentialType type, uint flags);

    [DllImport(Library, EntryPoint = "CredEnumerateW", SetLastError = true)]
    private static extern int _CredEnumerate(char* filter, CredentialEnumerationFlags flags, uint* count,
        CREDENTIAL*** credentials);

    [DllImport(Library, EntryPoint = "CredFree", SetLastError = false)]
    internal static extern void _CredFree(void* buffer);
}