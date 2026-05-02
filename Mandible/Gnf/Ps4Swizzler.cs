using System;

namespace Mandible.Gnf;

public static class Ps4Swizzler
{
    public static void PerformSwizzle
    (
        ReadOnlySpan<byte> input,
        Span<byte> output,
        int width,
        int height,
        int blockSize,
        bool unswizzle
    )
    {
        //byte[] processed = new byte[input.Length];
        int heightTexels = height / 4;
        int heightTexelsAligned = (heightTexels + 7) / 8;
        int widthTexels = width / 4;
        int widthTexelsAligned = (widthTexels + 7) / 8;
        int dataIndex = 0;

        for (int y = 0; y < heightTexelsAligned; y++)
        {
            for (int x = 0; x < widthTexelsAligned; x++)
            {
                for (int t = 0; t < 64; t++)
                {
                    int pixelIndex = Morton(t, 8, 8);
                    int offsetY = y * 8 + pixelIndex / 8;
                    int offsetX = x * 8 + pixelIndex % 8;

                    if (offsetX < widthTexels && offsetY < heightTexels)
                    {
                        int destPixelIndex = offsetY * widthTexels + offsetX;
                        int destIndex = blockSize * destPixelIndex;

                        // The reference implementation resized the array if it were too short to fit the blockSize
                        // at the target index. I don't believe this is necessary; swizzling only rearranges texels,
                        // it should not affect the byte size of the mip.

                        if (unswizzle && dataIndex < input.Length)
                            input.Slice(dataIndex, blockSize).CopyTo(output[destIndex..]);
                        else if (destIndex < input.Length)
                            input.Slice(destIndex, blockSize).CopyTo(output[dataIndex..]);
                    }

                    dataIndex += blockSize;
                }
            }
        }
    }

    private static int Morton(int t, int sx, int sy)
    {
        int num1;
        int num2 = num1 = 1;
        int num3 = t;
        int num4 = sx;
        int num5 = sy;
        int num6 = 0;
        int num7 = 0;

        while (num4 > 1 || num5 > 1)
        {
            if (num4 > 1)
            {
                num6 += num2 * (num3 & 1);
                num3 >>= 1;
                num2 *= 2;
                num4 >>= 1;
            }
            if (num5 > 1)
            {
                num7 += num1 * (num3 & 1);
                num3 >>= 1;
                num1 *= 2;
                num5 >>= 1;
            }
        }

        return num7 * sx + num6;
    }
}
