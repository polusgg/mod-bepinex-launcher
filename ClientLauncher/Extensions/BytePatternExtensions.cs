using System.Linq;

namespace ClientLauncher.Extensions
{
    public static class BytePatternExtensions
    {
        public static int IndexOfPattern(this byte[] source, byte[] pattern)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                    return i;
            }

            return -1;
        }
    }
}