using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonBDSPRNGLibrary.StarterRNG.RestoreSeedModules
{
    internal static class Util
    {
        public static int CountConfidenceBits(this uint rand, uint epsilon)
        {
            for (int i = 22; i >= 0; i--)
            {
                var mask = (1u << i);
                var r = rand & (2 * mask - 1);

                var diff = (mask > r) ? (mask - r) : (r - mask);
                if (diff <= epsilon) return 22 - i;
            }

            return 22;
        }

        public static uint GetRawInt(this float rand)
        {
            var r = 12.0f - (rand - MUNCHLAX_BLINK);
            if (r < 0) r = 0;

            var raw = (uint)(r / 9.0f * 8388607.0f);
            if (raw > 0x7F_FFFF) raw = 0x7F_FFFF;

            return raw;
        }

        public static uint GetBits(this uint rand, int n) => (rand >> (23 - n)) & ((1u << n) - 1);

        public const float MUNCHLAX_BLINK = 0.285f;
    }
}
