namespace WinCred;

/// <summary>
/// The CREDENTIAL_ATTRIBUTE structure contains an application-defined attribute of the credential.
/// An attribute is a keyword-value pair.
/// It is up to the application to define the meaning of the attribute.
/// </summary>
[PublicAPI, StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public unsafe struct CREDENTIAL_ATTRIBUTE
{
    /// <inheritdoc cref="Keyword"/>
    internal char* _keyword;

    /// <summary>
    /// Name of the application-specific attribute. Names should be of the form &lt;CompanyName&gt;_&lt;Name&gt;.
    /// </summary>
    /// <remarks>
    /// This member cannot be longer than CRED_MAX_STRING_LENGTH (256) characters.
    /// </remarks> 
    public readonly ReadOnlySpan<char> Keyword
        => _keyword is not null
            ? MemoryHelpers.NullTerminatedToReadOnlySpan(_keyword, Credential.MaximumStringSize)
            : ReadOnlySpan<char>.Empty;

    // Identifies characteristics of the credential attribute.
    // This member is reserved and should be originally initialized as zero and not otherwise altered to permit future enhancement.
    private uint _flags;

    /// <summary>
    /// Length of Value in bytes. This member cannot be larger than <see cref="Credential.MaximumValueSize"/> bytes.
    /// </summary>
    /// <remarks>
    /// Although the SDK defines this as a DWORD, for convenience, this is a signed int.
    /// </remarks>
    internal int _valueSize;

    /// <inheritdoc cref="Value"/>
    internal byte* _value;

    /// <summary>
    /// Data associated with the attribute.
    /// </summary>
    /// <remarks>
    /// <p>
    /// By convention, if Value is a text string, then Value should not include the trailing zero character and should be in UNICODE.
    /// </p>
    /// <p>
    /// Credentials are expected to be portable. The application should take care to ensure that the data in value is portable.
    /// </p>
    /// <p>
    /// It is the responsibility of the application to define the byte-endian and alignment of the data in Value.
    /// </p>
    /// </remarks>
    public readonly ReadOnlySpan<byte> Value => _value is not null
        ? new ReadOnlySpan<byte>(_value, _valueSize)
        : ReadOnlySpan<byte>.Empty;

    public readonly void CopyTo(ref CREDENTIAL_ATTRIBUTE target)
    {
        target._flags = _flags;
        
        var keywordSpan = MemoryHelpers.DuplicateNullTerminated
            (_keyword, Credential.MaximumStringSize, true);
        (target._keyword, _) = keywordSpan;
        
        var valueSpan = MemoryHelpers.Duplicate(Value);
        (target._value, target._valueSize) = valueSpan;
    }

    public override string ToString()
    {
        var pointer = (nuint)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
        return $"[CREDENTIAL_ATTRIBUTE @ 0x{(ulong) pointer:X8}]";
    }
}