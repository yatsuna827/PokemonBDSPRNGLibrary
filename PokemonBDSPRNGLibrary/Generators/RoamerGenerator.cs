using System.Linq;
using PokemonPRNG.XorShift128;
using PokemonPRNG.Xoroshiro128p.BDSP;
using PokemonStandardLibrary.Gen8;
using PokemonStandardLibrary;

namespace PokemonBDSPRNGLibrary.Generators
{
    public class RoamerGenerator : IGeneratable<Pokemon.Individual>, IGeneratable<Pokemon.Individual, Synchronize>
    {
        private readonly bool _neverShiny;
        private readonly uint _flawlessIVs;
        private readonly Pokemon.Species _species;
        private readonly uint _lv;

        private readonly uint _tsv;

        public Pokemon.Individual Generate((uint S0, uint S1, uint S2, uint S3) seed)
        {
            var ec = seed.GetRand();

            var rng = ec.Initialize();

            var pid = rng.GeneratePID(_tsv, _neverShiny);
            var ivs = rng.GenerateIVs(_flawlessIVs);

            var ability = rng.GetRand(2);
            var gender = rng.GenerateGender(_species.GenderRatio);
            var nature = (Nature)rng.GetRand(25);

            var height = (byte)(rng.GetRand(129) + rng.GetRand(128));
            var weight = (byte)(rng.GetRand(129) + rng.GetRand(128));

            return _species
                .GetIndividual(_lv, ivs, ec, pid, nature, ability, gender, height, weight)
                .SetShinyType(pid.ToShinyValue().ToShinyType(_tsv));
        }

        public Pokemon.Individual Generate((uint S0, uint S1, uint S2, uint S3) seed, Synchronize synchronize)
        {
            var ec = seed.GetRand();

            var rng = ec.Initialize();

            var pid = rng.GeneratePID(_tsv, _neverShiny);
            var ivs = rng.GenerateIVs(_flawlessIVs);

            var ability = rng.GetRand(2);
            var gender = rng.GenerateGender(_species.GenderRatio);
            var nature = (uint)synchronize.FixedNature < 25 ? synchronize.FixedNature : (Nature)rng.GetRand(25);

            var height = (byte)(rng.GetRand(129) + rng.GetRand(128));
            var weight = (byte)(rng.GetRand(129) + rng.GetRand(128));

            return _species
                .GetIndividual(_lv, ivs, ec, pid, nature, ability, gender, height, weight)
                .SetShinyType(pid.ToShinyValue().ToShinyType(_tsv));
        }

        public RoamerGenerator(string name, uint lv, uint tsv, uint flawlessIVs = 3, bool neverShiny = false)
            => (_species, _lv, _flawlessIVs, _neverShiny, _tsv) = (Pokemon.GetPokemon(name), lv, flawlessIVs, neverShiny, tsv);
    }

    static class RoamerGenerationExt
    {
        public static uint GeneratePID(ref this (ulong S0, ulong S1) rng, uint tsv, bool neverShiny = false)
        {
            var tempTSV = rng.GetRand().ToShinyValue();
            var pid = rng.GetRand();
            var psv = pid.ToShinyValue();

            var shinyType = neverShiny ? ShinyType.NotShiny : psv.ToShinyType(tempTSV);
            if (shinyType == 0 && psv.ToShinyType(tsv) != 0)
                pid ^= 0x10000000; // Antishiny
            else if (shinyType != 0 && psv.ToShinyType(tsv) == 0)
                pid = (tsv ^ pid) << 16 | pid & 0xFFFF;

            return pid;
        }

        public static uint[] GenerateIVs(ref this (ulong S0, ulong S1) seed, uint flawlessIVs)
        {
            var ivs = Enumerable.Repeat(32u, 6).ToArray();
            for (int i = 0; i < flawlessIVs; i++)
            {
                while (true)
                {
                    var idx = seed.GetRand(6);
                    if (ivs[idx] == 32)
                    {
                        ivs[idx] = 31;
                        break;
                    }
                }
            }
            for (int i = 0; i < 6; i++)
                if (ivs[i] == 32) ivs[i] = seed.GetRand(32);

            return ivs;
        }

        public static Gender GenerateGender(ref this (ulong S0, ulong S1) seed, GenderRatio ratio)
            => ratio.ToFixedGender() ?? ((seed.GetRand(253) + 1) < (uint)ratio ? Gender.Female : Gender.Male);

    }

}
