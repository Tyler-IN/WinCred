using FluentAssertions;

namespace WinCred.Test;

[TestFixture]
public class CredentialBasicTests : TestFixtureWithAllocationScope
{
    private const string TestPrefix = "WinCredTest_";
    private readonly List<string> _createdCredentials = new();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        TestFixtureWithAllocationScope.OneTimeSetup(GetType());
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        TestFixtureWithAllocationScope.OneTimeTeardown(GetType());
    }

    [TearDown]
    public void Cleanup()
    {
        // Clean up any test credentials we created
        foreach (var target in _createdCredentials)
        {
            try
            {
                Credential.Delete(target, CredType.Generic);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }

        _createdCredentials.Clear();
    }

    private string GetUniqueTargetName()
    {
        var targetName = $"{TestPrefix}{Guid.NewGuid()}";
        _createdCredentials.Add(targetName);
        return targetName;
    }

    [Test]
    public void Create_SimpleCredential_SavesSuccessfully()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        var username = "testuser";
        var password = "testpassword";

        try
        {
            // Act
            using var credential = Credential.Create(targetName, CredType.Generic, CredPersist.Session);
            credential.Data.SetUserName(username);
            credential.Data.SetCredentialBlob(password);
            var committed = credential.Commit();

            // Assert
            committed.Should().BeTrue("Commit should succeed");
            var retrieved = Credential.Read(targetName, CredType.Generic);
            retrieved.Should().NotBeNull();
            retrieved.Value.UserName.ToString().Should().Be(username);
            System.Text.Encoding.Unicode
                .GetString(retrieved.Value.CredentialBlob)
                .Should().Be(password);
        }
        finally
        {
            try
            {
                Credential.Delete(targetName, CredType.Generic);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }

    [Test]
    public void Create_SimpleUtf8Credential_SavesSuccessfully()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        var username = "testuser";
        var password = "testpassword";

        try
        {
            // Act
            using var credential = Credential.Create(targetName, CredType.Generic, CredPersist.Session);
            credential.Data.SetUserName(username);
            credential.Data.SetCredentialBlob(password, utf8: true);
            var committed = credential.Commit();

            // Assert
            committed.Should().BeTrue("Commit should succeed");
            var retrieved = Credential.Read(targetName, CredType.Generic);
            retrieved.Should().NotBeNull();
            retrieved.Value.UserName.ToString().Should().Be(username);
            System.Text.Encoding.UTF8
                .GetString(retrieved.Value.CredentialBlob)
                .Should().Be(password);
        }
        finally
        {
            try
            {
                Credential.Delete(targetName, CredType.Generic);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }

    [Test]
    public void Read_NonExistentCredential_ReturnsNull()
    {
        // Arrange
        var nonExistentTarget = $"{TestPrefix}_NonExistent_{Guid.NewGuid()}";

        // Act
        var credential = Credential.Read(nonExistentTarget, CredType.Generic);

        // Assert
        credential.Should().BeNull();
    }

    [Test]
    public void Delete_ExistingCredential_RemovesSuccessfully()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        try
        {
            using var credential = Credential.Create(targetName, CredType.Generic, CredPersist.Session);
            credential.Data.SetUserName("deletetest");
            var committed = credential.Commit();

            // Verify it exists
            committed.Should().BeTrue("Commit should succeed");
            var exists = Credential.Read(targetName, CredType.Generic);
            exists.Should().NotBeNull();

            // Act
            var result = Credential.Delete(targetName, CredType.Generic);

            // Assert
            result.Should().BeTrue();
            var afterDelete = Credential.Read(targetName, CredType.Generic);
            afterDelete.Should().BeNull();
        }
        finally
        {
            try
            {
                Credential.Delete(targetName, CredType.Generic);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }

    [Test]
    public void Create_CredentialWithComment_SavesComment()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        try
        {
            const string user = "commentuser";
            const string comment = "Test credential comment";

            // Act
            using var credential = Credential.Create(targetName, comment, CredType.Generic, CredPersist.Session);
            credential.Data.SetUserName(user);
            var committed = credential.Commit();

            // Assert
            committed.Should().BeTrue("Commit should succeed");
            var retrieved = Credential.Read(targetName, CredType.Generic);
            retrieved.Should().NotBeNull();
            retrieved.Value.UserName.ToString().Should().Be(user);
            retrieved.Value.Comment.ToString().Should().Be(comment);
        }
        finally
        {
            try
            {
                Credential.Delete(targetName, CredType.Generic);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }

    [Test]
    public void Enumerate_AfterCreatingCredentials_ReturnsCredentials()
    {
        // Arrange
        var targetName1 = GetUniqueTargetName();
        var targetName2 = GetUniqueTargetName();

        try
        {
            using var cred1 = Credential.Create(targetName1, CredType.Generic, CredPersist.Session);
            cred1.Data.SetUserName("user1");
            var committed1 = cred1.Commit();
            committed1.Should().BeTrue("Commit should succeed");

            using var cred2 = Credential.Create(targetName2, CredType.Generic, CredPersist.Session);
            cred2.Data.SetUserName("user2");
            var committed2 = cred2.Commit();
            committed2.Should().BeTrue("Commit should succeed");

            // Act
            var credentials = Credential.Enumerate(TestPrefix + "*");

            // Assert
            credentials.Should().NotBeNull();
            credentials.Span.Length.Should().BeGreaterThanOrEqualTo(2);

            bool foundCred1 = false, foundCred2 = false;
            foreach (ref readonly var cred in credentials)
            {
                var target = cred.TargetName.ToString();
                if (target == targetName1) foundCred1 = true;
                if (target == targetName2) foundCred2 = true;
            }

            foundCred1.Should().BeTrue("Should find the first test credential");
            foundCred2.Should().BeTrue("Should find the second test credential");
        }
        finally
        {
            try
            {
                Credential.Delete(targetName1, CredType.Generic);
                Credential.Delete(targetName2, CredType.Generic);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }

    [Test]
    public void CreateMutableCopy_FromReadOnly_CreatesEquivalentCredential()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        try
        {
            using var original = Credential.Create(targetName, CredType.Generic, CredPersist.Session);
            original.Data.SetUserName("originaluser");
            original.Data.SetComment("Original comment");
            var committed = original.Commit();

            var readOnly = Credential.Read(targetName, CredType.Generic);
            readOnly.Should().NotBeNull();

            // Act
            using var copy = readOnly.CreateMutableCopy();

            // Assert
            committed.Should().BeTrue("Commit should succeed");
            copy.Should().NotBeNull();
            var dataTargetName = copy.Data.TargetName.ToString();
            var dataUserName = copy.Data.UserName.ToString();
            var dataComment = copy.Data.Comment.ToString();
            dataTargetName.Should().Be(targetName);
            dataUserName.Should().Be("originaluser");
            dataComment.Should().Be("Original comment");
        }
        finally
        {
            try
            {
                Credential.Delete(targetName, CredType.Generic);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }
}