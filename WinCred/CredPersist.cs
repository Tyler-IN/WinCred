namespace WinCred;

[PublicAPI]
public enum CredPersist : uint
{
    None = 0,

    /// <summary>
    /// The credential persists for the life of the logon session.
    /// It will not be visible to other logon sessions of this same user.
    /// It will not exist after this user logs off and back on.
    /// </summary>
    Session = 1,

    /// <summary>
    /// The credential persists for all subsequent logon sessions on this same computer.
    /// It is visible to other logon sessions of this same user on this same computer
    /// and not visible to logon sessions for this user on other computers.
    /// </summary>
    LocalMachine = 2,

    /// <summary>
    /// The credential persists for all subsequent logon sessions on this same computer.
    /// It is visible to other logon sessions of this same user on this same computer
    /// and to logon sessions for this user on other computers.
    /// </summary>
    /// <remarks>
    /// This option can be implemented as locally persisted credential if the administrator or user
    /// configures the user account to not have roam-able state.
    /// For instance, if the user has no roaming profile, the credential will only persist locally.
    /// </remarks>
    Enterprise = 3
}