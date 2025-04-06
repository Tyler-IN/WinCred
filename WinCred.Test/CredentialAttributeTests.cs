using FluentAssertions;

namespace WinCred.Test;

[TestFixture]
public class CredentialAttributeTests : TestFixtureWithAllocationScope
{
    private const string TestPrefix = "WinCredAttrTest_";
    private readonly List<string> _createdCredentials = [];


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
                Credential.Delete(target, CredentialType.Generic);
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
    public void AddAttribute_SingleAttribute_SavesSuccessfully()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        using var credential = Credential.Create(targetName, CredentialType.Generic, CredentialPersistence.Session);
        credential.Data.SetUserName("attruser");

        // Act
        ref var attribute = ref credential.AddAttribute();
        attribute.SetKeyword("TestCompany_AttrName");
        attribute.SetValue("AttributeValue");
        credential.Commit();

        // Assert
        using var retrieved = Credential.Read(targetName, CredentialType.Generic);
        retrieved.Should().NotBeNull();
        retrieved.Value.Attributes.Length.Should().Be(1);


        var attr = retrieved.Value.Attributes[0];
        attr.Keyword.ToString().Should().Be("TestCompany_AttrName");
        attr.Value.Length.Should().Be(28); // 14 chars * 2 bytes per char
        System.Text.Encoding.Unicode.GetString(attr.Value)
            .Should().Be("AttributeValue");
    }

    [Test]
    public void AddAttribute_MultipleAttributes_SavesSuccessfully()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        using var credential = Credential.Create(targetName, CredentialType.Generic, CredentialPersistence.Session);

        // Act
        credential.Data.Attributes.IsEmpty.Should().BeTrue();
        credential.Data._attributeCount.Should().Be(0);
        ref var attr1 = ref credential.AddAttribute();
        attr1.SetKeyword("TestCompany_Attr1");
        attr1.SetValue("Value1");

        credential.Data.Attributes.IsEmpty.Should().BeFalse();
        credential.Data._attributeCount.Should().Be(1);
        credential.Data.Attributes[0].Keyword.ToString()
            .Should().Be("TestCompany_Attr1");
        credential.Data.Attributes[0].Value
            .Length.Should().Be(12); // 6 chars * 2 bytes per char
        System.Text.Encoding.Unicode.GetString(credential.Data.Attributes[0].Value)
            .Should().Be("Value1");

        ref var attr2 = ref credential.AddAttribute();
        attr2.SetKeyword("TestCompany_Attr2");
        attr2.SetValue("Value2");

        credential.Data.Attributes.IsEmpty.Should().BeFalse();
        credential.Data._attributeCount.Should().Be(2);
        credential.Data.Attributes[0].Keyword.ToString()
            .Should().Be("TestCompany_Attr1");
        credential.Data.Attributes[0].Value
            .Length.Should().Be(12); // 6 chars * 2 bytes per char
        System.Text.Encoding.Unicode.GetString(credential.Data.Attributes[0].Value)
            .Should().Be("Value1");
        credential.Data.Attributes[1].Keyword.ToString()
            .Should().Be("TestCompany_Attr2");
        credential.Data.Attributes[1].Value
            .Length.Should().Be(12);
        System.Text.Encoding.Unicode.GetString(credential.Data.Attributes[1].Value)
            .Should().Be("Value2");

        credential.Commit();

        // Assert
        using var retrieved = Credential.Read(targetName, CredentialType.Generic);
        retrieved.Should().NotBeNull();
        retrieved.Value.Attributes.Length.Should().Be(2);

        bool foundAttr1 = false, foundAttr2 = false;
        foreach (ref readonly var attr in retrieved.Value.Attributes)
        {
            var keyword = attr.Keyword.ToString();
            var value = System.Text.Encoding.Unicode.GetString(attr.Value);

            if (keyword == "TestCompany_Attr1" && value == "Value1")
                foundAttr1 = true;

            if (keyword == "TestCompany_Attr2" && value == "Value2")
                foundAttr2 = true;
        }

        foundAttr1.Should().BeTrue("Should find first attribute");
        foundAttr2.Should().BeTrue("Should find second attribute");
    }

    [Test]
    public void RemoveAttribute_ExistingAttribute_RemovesSuccessfully()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        using var credential = Credential.Create(targetName, CredentialType.Generic, CredentialPersistence.Session);

        ref var attr1 = ref credential.AddAttribute();
        attr1.SetKeyword("TestCompany_ToKeep");
        attr1.SetValue("Value1");

        ref var attr2 = ref credential.AddAttribute();
        attr2.SetKeyword("TestCompany_ToRemove");
        attr2.SetValue("Value2");

        // Save initial state
        credential.Commit();

        // Get a fresh copy
        using var oldCopy = Credential.Read(targetName, CredentialType.Generic);
        using var mutableCopy = oldCopy!.CreateMutableCopy();

        // Act - find and remove the second attribute
        var indexToRemove = -1;
        var attributes = mutableCopy.Data.Attributes;
        for (var i = 0; i < attributes.Length; i++)
        {
            var keyword = attributes[i].Keyword;
            if (keyword.ToString() != "TestCompany_ToRemove")
                continue;

            indexToRemove = i;
            break;
        }

        indexToRemove.Should().BeGreaterThanOrEqualTo(0, "Should find the attribute to remove");
        mutableCopy.RemoveAttribute(indexToRemove);
        mutableCopy.Commit();

        // Assert
        using var newCopy = Credential.Read(targetName, CredentialType.Generic);
        newCopy.Should().NotBeNull();
        newCopy.Value.Attributes.Length.Should().Be(1);
        newCopy.Value.Attributes[0].Keyword.ToString().Should().Be("TestCompany_ToKeep");
    }

    [Test]
    public void SetValue_DifferentTypes_SavesCorrectly()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        using var credential = Credential.Create(targetName, CredentialType.Generic, CredentialPersistence.Session);

        // Act - test different value types
        ref var strAttr = ref credential.AddAttribute();
        strAttr.SetKeyword("TestCompany_String");
        strAttr.SetValue("String value");

        ref var bytesAttr = ref credential.AddAttribute();
        bytesAttr.SetKeyword("TestCompany_Bytes");
        byte[] testBytes = [1, 2, 3, 4, 5];
        bytesAttr.SetValue(testBytes);

        ref var intAttr = ref credential.AddAttribute();
        intAttr.SetKeyword("TestCompany_Int");
        var testInt = 12345;
        intAttr.SetValue([testInt]);

        credential.Commit();

        // Assert
        using var retrieved = Credential.Read(targetName, CredentialType.Generic);
        retrieved.Should().NotBeNull();
        retrieved.Value.Attributes.Length.Should().Be(3);

        foreach (ref readonly var attr in retrieved.Value.Attributes)
        {
            var keyword = attr.Keyword.ToString();

            if (keyword == "TestCompany_String")
            {
                var value = System.Text.Encoding.Unicode.GetString(attr.Value);
                value.Should().Be("String value");
            }
            else if (keyword == "TestCompany_Bytes")
            {
                var value = attr.Value.ToArray();
                value.Should().BeEquivalentTo(testBytes);
            }
            else if (keyword == "TestCompany_Int")
            {
                var value = BitConverter.ToInt32(attr.Value);
                value.Should().Be(testInt);
            }
        }
    }

    [Test]
    public void IndexOfAttribute_FindsAttribute()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        using var credential = Credential.Create(targetName, CredentialType.Generic, CredentialPersistence.Session);

        ref var attr1 = ref credential.AddAttribute();
        attr1.SetKeyword("TestCompany_First");
        attr1.SetValue("Value1");

        ref var attr2 = ref credential.AddAttribute();
        attr2.SetKeyword("TestCompany_Second");
        attr2.SetValue("Value2");

        // Act
        var index = credential.IndexOfAttribute(ref attr2);

        // Assert
        index.Should().BeGreaterThanOrEqualTo(0, "Should find the attribute");
    }

    [Test]
    public void AddAttribute_SpecialCharacters_SavesSuccessfully()
    {
        // Arrange
        var targetName = GetUniqueTargetName();
        using var credential = Credential.Create(targetName, CredentialType.Generic, CredentialPersistence.Session);
        credential.Data.SetUserName("specialcharuser");

        // Act
        ref var attribute = ref credential.AddAttribute();
        attribute.SetKeyword("TestCompany_SpecialChars");
        attribute.SetValue("Attr!@#$%^&*()_+");

        credential.Commit();

        // Assert
        using var retrieved = Credential.Read(targetName, CredentialType.Generic);
        retrieved.Should().NotBeNull();
        retrieved.Value.Attributes.Length.Should().Be(1);

        var attr = retrieved.Value.Attributes[0];
        attr.Keyword.ToString().Should().Be("TestCompany_SpecialChars");
        System.Text.Encoding.Unicode.GetString(attr.Value)
            .Should().Be("Attr!@#$%^&*()_+");
    }
}
