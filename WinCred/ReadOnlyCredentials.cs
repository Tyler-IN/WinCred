namespace WinCred;

[PublicAPI]
public sealed unsafe class ReadOnlyCredentials : IDisposable
{
    public static readonly ReadOnlyCredentials Empty = new(null, 0);

    private CREDENTIAL** _credentials;
    private readonly uint _count;

    public ReadOnlySpan<ReadOnlyPointer<CREDENTIAL>> Span
        => new(_credentials, (int) _count);

    public ReadOnlyCredentials(CREDENTIAL** credentials, uint count)
    {
        _credentials = credentials;
        _count = count;
    }

    ~ReadOnlyCredentials()
    {
        if (_credentials is null) return;
        Dispose();
    }

    public void Dispose()
    {
        if (_credentials is null) return;
        GC.SuppressFinalize(this);
        AdvApi32.CredFree(_credentials);
        _credentials = null;
    }

    /// <summary>
    /// Index operator for <see cref="ReadOnlyCredentials"/>.
    /// </summary>
    /// <remarks>
    /// Delegates indexing exceptions to <see cref="Span"/>'s indexer method.
    /// </remarks>
    /// <param name="index">The index of the credential to retrieve.</param>
    public ref readonly CREDENTIAL this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Span[index].Target;
    }

    /// <summary>
    /// Index operator for <see cref="ReadOnlyCredentials"/>.
    /// </summary>
    /// <remarks>
    /// Delegates indexing exceptions to <see cref="Span"/>'s indexer method.
    /// </remarks>
    /// <param name="index">The index of the credential to retrieve.</param>
    public ref readonly CREDENTIAL this[Index index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Span[index].Target;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(Span);
    }

    public ref struct Enumerator
    {
        private readonly ReadOnlySpan<ReadOnlyPointer<CREDENTIAL>> _span;
        private int _index;

        internal Enumerator(ReadOnlySpan<ReadOnlyPointer<CREDENTIAL>> span)
        {
            _span = span;
            _index = -1;
        }

        public ref readonly CREDENTIAL Current
            => ref _span[_index].Target;

        public bool MoveNext()
        {
            if (_index >= _span.Length - 1)
                return false;
            _index++;
            return true;
        }
    }
}