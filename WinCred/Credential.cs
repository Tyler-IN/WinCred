namespace WinCred;

/// <summary>
/// Represents a Windows Credential.
/// </summary>
/// <remarks>
/// Wrapper for a mutable <see cref="CREDENTIAL"/> struct.
/// </remarks>
[PublicAPI, SupportedOSPlatform("windows")]
public sealed unsafe class Credential : IDisposable
{
    /// <summary>
    /// The size, in bytes, of various string members.
    /// * <see cref="CREDENTIAL.TargetName"/>
    /// * <see cref="CREDENTIAL.Comment"/>
    /// * <see cref="CREDENTIAL.TargetAlias"/>
    /// * <see cref="CREDENTIAL_ATTRIBUTE.Keyword"/>
    /// </summary>
    /// <remarks>
    /// Note that this includes the null terminator, so you should likely refer to the <see cref="MaximumStringLength"/> instead.
    /// Defined by CRED_MAX_STRING_LENGTH; 256 bytes.
    /// </remarks>
    public const int MaximumStringSize = 256;

    /// <summary>
    /// The maximum length of various string members.
    /// * <see cref="CREDENTIAL.TargetName"/>
    /// * <see cref="CREDENTIAL.Comment"/>
    /// * <see cref="CREDENTIAL.TargetAlias"/>
    /// * <see cref="CREDENTIAL_ATTRIBUTE.Keyword"/>
    /// </summary>
    /// <remarks>
    /// Note that this excludes the null terminator which is included in the <see cref="MaximumStringSize"/>.
    /// This is defined as one less than the maximum string size.
    /// </remarks>
    public const int MaximumStringLength = MaximumStringSize - 1;

    /// <summary>
    /// The size, in bytes, of the <see cref="CREDENTIAL_ATTRIBUTE.Value"/> member. 
    /// </summary>
    /// <remarks>
    /// Defined by CRED_MAX_VALUE_SIZE; 256 bytes.
    /// </remarks>
    public const int MaximumValueSize = 256;

    /// <summary>
    /// The size, in bytes, of the <see cref="CREDENTIAL.CredentialBlob"/> member. 
    /// </summary>
    /// <remarks>
    /// Defined by CRED_MAX_CREDENTIAL_BLOB_SIZE; 5 * 512 bytes.
    /// </remarks>
    public const int MaximumCredentialBlobSize = 2560;

    /// <summary>
    /// The maximum length of the <see cref="CREDENTIAL.UserName"/> member.
    /// </summary>
    /// <remarks>
    /// Defined by CRED_MAX_USERNAME_LENGTH; 513 characters.
    /// </remarks>
    public const int MaximumUsernameLength = 513;

    /// <summary>
    /// The maximum length of the <see cref="CREDENTIAL.Attributes"/> member.
    /// </summary>
    /// <remarks>
    /// Defined by CRED_MAX_ATTRIBUTES; 64 attributes.
    /// </remarks>
    public const int MaximumAttributes = 64;

    [MustDisposeResource, MustUseReturnValue]
    public static Credential Draft()
    {
        var c = MemoryHelpers.New<CREDENTIAL>();
        MemoryHelpers.Clear(c);
        return new Credential(c);
    }

