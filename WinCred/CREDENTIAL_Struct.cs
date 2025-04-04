namespace WinCred;

[PublicAPI, StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public unsafe struct CREDENTIAL
{
    public CredFlags Flags;
    public CredType Type;

    /// <inheritdoc cref="TargetName"/>
    public char* _targetName;

    /// <summary>
    /// The name of the credential, case-insensitive.
    /// The TargetName and Type members uniquely identify the credential.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This member cannot be changed after the credential is created.
    /// Instead, the credential with the old name should be deleted and the credential with the new name created.
    /// </p>
    /// <p>
    /// If Type is CRED_TYPE_DOMAIN_PASSWORD or CRED_TYPE_DOMAIN_CERTIFICATE, this member identifies the server
    /// or servers that the credential is to be used for.
    /// The member is either a NetBIOS or DNS server name, a DNS host name suffix that contains a wildcard character,
    /// a NetBIOS or DNS domain name that contains a wildcard character sequence, or an asterisk.
    /// </p>
    /// <p>
    /// If TargetName is a DNS host name, the TargetAlias member can be the NetBIOS name of the host.
    /// </p>
    /// <p>
    /// If the TargetName is a DNS host name suffix that contains a wildcard character,
    /// the leftmost label of the DNS host name is an asterisk (*), which denotes that the target name is any server
    /// whose name ends in the specified name, for example, *.microsoft.com.
    /// </p>
    /// <p>
    /// If the TargetName is a domain name that contains a wildcard character sequence,
    /// the syntax is the domain name followed by a backslash and asterisk (\*), which denotes that the target name is
    /// any server that is a member of the named domain (or realm).
    /// </p>
    /// <p>
    /// If TargetName is a DNS domain name that contains a wildcard character sequence,
    /// the TargetAlias member can be a NetBIOS domain name that uses a wildcard sequence for the same domain.
    /// </p>
    /// <p>
    /// If TargetName specifies a DFS share, for example, DfsRoot\DfsShare,
    /// then this credential matches the specific DFS share and any servers reached through that DFS share.
    /// </p>
    /// <p>
    /// If TargetName is a single asterisk (*), this credential matches any server name.
    /// </p>
    /// <p>
    /// If TargetName is CRED_SESSION_WILDCARD_NAME, this credential matches any server name.
    /// This credential matches before a single asterisk and is only valid if Persist is CRED_PERSIST_SESSION.
    /// The credential can be set by applications that want to temporarily override the default credential.
    /// This member cannot be longer than CRED_MAX_DOMAIN_TARGET_NAME_LENGTH (337) characters.
    /// </p>
    /// <p>
    /// If the Type is CRED_TYPE_GENERIC, this member should identify the service that uses the credential
    /// in addition to the actual target. Microsoft suggests the name be prefixed by the name of the company
    /// implementing the service. Microsoft will use the prefix "Microsoft".
    /// </p>
    /// <p>
    /// Services written by Microsoft should append their service name, for example Microsoft_RAS_TargetName.
    /// This member cannot be longer than CRED_MAX_GENERIC_TARGET_NAME_LENGTH (32767) characters.
    /// </p>
    /// </remarks>
    public ReadOnlySpan<char> TargetName => _targetName is null
        ? ReadOnlySpan<char>.Empty
        : MemoryHelpers.NullTerminatedToReadOnlySpan(_targetName, Credential.MaximumStringSize);

    /// <inheritdoc cref="Comment"/>
    public char* _comment;

    /// <summary>
    /// A string comment from the user that describes this credential.
    /// </summary>
    /// <remarks>
    /// This member cannot be longer than CRED_MAX_STRING_LENGTH (256) characters.
    /// </remarks>
    public readonly ReadOnlySpan<char> Comment
        => _comment is not null
            ? MemoryHelpers.NullTerminatedToReadOnlySpan(_comment, Credential.MaximumStringSize)
            : ReadOnlySpan<char>.Empty;

    /// <summary>
    /// The time, in Coordinated Universal Time (Greenwich Mean Time), of the last modification of the credential.
    /// For write operations, the value of this member is ignored.
    /// </summary>
    public FILETIME LastWritten;

    /// <summary>
    /// The size, in bytes, of the CredentialBlob member.
    /// This member cannot be larger than CRED_MAX_CREDENTIAL_BLOB_SIZE (5*512) bytes.
    /// </summary>
    /// <remarks>
    /// Although the SDK defines this as a DWORD, for convenience, this is a signed int.
    /// It has a maximum below <see cref="int.MaxValue"/>.
    /// </remarks>
    internal int _credentialBlobSize;

    /// <inheritdoc cref="CredentialBlob"/>
    internal byte* _credentialBlob;

    /// <summary>
    /// Secret data for the credential. The CredentialBlob member can be both read and written.
    /// </summary>
    /// <remarks>
    /// <p>
    /// If the Type member is CRED_TYPE_DOMAIN_PASSWORD, this member contains the plaintext Unicode password for UserName.
    /// The CredentialBlob and CredentialBlobSize members do not include a trailing zero character.
    /// Also, for CRED_TYPE_DOMAIN_PASSWORD, this member can only be read by the authentication packages.
    /// </p>
    /// <p>
    /// If the Type member is CRED_TYPE_DOMAIN_CERTIFICATE, this member contains the clear test Unicode PIN for UserName.
    /// The CredentialBlob and CredentialBlobSize members do not include a trailing zero character.
    /// Also, this member can only be read by the authentication packages.
    /// </p>
    /// <p>
    /// If the Type member is CRED_TYPE_GENERIC, this member is defined by the application.
    /// </p>
    /// <p>
    /// Credentials are expected to be portable.
    /// Applications should ensure that the data in CredentialBlob is portable.
    /// The application defines the byte-endian and alignment of the data in CredentialBlob.
    /// </p>
    /// </remarks>
    public readonly ReadOnlySpan<byte> CredentialBlob
        => _credentialBlob is not null
            ? new ReadOnlySpan<byte>(_credentialBlob, _credentialBlobSize)
            : ReadOnlySpan<byte>.Empty;

    /// <summary>
    /// Defines the persistence of this credential. This member can be read and written.
    /// </summary>
    public CredPersist Persist;

    /// <summary>
    /// The number of application-defined attributes to be associated with the credential.
    /// This member can be read and written.
    /// Its value cannot be greater than CRED_MAX_ATTRIBUTES (64).
    /// </summary>
    /// <remarks>
    /// Although the SDK defines this as a DWORD, for convenience, this is a signed int.
    /// It has a maximum below <see cref="int.MaxValue"/>.
    /// </remarks>
    public int _attributeCount;

    /// <inheritdoc cref="Attributes"/>
    public CREDENTIAL_ATTRIBUTE* _attributes;

    /// <summary>
    /// Application-defined attributes that are associated with the credential.
    /// This member can be read and written.
    /// </summary>
    /// <remarks>
    /// It cannot represent more than CRED_MAX_ATTRIBUTES (64) attributes.
    /// </remarks>
    public readonly ReadOnlySpan<CREDENTIAL_ATTRIBUTE> Attributes
        => _attributes is not null
            ? new ReadOnlySpan<CREDENTIAL_ATTRIBUTE>(_attributes, _attributeCount)
            : ReadOnlySpan<CREDENTIAL_ATTRIBUTE>.Empty;

    /// <inheritdoc cref="TargetAlias"/>
    internal char* _targetAlias;


    /// <summary>
    /// Alias for the TargetName member.
    /// This member can be read and written.
    /// </summary>
    /// <remarks>
    /// It cannot be longer than CRED_MAX_STRING_LENGTH (256) characters.
    /// If the credential Type is CRED_TYPE_GENERIC, this member can be non-NULL,
    /// but the credential manager ignores the member.
    /// </remarks>
    public readonly ReadOnlySpan<char> TargetAlias
        => _targetAlias is not null
            ? MemoryHelpers.NullTerminatedToReadOnlySpan(_targetAlias, Credential.MaximumStringSize)
            : ReadOnlySpan<char>.Empty;

    /// <inheritdoc cref="UserName"/>
    internal char* _userName;

    /// <summary>
    /// The user name of the account used to connect to TargetName.
    /// </summary>
    /// <remarks>
    /// <p>
    /// If the credential Type is CRED_TYPE_DOMAIN_PASSWORD,
    /// this member can be either a DomainName\UserName or a UPN.
    /// </p>
    /// <p>
    /// If the credential Type is CRED_TYPE_DOMAIN_CERTIFICATE,
    /// this member must be a marshaled certificate reference created
    /// by calling CredMarshalCredential with a CertCredential.
    /// </p>
    /// <p>
    /// If the credential Type is CRED_TYPE_GENERIC, this member can be non-NULL,
    /// but the credential manager ignores the member.
    /// </p>
    /// <p>
    /// This member cannot be longer than CRED_MAX_USERNAME_LENGTH (513) characters.
    /// </p>
    /// </remarks>
    public readonly ReadOnlySpan<char> UserName
        => _userName is not null
            ? MemoryHelpers.NullTerminatedToReadOnlySpan(_userName, Credential.MaximumUsernameLength)
            : ReadOnlySpan<char>.Empty;

    public readonly void CopyTo(ref CREDENTIAL mutableCopy, int modifyAttributeCount = 0)
    {
        mutableCopy.Flags = Flags;
        mutableCopy.Type = Type;
        mutableCopy.LastWritten = LastWritten;
        mutableCopy.Persist = Persist;
        
        var targetNameSpan = MemoryHelpers.DuplicateNullTerminated
            (_targetName, Credential.MaximumStringSize, true);
        (mutableCopy._targetName, _) = targetNameSpan;
        
        var commentSpan = MemoryHelpers.DuplicateNullTerminated
            (_comment, Credential.MaximumStringSize, true);
        (mutableCopy._comment, _) = commentSpan;

        var targetAliasSpan = MemoryHelpers.DuplicateNullTerminated
            (_targetAlias, Credential.MaximumStringSize, true);
        (mutableCopy._targetAlias, _) =
            targetAliasSpan;
        
        var userNameSpan = MemoryHelpers.DuplicateNullTerminated
            (_userName, Credential.MaximumUsernameLength, true);
        (mutableCopy._userName, _) = userNameSpan;
        
        if (_credentialBlobSize != 0)
        {
            var duplicateBlob = MemoryHelpers.Duplicate
                (CredentialBlob);
            (mutableCopy._credentialBlob, mutableCopy._credentialBlobSize) = duplicateBlob;
        }
        else
        {
            mutableCopy._credentialBlob = null;
            mutableCopy._credentialBlobSize = 0;
        }

        var newAttributeCount = modifyAttributeCount + _attributeCount;
        mutableCopy._attributeCount = newAttributeCount;
        if (newAttributeCount > 0)
        {
            var attributes = MemoryHelpers.New
                (out mutableCopy._attributes, newAttributeCount);
            attributes.Clear();
            var copyAttributeCount = Math.Min(newAttributeCount, _attributeCount);
            {
                // Copy existing attributes and zero out any new ones
                var i = 0;
                for (; i < copyAttributeCount; i++)
                    _attributes[i].CopyTo(ref attributes[i]);
                for (; i < newAttributeCount; i++)
                    attributes[i] = default;
            }
        }
        else
            mutableCopy._attributes = null;
    }

    
    [MustDisposeResource, MustUseReturnValue]
    public readonly Credential CreateMutableCopy()
    {
        var draft = Credential.Draft();
        CopyTo(ref draft.Data);
        return draft;
    }

    public override string ToString()
    {
        var pointer = (nuint) Unsafe.AsPointer(ref Unsafe.AsRef(in this));
        return $"[CREDENTIAL_ATTRIBUTE @ 0x{(ulong) pointer:X8}]";
    }
}