﻿namespace WinCred;

[PublicAPI]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class CREDENTIAL_Helper
{
    /// <summary>
    /// Sets the <see cref="CREDENTIAL.TargetName"/> of the credential.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL"/> struct.
    /// </remarks>
    /// <param name="c">The relevant credential.</param>
    /// <param name="value">The target name to set.</param>
    public static unsafe ref CREDENTIAL SetTargetName(ref this CREDENTIAL c, ReadOnlySpan<char> value)
    {
        if (value.Length > Credential.MaximumStringSize)
            ExceptionHelper.ArgumentOutOfRange(nameof(value), "Target name size exceeds maximum size.");

        if (c._targetName is not null) MemoryHelpers.Free(c._targetName);
        if (value.IsEmpty)
        {
            c._targetName = null;
            return ref c;
        }

        var length = value.Length;
        var needsNullTerminator = value[^1] != 0;
        var (pCopy, _) = MemoryHelpers.NewCopy(value, length + (needsNullTerminator ? 1 : 0));
        if (needsNullTerminator)
            pCopy[length] = default; // null terminate
        c._targetName = pCopy;
        return ref c;
    }

    /// <summary>
    /// Sets the <see cref="CREDENTIAL.Comment"/> of the credential.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL"/> struct.
    /// </remarks>
    /// <param name="c">The relevant credential.</param>
    /// <param name="value">The comment to set.</param>
    public static unsafe ref CREDENTIAL SetComment(ref this CREDENTIAL c, ReadOnlySpan<char> value)
    {
        if (value.Length > Credential.MaximumStringSize)
            ExceptionHelper.ArgumentOutOfRange(nameof(value), "Comment size exceeds maximum size.");

        if (c._comment is not null) MemoryHelpers.Free(c._comment);
        if (value.IsEmpty)
        {
            c._comment = null;
            return ref c;
        }

        var length = value.Length;
        var needsNullTerminator = value[^1] != 0;
        var (pCopy, _) = MemoryHelpers.NewCopy(value, length + (needsNullTerminator ? 1 : 0));
        if (needsNullTerminator)
            pCopy[length] = default; // null terminate
        c._comment = pCopy;
        return ref c;
    }

    /// <summary>
    /// Sets the <see cref="CREDENTIAL.CredentialBlob"/> of the credential.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL"/> struct.
    /// </remarks>
    /// <param name="c">The relevant credential.</param>
    /// <param name="value">The credential blob to set.</param>
    public static unsafe ref CREDENTIAL SetCredentialBlob<T>(ref this CREDENTIAL c, ReadOnlySpan<T> value)
        where T : struct
    {
        var credentialBlob = MemoryMarshal.AsBytes(value);
        if (credentialBlob.Length > Credential.MaximumCredentialBlobSize)
            ExceptionHelper.ArgumentOutOfRange(nameof(value), "Credential blob size exceeds maximum size.");

        if (c._credentialBlob is not null) MemoryHelpers.Free(c._credentialBlob);
        if (credentialBlob.IsEmpty)
        {
            c._credentialBlob = null;
            c._credentialBlobSize = 0;
            return ref c;
        }

        (c._credentialBlob, c._credentialBlobSize) = MemoryHelpers.Duplicate(credentialBlob);
        return ref c;
    }


    /// <summary>
    /// Sets the <see cref="CREDENTIAL.CredentialBlob"/> of the credential.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL"/> struct.
    /// </remarks>
    /// <param name="c">The relevant credential.</param>
    /// <param name="value">The credential blob to set.</param>
    /// <param name="utf8">If true, the credentials are UTF-8 encoded. Otherwise, they are treated as a character array.</param>
    /// <param name="nullTerminate">If true, the credentials are null terminated.</param>
    public static ref CREDENTIAL SetCredentialBlob(ref this CREDENTIAL c, string? value,
        bool utf8 = false, bool nullTerminate = false)
        => ref utf8
            ? ref new Utf8Prep(value).OnStack(
                static (ReadOnlySpan<byte> bytes, ref CREDENTIAL c)
                    => c.SetCredentialBlob(bytes),
                ref c, nullTerminate)
            : ref c.SetCredentialBlob<char>(value);

    /// <summary>
    /// Sets the <see cref="CREDENTIAL.Attributes"/> of the credential.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL"/> struct.
    /// </remarks>
    /// <param name="c">The relevant credential.</param>
    /// <param name="attributes">The attributes to set.</param>
    public static unsafe ref CREDENTIAL SetAttributes(ref this CREDENTIAL c,
        ReadOnlySpan<CREDENTIAL_ATTRIBUTE> attributes)
    {
        if (attributes.Length > Credential.MaximumAttributes)
            ExceptionHelper.ArgumentOutOfRange(nameof(attributes), "Attribute count exceeds maximum size.");

        if (c._attributes is not null) MemoryHelpers.Free(c._attributes);
        if (attributes.IsEmpty)
        {
            c._attributes = null;
            c._attributeCount = 0;
            return ref c;
        }

        (c._attributes, c._attributeCount) = MemoryHelpers.Duplicate(attributes);
        return ref c;
    }

    /// <summary>
    /// Sets the <see cref="CREDENTIAL.TargetAlias"/> of the credential.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL"/> struct.
    /// </remarks>
    /// <param name="c">The relevant credential.</param>
    /// <param name="value">The target alias to set.</param>
    public static unsafe ref CREDENTIAL SetTargetAlias(ref this CREDENTIAL c, ReadOnlySpan<char> value)
    {
        if (value.Length > Credential.MaximumStringSize)
            ExceptionHelper.ArgumentOutOfRange(nameof(value), "Target alias size exceeds maximum size.");

        if (c._targetAlias is not null) MemoryHelpers.Free(c._targetAlias);
        if (value.IsEmpty)
        {
            c._targetAlias = null;
            return ref c;
        }

        var length = value.Length;
        var needsNullTerminator = value[^1] != 0;
        var (pCopy, _) = MemoryHelpers.NewCopy(value, length + (needsNullTerminator ? 1 : 0));
        if (needsNullTerminator)
            pCopy[length] = default; // null terminate
        c._targetAlias = pCopy;
        return ref c;
    }

    /// <summary>
    /// Sets the <see cref="CREDENTIAL.UserName"/> of the credential.
    /// </summary>
    /// <remarks>
    /// The ref keyword means this only works on a mutable <see cref="CREDENTIAL"/> struct.
    /// </remarks>
    /// <param name="c">The relevant credential.</param>
    /// <param name="value">The user name to set.</param>
    public static unsafe ref CREDENTIAL SetUserName(ref this CREDENTIAL c, ReadOnlySpan<char> value)
    {
        if (value.Length > Credential.MaximumUsernameLength)
            ExceptionHelper.ArgumentOutOfRange(nameof(value), "User name size exceeds maximum size.");

        if (c._userName is not null) MemoryHelpers.Free(c._userName);
        if (value.IsEmpty)
        {
            c._userName = null;
            return ref c;
        }

        var length = value.Length;
        var needsNullTerminator = value[^1] != 0;
        var (pCopy, _) = MemoryHelpers.NewCopy(value, length + (needsNullTerminator ? 1 : 0));
        if (needsNullTerminator)
            pCopy[length] = default; // null terminate
        c._userName = pCopy;
        return ref c;
    }
}