    public void Commit(CredentialInputFlags flags = CredentialInputFlags.None)
    {
        ThrowIfDisposed();
        AdvApi32.CredWrite(this, flags);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal CREDENTIAL* _data;

    public ref CREDENTIAL Data
    {
        get
        {
            ThrowIfDisposed();
            return ref Unsafe.AsRef<CREDENTIAL>(_data);
        }
    }

    [MustDisposeResource]
    private Credential(CREDENTIAL* data) => _data = data;

    ~Credential()
    {
        if (_data is null) return;
        Dispose();
    }

    public void Dispose()
    {
        if (_data is null) return;
        GC.SuppressFinalize(this);
        // free all pointers inside first

        MemoryHelpers.FreeIfNotNull(Data._targetName);
        MemoryHelpers.FreeIfNotNull(Data._comment);
        MemoryHelpers.FreeIfNotNull(Data._targetAlias);
        MemoryHelpers.FreeIfNotNull(Data._credentialBlob);
        MemoryHelpers.FreeIfNotNull(Data._userName);
        if (Data._attributes is not null)
        {
            foreach (ref readonly var attr in Data.Attributes)
            {
                MemoryHelpers.FreeIfNotNull(attr._keyword);
                MemoryHelpers.FreeIfNotNull(attr._value);
            }

            MemoryHelpers.FreeAndNull(ref Data._attributes);
        }

        MemoryHelpers.FreeAndNull(ref _data);
    }

    public ref CREDENTIAL_ATTRIBUTE AddAttribute()
    {
        ThrowIfDisposed();
        var (pOld, oldCount) = Data.Attributes;
        if (oldCount >= MaximumAttributes)
            ExceptionHelper.ArgumentOutOfRange(nameof(Data._attributeCount), "Maximum number of attributes reached.");
        if (pOld is null)
        {
            var newSpan = MemoryHelpers.New<CREDENTIAL_ATTRIBUTE>(out var pNew, 1);
            newSpan.Clear();
            Data._attributes = pNew;
            Data._attributeCount = 1;
            return ref pNew[0];
        }
        else
        {
            var newCount = oldCount + 1;
            var (pNew, _) = MemoryHelpers.NewCopy(Data.Attributes, newCount);
            ref var pNewAttribute = ref pNew[oldCount];
            pNewAttribute = default; // clear
            Data._attributes = pNew;
            Data._attributeCount = newCount;
            MemoryHelpers.Free(pOld);
            return ref pNewAttribute;
        }
    }

    public void RemoveAttribute(int index)
    {
        ThrowIfDisposed();
        var oldAttributes = Data.Attributes;
        var (pOld, oldCount) = oldAttributes;
        if ((uint) index >= oldCount) // cast to uint is a bounds check hack
            ExceptionHelper.ArgumentOutOfRange(nameof(index), "Index out of range.");
        ref readonly var removed = ref oldAttributes[index];
        var newCount = oldCount - 1;
        var newAttributes = MemoryHelpers.New<CREDENTIAL_ATTRIBUTE>(newCount);
        newAttributes.Clear();
        oldAttributes[..index].CopyTo(newAttributes);
        oldAttributes[(index + 1)..].CopyTo(newAttributes[index..]);
        (Data._attributes, Data._attributeCount) = newAttributes;

        MemoryHelpers.FreeIfNotNull(removed._keyword);
        MemoryHelpers.FreeIfNotNull(removed._value);
        MemoryHelpers.Free(pOld);
    }

    public int IndexOfAttribute(ref CREDENTIAL_ATTRIBUTE attribute)
    {
        ThrowIfDisposed();
        var attrs = Data.Attributes;
        for (var i = 0; i < Data._attributeCount; i++)
        {
            ref var a = ref Unsafe.AsRef(in attrs[i]);
            if (Unsafe.AreSame(ref a, ref attribute))
                return i;
        }

        return -1;
    }

    [MustDisposeResource, MustUseReturnValue]
    public static Credential Create(ReadOnlySpan<char> target,
        CredentialType type = CredentialType.Generic,
        CredentialPersistence persist = CredentialPersistence.LocalMachine)
    {
        var cred = Draft();
        ref var value = ref cred.Data;
        value.Type = CredentialType.Generic;
        value.SetTargetName(target);
        value.Persist = persist;
        return cred;
    }


    [MustDisposeResource, MustUseReturnValue]
    public static Credential Create(ReadOnlySpan<char> target, ReadOnlySpan<char> comment,
        CredentialType type = CredentialType.Generic,
        CredentialPersistence persist = CredentialPersistence.LocalMachine)
    {
        var cred = Draft();
        try
        {
            ref var value = ref cred.Data;
            value.Type = CredentialType.Generic;
            value.SetTargetName(target);
            value.SetComment(comment);
            value.Persist = persist;
        }
        catch
        {
            cred.Dispose();
            throw;
        }

        return cred;
    }

    [MustDisposeResource, MustUseReturnValue]
    public static Credential Create(ReadOnlySpan<char> target, RefAction<CREDENTIAL> action)
    {
        var cred = Draft();
        ref var value = ref cred.Data;
        value.Type = CredentialType.Generic;
        value.SetTargetName(target);
        action(ref value);
        return cred;
    }

    [MustDisposeResource, MustUseReturnValue]
    public static ReadOnlyCredential? Read(ReadOnlySpan<char> target, CredentialType type)
    {
        var copy = ReadOnlySpan<char>.Empty;
        try
        {
            copy = MemoryHelpers.NullTerminateByCopy(target);
            return AdvApi32.CredRead(copy, type);
        }
        finally
        {
            MemoryHelpers.Free(copy);
        }
    }

    public static bool Delete(ReadOnlySpan<char> target, CredentialType type)
    {
        if (target.Length > MaximumStringLength)
            ExceptionHelper.ArgumentOutOfRange(nameof(target), "Target name is too long.");

        var copy = ReadOnlySpan<char>.Empty;
        try
        {
            copy = MemoryHelpers.NullTerminateByCopy(target);
            return AdvApi32.CredDelete(copy, type);
        }
        finally
        {
            MemoryHelpers.Free(copy);
        }
    }

    public static ReadOnlyCredentials Enumerate(ReadOnlySpan<char> filter)
    {
        var copy = ReadOnlySpan<char>.Empty;
        try
        {
            if (filter.IsEmpty)
                return AdvApi32.CredEnumerate()
                       ?? ReadOnlyCredentials.Empty;
            else
            {
                copy = MemoryHelpers.NullTerminateByCopy(filter);
                return AdvApi32.CredEnumerate(copy)
                       ?? ReadOnlyCredentials.Empty;
            }
        }
        finally
        {
            MemoryHelpers.Free(copy);
        }
    }

    public static ReadOnlyCredentials Enumerate()
        => Enumerate(ReadOnlySpan<char>.Empty);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_data is null)
            ExceptionHelper.ObjectDisposed(nameof(Credential),
                "Cannot access a disposed credential.");
    }
}