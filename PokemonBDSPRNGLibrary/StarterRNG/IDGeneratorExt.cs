using System;
using System.Collections.Generic;
using System.Text;
using PokemonPRNG.XorShift128;
using static PokemonBDSPRNGLibrary.StarterRNG.RestoreSeedModules.Util;

namespace PokemonBDSPRNGLibrary.StarterRNG
{
    public static class IDGeneratorExt
    {
        public static (uint rawID, uint id_6dec) GenerateID(this (uint s0, uint s1, uint s2, uint s3) state)
        {
            var raw = (state.GetRand() % 0xFFFFFFFF) + 0x80000000;
            return (raw, raw % 1_000_000);
        }

        public static float BlinkMunchlax(this ref (uint s0, uint s1, uint s2, uint s3) state)
        {
            return state.GetRand_f(3.0f, 12.0f) + MUNCHLAX_BLINK;
        }
    }
}
