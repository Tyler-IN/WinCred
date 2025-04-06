using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace WinCred.Test;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    [ExcludeFromCodeCoverage]
    internal static void Initialize()
    {
        FluentAssertions.License.Accepted = true;
        
        // clean up any existing credentials
        foreach (ref readonly var cred in Credential.Enumerate("WinCredTest_*"))
        {
            try
            {
                Credential.Delete(cred.TargetName, cred.Type);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }
}