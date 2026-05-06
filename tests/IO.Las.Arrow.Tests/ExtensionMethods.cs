#if !NET6_0_OR_GREATER
namespace Altemiq.IO.Las.Arrow;

public static class ExtensionMethods
{
    private static ReadOnlySpan<byte> Log2DeBruijn =>
    [
        00, 09, 01, 10, 13, 21, 02, 29,
        11, 14, 16, 18, 22, 25, 03, 30,
        08, 12, 20, 28, 15, 17, 24, 07,
        19, 27, 23, 06, 26, 05, 04, 31
    ];
    
    extension(System.Random random)
    {
        public long NextInt64()
        {
            while (true)
            {
                // Get top 63 bits to get a value in the range [0, long.MaxValue], but try again
                // if the value is actually long.MaxValue, as the method is defined to return a value
                // in the range [0, long.MaxValue).
                var result = random.NextUInt64() >> 1;
                if (result != long.MaxValue)
                {
                    return (long)result;
                }
            }
        }
        
        public long NextInt64(long minValue, long maxValue)
        {
            var exclusiveRange = (ulong)(maxValue - minValue);

            if (exclusiveRange > 1)
            {
                // Narrow down to the smallest range [0, 2^bits] that contains maxValue - minValue
                // Then repeatedly generate a value in that outer range until we get one within the inner range.
                var bits = Log2Ceiling(exclusiveRange);
                while (true)
                {
                    var result = random.NextUInt64() >> (sizeof(long) * 8 - bits);
                    if (result < exclusiveRange)
                    {
                        return (long)result + minValue;
                    }
                }
            }

            System.Diagnostics.Debug.Assert(minValue == maxValue || minValue + 1 == maxValue);
            return minValue;
        }
        
        public float NextSingle()
        {
            while (true)
            {
                var f = (float)random.NextDouble();
                if (f < 1.0f) // reject 1.0f, which is rare but possible due to rounding
                {
                    return f;
                }
            }
        }
    
        
        private ulong NextUInt64() =>
            (uint)random.Next(1 << 22) |
            ((ulong)(uint)random.Next(1 << 22) << 22) |
            ((ulong)(uint)random.Next(1 << 20) << 44);
    }
    
    private static int Log2Ceiling(ulong value)
    {
        int result = Log2(value);
        if (PopCount(value) != 1)
        {
            result++;
        }
        
        return result;
        
        static int PopCount(ulong value)
        {
            const ulong C1 = 0x_55555555_55555555ul;
            const ulong C2 = 0x_33333333_33333333ul;
            const ulong C3 = 0x_0F0F0F0F_0F0F0F0Ful;
            const ulong C4 = 0x_01010101_01010101ul;

            value -= (value >> 1) & C1;
            value = (value & C2) + ((value >> 2) & C2);
            value = (((value + (value >> 4)) & C3) * C4) >> 56;

            return (int)value;
        }
    }
    
    private static int Log2(uint value)
    {
        // The 0->0 contract is fulfilled by setting the LSB to 1.
        // Log(1) is 0, and setting the LSB for values > 1 does not change the log2 result.
        value |= 1;

        // Fallback contract is 0->0
        // Fill trailing zeros with ones, e.g. 00010010 becomes 00011111
        value |= value >> 01;
        value |= value >> 02;
        value |= value >> 04;
        value |= value >> 08;
        value |= value >> 16;

        // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
        return Log2DeBruijn[(int)((value * 0x07C4ACDDu) >> 27)];
    }
    
    private static int Log2(ulong value)
    {
        value |= 1;

        uint hi = (uint)(value >> 32);

        if (hi == 0)
        {
            return Log2((uint)value);
        }

        return 32 + Log2(hi);
    }
    
}

#endif