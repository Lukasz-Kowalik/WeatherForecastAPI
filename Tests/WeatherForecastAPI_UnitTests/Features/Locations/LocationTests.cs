using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI_UnitTests.Features.Locations;

public class LocationTests
{
    private readonly Coordinates _validCoordinates = Coordinates.Create(40.7128m, -74.0060m);

    [Fact]
    public void Create_WithValidCoordinates_ReturnsLocation()
    {
        // Act
        var location = Location.Create(_validCoordinates);

        // Assert
        location.Should().NotBeNull();
        location.Coordinates.Should().Be(_validCoordinates);
        location.Name.Should().BeNull();
        location.LastUsedAt.Should().NotBe(DateTime.MinValue);
    }

    [Fact]
    public void Create_WithCoordinatesAndName_ReturnsLocationWithName()
    {
        // Arrange
        string name = "New York";

        // Act
        var location = Location.Create(_validCoordinates, name);

        // Assert
        location.Should().NotBeNull();
        location.Coordinates.Should().Be(_validCoordinates);
        location.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithNullName_ReturnsLocationWithNullName()
    {
        // Act
        var location = Location.Create(_validCoordinates, null);

        // Assert
        location.Should().NotBeNull();
        location.Name.Should().BeNull();
    }

    [Fact]
    public void Create_SetsLastUsedAtToNearCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var location = Location.Create(_validCoordinates);
        var afterCreation = DateTime.UtcNow;

        // Assert
        location.LastUsedAt.Should().BeOnOrAfter(beforeCreation);
        location.LastUsedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Create_MultipleLocations_HaveIndependentState()
    {
        // Arrange
        var coordinates1 = Coordinates.Create(40.7128m, -74.0060m);
        var coordinates2 = Coordinates.Create(51.5074m, -0.1278m);

        // Act
        var location1 = Location.Create(coordinates1, "New York");
        var location2 = Location.Create(coordinates2, "London");

        // Assert
        location1.Coordinates.Should().NotBe(location2.Coordinates);
        location1.Name.Should().NotBe(location2.Name);
    }

    [Fact]
    public void UpdateUsage_UpdatesLastUsedAt()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);
        var originalLastUsedAt = location.LastUsedAt;
        System.Threading.Thread.Sleep(10);

        // Act
        location.UpdateUsage();

        // Assert
        location.LastUsedAt.Should().BeAfter(originalLastUsedAt);
    }

    [Fact]
    public void UpdateUsage_SetLastUsedAtToNearCurrentTime()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        location.UpdateUsage();
        var afterUpdate = DateTime.UtcNow;

        // Assert
        location.LastUsedAt.Should().BeOnOrAfter(beforeUpdate);
        location.LastUsedAt.Should().BeOnOrBefore(afterUpdate);
    }

    [Fact]
    public void UpdateUsage_MultipleInvocations_IncrementsLastUsedAt()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);
        var times = new List<DateTime> { location.LastUsedAt };

        // Act
        for (int i = 0; i < 3; i++)
        {
            System.Threading.Thread.Sleep(10);
            location.UpdateUsage();
            times.Add(location.LastUsedAt);
        }

        // Assert
        for (int i = 0; i < times.Count - 1; i++)
        {
            times[i + 1].Should().BeAfter(times[i]);
        }
    }

    [Fact]
    public void SetName_WithValidName_UpdatesName()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);
        string newName = "Updated Location";

        // Act
        location.SetName(newName);

        // Assert
        location.Name.Should().Be(newName);
    }

    [Fact]
    public void SetName_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);

        // Act & Assert
        var action = () => location.SetName(string.Empty);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*Name cannot be empty*");
    }

    [Fact]
    public void SetName_WithWhitespaceOnly_ThrowsArgumentException()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);

        // Act & Assert
        var action = () => location.SetName("   ");
        action.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void SetName_WithNull_ThrowsArgumentException()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);

        // Act & Assert
        var action = () => location.SetName(null!);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void SetName_OverwritesPreviousName()
    {
        // Arrange
        var location = Location.Create(_validCoordinates, "Original");

        // Act
        location.SetName("Updated");

        // Assert
        location.Name.Should().Be("Updated");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Location Name")]
    [InlineData("Very Long Location Name With Many Words")]
    [InlineData("123")]
    public void SetName_WithVariousValidNames_UpdatesName(string name)
    {
        // Arrange
        var location = Location.Create(_validCoordinates);

        // Act
        location.SetName(name);

        // Assert
        location.Name.Should().Be(name);
    }

    [Fact]
    public void ClearName_SetsNameToNull()
    {
        // Arrange
        var location = Location.Create(_validCoordinates, "Test Location");

        // Act
        location.ClearName();

        // Assert
        location.Name.Should().BeNull();
    }

    [Fact]
    public void ClearName_WhenNameIsNull_StaysNull()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);

        // Act
        location.ClearName();

        // Assert
        location.Name.Should().BeNull();
    }

    [Fact]
    public void ClearName_CanBeCalledMultipleTimes()
    {
        // Arrange
        var location = Location.Create(_validCoordinates, "Test Location");

        // Act
        location.ClearName();
        location.ClearName();

        // Assert
        location.Name.Should().BeNull();
    }

    [Fact]
    public void SetNameThenClearName_RemovesName()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);

        // Act
        location.SetName("Name");
        location.Name.Should().Be("Name");
        location.ClearName();

        // Assert
        location.Name.Should().BeNull();
    }

    [Fact]
    public void WeatherForecasts_InitiallyEmpty()
    {
        // Arrange & Act
        var location = Location.Create(_validCoordinates);

        // Assert
        location.WeatherForecasts.Should().NotBeNull();
        location.WeatherForecasts.Should().BeEmpty();
    }

    [Fact]
    public void WeatherForecasts_IsReadOnly()
    {
        // Arrange
        var location = Location.Create(_validCoordinates);

        // Assert
        location.WeatherForecasts.Should().BeAssignableTo<IReadOnlyCollection<WeatherForecastAPI.Features.Weather.WeatherForecast>>();
    }

    [Fact]
    public void Location_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var location = Location.Create(_validCoordinates);

        // Assert
        location.Should().NotBeNull();
        location.Should().BeAssignableTo<WeatherForecastAPI.Common.Database.BaseEntity>();
    }

    [Fact]
    public void Location_CannotBeCreatedWithoutFactory()
    {
        // Assert - Location has private constructor
        var constructors = typeof(Location).GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        constructors.Should().BeEmpty();
    }
}