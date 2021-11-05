using Mandible.Zng.Deflate;
using Mandible.Zng.Inflate;
using System;
using System.Text;
using Xunit;

namespace Mandible.Tests.Zng
{
    public class ZlibTests
    {
        [Fact]
        public void TestRoundTrip()
        {
            Span<byte> actual = Encoding.UTF8.GetBytes("aaaabbbbccccddddeeeeffffgggghhhhiiiijjjjkkkk");
            Span<byte> deflated = stackalloc byte[actual.Length];
            Span<byte> inflated = stackalloc byte[actual.Length];

            using ZngDeflater deflater = new();
            uint amount = deflater.Deflate(actual, deflated);
            Assert.NotEqual(0u, amount);

            using ZngInflater inflater = new();
            inflater.Inflate(deflated[0..(int)amount], inflated);

            for (int i = 0; i < actual.Length; i++)
                Assert.Equal(actual[i], inflated[i]);
        }
    }
}
