namespace WinCred.Test;

public class FiletimeTests
{
    [Test]
    public void DateTime_Conversion_Utc_RoundTrip()
    {
        // Arrange
        var original = DateTime.UtcNow;

        // Act
        FILETIME fileTime = original;
        DateTime roundTrip = fileTime;

        // Assert
        roundTrip.ToUniversalTime()
            .Should().BeCloseTo(original, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void DateTime_Conversion_Local_RoundTrip()
    {
        // Arrange
        var original = DateTime.Now;

        // Act
        FILETIME fileTime = original;
        DateTime roundTrip = fileTime;

        // Assert
        roundTrip.ToLocalTime()
            .Should().BeCloseTo(original, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void DateTime_Conversion_LocalToUtc_RoundTrip()
    {
        // Arrange
        var original = DateTime.Now;

        // Act
        FILETIME fileTime = original;
        DateTime roundTrip = fileTime;

        // Assert
        roundTrip.ToUniversalTime()
            .Should().BeCloseTo(original.ToUniversalTime(), TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void DateTime_Conversion_UtcToLocal_RoundTrip()
    {
        // Arrange
        var original = DateTime.UtcNow;

        // Act
        FILETIME fileTime = original;
        DateTime roundTrip = fileTime;

        // Assert
        roundTrip.ToLocalTime().ToLocalTime()
            .Should().BeCloseTo(original.ToLocalTime(), TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void DateTimeOffset_Conversion_RoundTrip()
    {
        // Arrange
        var original = DateTimeOffset.Now;

        // Act
        FILETIME fileTime = original;
        DateTimeOffset roundTrip = fileTime;

        // Assert
        roundTrip.Should().BeCloseTo(original, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void ComFiletime_Conversion_RoundTrip()
    {
        // Arrange
        var ticks = DateTime.Now.ToFileTime();
        System.Runtime.InteropServices.ComTypes.FILETIME comFileTime = new()
        {
            dwLowDateTime = (int) (ticks & 0xFFFFFFFF),
            dwHighDateTime = (int) ((ticks >> 32) & 0xFFFFFFFF)
        };

        // Act
        FILETIME fileTime = comFileTime;
        System.Runtime.InteropServices.ComTypes.FILETIME roundTrip = fileTime;

        // Assert
        roundTrip.dwLowDateTime.Should().Be(comFileTime.dwLowDateTime);
        roundTrip.dwHighDateTime.Should().Be(comFileTime.dwHighDateTime);
    }

    [Test]
    public void MinDateTime_Conversion()
    {
        // Arrange - the minimum date Windows FILETIME can represent (January 1, 1601)
        var minDate = DateTime.FromFileTimeUtc(0);

        // Act
        FILETIME fileTime = minDate;
        DateTime roundTrip = fileTime;

        //roundTrip = roundTrip.ToUniversalTime();

        // Assert
        roundTrip.ToString("u")
            .Should().Be(minDate.ToString("u"));
    }

    [Test]
    public void SpecificDateTime_Values()
    {
        // Test specific dates to ensure conversion works correctly
        TestDateTimeConversion(new DateTime(2000, 1, 1));
        TestDateTimeConversion(new DateTime(2020, 6, 15, 12, 30, 45, 500));
        TestDateTimeConversion(new DateTime(1980, 1, 1));
        TestDateTimeConversion(new DateTime(2050, 12, 31, 23, 59, 59, 999));
    }

    private void TestDateTimeConversion(DateTime original)
    {
        // Act
        FILETIME fileTime = original;
        DateTime roundTrip = fileTime;

        // Assert
        roundTrip.ToUniversalTime()
            .Should().BeCloseTo(original.ToUniversalTime(), TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void SpecificDateTimeOffset_Values()
    {
        // Test specific DateTimeOffset values
        TestDateTimeOffsetConversion(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
        TestDateTimeOffsetConversion(new DateTimeOffset(2020, 6, 15, 12, 30, 45, 500, TimeSpan.FromHours(-5)));
        TestDateTimeOffsetConversion(DateTimeOffset.UtcNow);
    }

    private void TestDateTimeOffsetConversion(DateTimeOffset original)
    {
        // Act
        FILETIME fileTime = original;
        DateTimeOffset roundTrip = fileTime;

        // Assert
        roundTrip.ToUniversalTime()
            .Should().BeCloseTo(original.ToUniversalTime(), TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void DateTime_To_DateTimeOffset_Conversion()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        FILETIME fileTime = dateTime;
        DateTimeOffset dateTimeOffset = fileTime;

        // Assert
        dateTimeOffset.UtcDateTime
            .Should().BeCloseTo(dateTime, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void DateTimeOffset_To_DateTime_Conversion()
    {
        // Arrange
        var dateTimeOffset = DateTimeOffset.UtcNow;

        // Act
        FILETIME fileTime = dateTimeOffset;
        DateTime dateTime = fileTime;

        // Assert
        dateTime.Should().BeCloseTo(dateTimeOffset.DateTime, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void LastWrittenTime_ShouldConvertCorrectly()
    {
        // This test simulates the LastWritten field in CREDENTIAL_Struct

        // Arrange
        var original = DateTime.UtcNow;
        FILETIME lastWritten = original;

        // Act
        DateTime converted = lastWritten;

        // Assert
        converted.Should().BeCloseTo(original, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void MaxDateTime_Conversion()
    {
        // Arrange - test with a very high date value
        // Windows FILETIME can represent dates up to year 30828
        var futureDate = new DateTime(9999, 12, 31, 23, 59, 59, 999);

        // Act
        FILETIME fileTime = futureDate;
        DateTime roundTrip = fileTime;

        // Assert
        roundTrip.ToUniversalTime()
            .Should().BeCloseTo(futureDate.ToUniversalTime(), TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void Zero_Filetime_ReturnsMinDateTime()
    {
        // Arrange
        var fileTime = new FILETIME(); // All zeros

        // Act
        DateTime result = fileTime;

        // Assert
        result.Should().Be(DateTime.FromFileTimeUtc(0));
    }

    [Test]
    public void RoundTripWith_ExplicitFileTimeValue()
    {
        var twentyTwenty = new System.Runtime.InteropServices.ComTypes.FILETIME
        {
            dwHighDateTime = 0x01D5C036,
            dwLowDateTime = 0x69050000
        };

        FILETIME fileTime = twentyTwenty;

        // Act
        DateTime dateTime = fileTime;

        // Assert
        dateTime.Year.Should().Be(2020);
        dateTime.Month.Should().Be(1);
        dateTime.Day.Should().Be(1);
        dateTime.Hour.Should().Be(0);
        dateTime.Minute.Should().Be(0);
        dateTime.Second.Should().Be(0);
    }

    [Test]
    public void Filetime_ShouldStoreAndRetrieve_IdenticalValues()
    {
        // Arrange
        DateTime original = new DateTime(2021, 3, 15, 10, 30, 45, DateTimeKind.Utc);

        // Act
        FILETIME fileTime = original;
        DateTime retrieved = fileTime;
        FILETIME roundTrip = retrieved;

        // Use reflection to get private field values
        var lowOriginal = typeof(FILETIME)
            .GetField("Low", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(fileTime);

        var highOriginal = typeof(FILETIME)
            .GetField("High", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(fileTime);

        var lowRoundTrip = typeof(FILETIME)
            .GetField("Low", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(roundTrip);

        var highRoundTrip = typeof(FILETIME)
            .GetField("High", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(roundTrip);

        // Assert
        lowRoundTrip.Should().Be(lowOriginal);
        highRoundTrip.Should().Be(highOriginal);
    }
}