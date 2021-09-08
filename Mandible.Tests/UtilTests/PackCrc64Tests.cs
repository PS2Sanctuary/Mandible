using Mandible.Util;
using Xunit;

namespace Mandible.Tests.UtilTests
{
    public class PackCrc64Tests
    {
        [Fact]
        public void TestCalculate()
        {
            const string name = "{NAMELIST}";
            const ulong nameHash = 0x4137cc65bd97fd30;

            Assert.Equal(nameHash, PackCrc64.Calculate(name));
        }
    }
}
