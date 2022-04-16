using System;
using System.Collections.Generic;
using System.Text;
using PokemonPRNG.XorShift128;

namespace PokemonBDSPRNGLibrary.StarterRNG
{
    public static class IDGeneratorExt
    {
        public static (uint rawID, uint id_6dec) GenerateID(this (uint s0, uint s1, uint s2, uint s3) state)
        {
            var raw = state.GetRand();
            return (raw, raw % 1_000_000);
        }

        public static float BlinkMunchlax(this ref (uint s0, uint s1, uint s2, uint s3) state, float munchlaxBlink = 0.285f)
        {
            return state.GetRand_f(3.0f, 12.0f) + munchlaxBlink;
        }
    }
}
