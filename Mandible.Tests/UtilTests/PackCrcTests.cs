using Mandible.Util;
using Xunit;

namespace Mandible.Tests.UtilTests
{
    public class PackCrcTests
    {
        [Fact]
        public void TestCalculate64()
        {
            const string name = "{NAMELIST}";
            const ulong nameHash = 0x4137cc65bd97fd30;

            Assert.Equal(nameHash, PackCrc.Calculate64(name));
        }
    }
}
