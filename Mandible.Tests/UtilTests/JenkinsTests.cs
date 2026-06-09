using Mandible.Util;
using System.Threading.Tasks;

namespace Mandible.Tests.UtilTests;

public class JenkinsTests
{
    [Test]
    public async Task TestOneAtATime()
    {
        const uint expected = 0x519e91f5;
        uint actual = Jenkins.OneAtATime("The quick brown fox jumps over the lazy dog");
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task TestLookup2()
    {
        const uint expected = 2606834234;
        uint actual = Jenkins.Lookup2("Global.Text.300"); // Vehicle name ID for the Flash
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task TestLocaleStringIdToLookup()
    {
        const uint expected = 3335246055;
        uint actual = Jenkins.LocaleStringIdToLookup(318); // Vehicle name ID for the Sunderer
        await Assert.That(actual).IsEqualTo(expected);
    }
}
