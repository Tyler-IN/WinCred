namespace WinCred;

[PublicAPI]
public sealed unsafe class ReadOnlyCredential : IDisposable
{
    private CREDENTIAL* _credential;
    public ref readonly CREDENTIAL Value => ref Unsafe.AsRef<CREDENTIAL>(_credential);

    [MustDisposeResource]
    public ReadOnlyCredential(CREDENTIAL* credential) => _credential = credential;

    ~ReadOnlyCredential()
    {
        if (_credential is null) return;
        Dispose();
    }

    public void Dispose()
    {
        if (_credential is null) return;
        GC.SuppressFinalize(this);
        AdvApi32.CredFree(ref Unsafe.AsRef<CREDENTIAL>(_credential));
        _credential = null;
    }

    /// <summary>
    /// Creates a mutable copy of this read-only credential.
    /// </summary>
    /// <returns>A new Credential instance with the same data.</returns>
    [MustDisposeResource, MustUseReturnValue]
    public Credential CreateMutableCopy() => Value.CreateMutableCopy();
}