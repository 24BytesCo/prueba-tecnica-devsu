using Xunit;

namespace Tests.Unit.Sample;

public class SanityTests
{
    [Fact(DisplayName = "Sanidad: el framework de pruebas está operativo")]
    public void Should_Pass_When_Framework_Is_Working()
    {
        Assert.True(true);
    }
}
