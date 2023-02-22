using Mandible.Util;
using Xunit;

namespace Mandible.Tests.UtilTests;

public class JenkinsTests
{
    [Fact]
    public void TestOneAtATime()
    {
        const uint expected = 0x519e91f5;
        uint actual = Jenkins.OneAtATime("The quick brown fox jumps over the lazy dog");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestLookup2()
    {
        const uint expected = 2606834234;
        uint actual = Jenkins.Lookup2("Global.Text.300"); // Vehicle name ID for the Flash
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestLocaleStringIdToLookup()
    {
        const uint expected = 3335246055;
        uint actual = Jenkins.LocaleStringIdToLookup(318); // Vehicle name ID for the Sunderer
        Assert.Equal(expected, actual);
    }
}
