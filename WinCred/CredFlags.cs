﻿namespace WinCred;

[PublicAPI, Flags]
public enum CredFlags : uint
{
    None = 0,
    PasswordForCert = 1 << 0,
    PromptNow = 1 << 1,
    UsernameTarget = 1 << 2,
    OwfCredBlob = 1 << 3,
    RequireConfirmation = 1 << 4,
    WildcardMatch = 1 << 5,
    VsmProtected = 1 << 6,
    NgcCert = 1 << 7,
}