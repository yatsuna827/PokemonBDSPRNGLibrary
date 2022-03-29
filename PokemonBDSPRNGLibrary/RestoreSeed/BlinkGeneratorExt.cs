using System;
using System.Collections.Generic;
using System.Text;
using PokemonPRNG.XorShift128;

namespace PokemonBDSPRNGLibrary.RestoreSeed
{
    public enum PlayerBlink
    {
        None,
        Single,
        Double
    }
    public static class BlinkGeneratorExt
    {
        public static PlayerBlink BlinkPlayer(this ref (uint s0, uint s1, uint s2, uint s3) state)
        {
            var r = state.GetRand() & 0xF;
            return r == 0 ? PlayerBlink.Single : r == 1 ? PlayerBlink.Double : PlayerBlink.None;
        }
    }
}
