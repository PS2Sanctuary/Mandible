using Mandible.Util;
using System.Threading.Tasks;

namespace Mandible.Tests.UtilTests;

public class PackCrcTests
{
    [Test]
    public async Task TestCalculate64()
    {
        const string name = "{NAMELIST}";
        const ulong nameHash = 0x4137cc65bd97fd30;

        await Assert.That(PackCrc64.Calculate(name)).IsEqualTo(nameHash);
    }
}