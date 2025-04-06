namespace WinCred;

[PublicAPI]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class CREDENTIAL_ATTRIBUTE_Helper
{
    /// <summary>
    /// Sets the <see cref="CREDENTIAL_ATTRIBUTE.Keyword"/> of the credential attribute.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL_ATTRIBUTE"/> struct.
    /// </remarks>
    /// <param name="ca">The relevant credential attribute.</param>
    /// <param name="value">The keyword to set.</param>
    public static unsafe ref CREDENTIAL_ATTRIBUTE SetKeyword(ref this CREDENTIAL_ATTRIBUTE ca,
        ReadOnlySpan<char> value)
    {
        if (value.Length > Credential.MaximumStringSize)
            ExceptionHelper.ArgumentOutOfRange(nameof(value), "Keyword size exceeds maximum size.");

        if (ca._keyword is not null) MemoryHelpers.Free(ca._keyword);
        if (value.IsEmpty)
        {
            ca._keyword = null;
            return ref ca;
        }

        var length = value.Length;
        var needsNullTerminator = value[^1] != 0;
        var (pCopy, _) = MemoryHelpers.NewCopy(value, length + (needsNullTerminator ? 1 : 0));
        if (needsNullTerminator)
            pCopy[length] = default; // null terminate
        ca._keyword = pCopy;
        return ref ca;
    }

    /// <summary>
    /// Sets the <see cref="CREDENTIAL_ATTRIBUTE.Value"/> of the credential attribute.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL_ATTRIBUTE"/> struct.
    /// </remarks>
    /// <param name="ca">The relevant credential attribute.</param>
    /// <param name="value">The value to set.</param>
    private static unsafe ref CREDENTIAL_ATTRIBUTE SetValueInternal(ref this CREDENTIAL_ATTRIBUTE ca,
        ReadOnlySpan<byte> value)
    {
        if (value.Length > Credential.MaximumValueSize)
            ExceptionHelper.ArgumentOutOfRange(nameof(value), "Value size exceeds maximum size.");

        if (ca._value is not null) MemoryHelpers.Free(ca._value);
        if (value.IsEmpty)
        {
            ca._value = null;
            ca._valueSize = 0;
            return ref ca;
        }

        (ca._value, ca._valueSize) = MemoryHelpers.Duplicate(value);
        return ref ca;
    }

    /// <summary>
    /// Sets the <see cref="CREDENTIAL_ATTRIBUTE.Value"/> of the credential attribute.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL_ATTRIBUTE"/> struct.
    /// </remarks>
    /// <param name="ca">The relevant credential attribute.</param>
    /// <param name="value">The value to set.</param>
    public static ref CREDENTIAL_ATTRIBUTE SetValue(ref this CREDENTIAL_ATTRIBUTE ca, ReadOnlySpan<byte> value)
        => ref SetValueInternal(ref ca, value);

    /// <summary>
    /// Sets the <see cref="CREDENTIAL_ATTRIBUTE.Value"/> of the credential attribute.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL_ATTRIBUTE"/> struct.
    /// </remarks>
    /// <param name="ca">The relevant credential attribute.</param>
    /// <param name="value">The value to set.</param>
    public static ref CREDENTIAL_ATTRIBUTE SetValue<T>(ref this CREDENTIAL_ATTRIBUTE ca, ReadOnlySpan<T> value)
        where T : unmanaged
        => ref SetValueInternal(ref ca, MemoryMarshal.AsBytes(value));

    /// <summary>
    /// Sets the <see cref="CREDENTIAL_ATTRIBUTE.Value"/> of the credential attribute.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL_ATTRIBUTE"/> struct.
    /// </remarks>
    /// <param name="ca">The relevant credential attribute.</param>
    /// <param name="value">The value to set.</param>
    public static ref CREDENTIAL_ATTRIBUTE SetValue(ref this CREDENTIAL_ATTRIBUTE ca, string value)
        => ref SetValueInternal(ref ca, MemoryMarshal.AsBytes((ReadOnlySpan<char>) value));
}