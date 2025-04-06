namespace WinCred;

/// <summary>
/// Values of the Credential Flags field.
/// A bit member that identifies characteristics of the credential.
/// Undefined bits should be initialized as zero and not otherwise altered to permit future enhancement.
/// </summary>
/// <remarks>
/// These are the flags that may be read from the <see cref="CREDENTIAL.Flags"/> field.
/// They are constrained by the <c>CRED_FLAGS_VALID_FLAGS</c> macro in WinCred.h.
/// </remarks>
[PublicAPI, Flags]
public enum CredentialFlags : uint
{
    None = 0,
    PasswordForCert = 1 << 0,

    /// <summary>
    /// A.k.a. CRED_FLAGS_PROMPT_NOW.
    /// Bit set if the credential does not persist the CredentialBlob and the credential has not been written during this logon session.
    /// This bit is ignored on input and is set automatically when queried.
    /// </summary>
    /// <remarks>
    /// <p>
    /// If Type is CRED_TYPE_DOMAIN_CERTIFICATE, the CredentialBlob is not persisted across
    /// logon sessions because the PIN of a certificate is very sensitive information.
    /// Indeed, when the credential is written to credential manager, the PIN is passed to the
    /// CSP associated with the certificate. The CSP will enforce a PIN retention policy
    /// appropriate to the certificate.
    /// </p>
    /// <p>
    /// If Type is CRED_TYPE_DOMAIN_PASSWORD or CRED_TYPE_DOMAIN_CERTIFICATE, an authentication
    /// package always fails an authentication attempt when using credentials marked as
    /// CRED_FLAGS_PROMPT_NOW. The application (typically through the key ring UI) prompts the user
    /// for the password. The application saves the credential and retries the authentication.
    /// Because the credential has been recently written, the authentication package now gets
    /// a credential that is not marked as CRED_FLAGS_PROMPT_NOW.
    /// </p>
    /// </remarks>
    PromptNow = 1 << 1,

    /// <summary>
    /// A.k.a. CRED_FLAGS_USERNAME_TARGET.
    /// Bit is set if this credential has a TargetName member set to the same value as
    /// the UserName member. Such a credential is one designed to store the CredentialBlob
    /// for a specific user. For more information, see the CredMarshalCredential function.
    /// </summary>
    /// <remarks>
    /// This bit can only be specified if Type is CRED_TYPE_DOMAIN_PASSWORD
    /// or CRED_TYPE_DOMAIN_CERTIFICATE.
    /// </remarks>
    UsernameTarget = 1 << 2,
    OwfCredBlob = 1 << 3,
    RequireConfirmation = 1 << 4,

    /// <summary>
    /// Indicates credential was returned due to wildcard match
    /// of targetname with credential.
    /// </summary>
    /// <remarks>
    /// Valid only for return and only with CredReadDomainCredentials().
    /// </remarks>
    WildcardMatch = 1 << 5,

    /// <summary>
    /// Indicates that the credential is VSM protected.
    /// </summary>
    /// <remarks>
    /// Valid only for return.
    /// </remarks>
    VsmProtected = 1 << 6,
    NgcCert = 1 << 7,
}