namespace WinCred;

/// <summary>
/// The type of the credential.
/// </summary>
[PublicAPI]
public enum CredentialType : uint
{
    /// <summary>
    /// The credential is a generic credential.
    /// The credential will not be used by any particular authentication package.
    /// The credential will be stored securely but has no other significant characteristics.
    /// </summary>
    Generic = 1,
    /// <summary>
    /// The credential is a password credential and is specific to Microsoft's authentication packages.
    /// The NTLM, Kerberos, and Negotiate authentication packages will automatically use this credential
    /// when connecting to the named target.
    /// </summary>
    DomainPassword = 2,
    /// <summary>
    /// The credential is a certificate credential and is specific to Microsoft's authentication packages.
    /// The Kerberos, Negotiate, and Schannel authentication packages automatically use this credential
    /// when connecting to the named target.
    /// </summary>
    DomainCertificate = 3,
    /// <summary>
    /// This value is no longer supported.
    /// </summary>
    [Obsolete("This value is no longer supported.")]
    DomainVisiblePassword = 4,
    /// <summary>
    /// The credential is a certificate credential that is a generic authentication package.
    /// </summary>
    GenericCertificate = 5,
    /// <summary>
    /// The credential is supported by extended Negotiate packages.
    /// </summary>
    DomainExtended = 6,
}