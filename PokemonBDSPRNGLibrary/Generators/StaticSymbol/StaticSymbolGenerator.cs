using System;
using System.Collections.Generic;
using System.Linq;
using PokemonPRNG.XorShift128;
using PokemonStandardLibrary.CommonExtension;
using PokemonStandardLibrary.Gen8;
using PokemonStandardLibrary;

namespace PokemonBDSPRNGLibrary.Generators
{
    public class StaticSymbolGenerator
    {
        private readonly bool neverShiny;
        private readonly uint flawlessIVs;
        private readonly Pokemon.Species species;
        private readonly uint lv;

        public Pokemon.Individual Generate((uint s0, uint s1, uint s2, uint s3) seed)
        {
            var ec = seed.GetRand();
            var tempTSV = seed.GetRand().ToShinyValue();
            var pid = seed.GetRand();
            var psv = pid.ToShinyValue();

            var shinyType = neverShiny ? ShinyType.NotShiny : psv.ToShinyType(tempTSV);

            var ivs = seed.GenerateIVs(flawlessIVs);

            var ability = seed.GetRand(2);
            var gender = seed.GenerateGender(species.GenderRatio);
            var nature = (Nature)seed.GetRand(25);

            return species.GetIndividual(lv, ivs, ec, pid, nature, ability, gender).SetShinyType(shinyType);
        }

        public Pokemon.Individual Generate((uint s0, uint s1, uint s2, uint s3) seed, in Synchronize synchronize)
        {
            var ec = seed.GetRand();
            var tempTSV = seed.GetRand().ToShinyValue();
            var pid = seed.GetRand();
            var psv = pid.ToShinyValue();

            var shinyType = neverShiny ? ShinyType.NotShiny : psv.ToShinyType(tempTSV);

            var ivs = seed.GenerateIVs(flawlessIVs);

            var ability = seed.GetRand(2);
            var gender = seed.GenerateGender(species.GenderRatio);
            var nature = (uint)synchronize.FixedNature < 25 ? synchronize.FixedNature : (Nature)seed.GetRand(25);

            return species.GetIndividual(lv, ivs, ec, pid, nature, ability, gender).SetShinyType(shinyType);
        }

        public Pokemon.Individual Generate((uint s0, uint s1, uint s2, uint s3) seed, in CuteCharm cuteCharm)
        {
            var ec = seed.GetRand();
            var tempTSV = seed.GetRand().ToShinyValue();
            var pid = seed.GetRand();
            var psv = pid.ToShinyValue();

            var shinyType = neverShiny ? ShinyType.NotShiny : psv.ToShinyType(tempTSV);

            var ivs = seed.GenerateIVs(flawlessIVs);

            var ability = seed.GetRand(2);
            var gender = seed.GenerateGender(species.GenderRatio, cuteCharm.FixedGender);
            var nature = (Nature)seed.GetRand(25);

            return species.GetIndividual(lv, ivs, ec, pid, nature, ability, gender).SetShinyType(shinyType);
        }

        public StaticSymbolGenerator(string name, uint lv, uint flawlessIVs = 0, bool neverShiny = false)
            => (this.species, this.lv, this.flawlessIVs, this.neverShiny) = (Pokemon.GetPokemon(name), lv, flawlessIVs, neverShiny);
    }

    public readonly struct Synchronize
    {
        public Nature FixedNature { get; }
        public Synchronize(Nature nature)
            => FixedNature = nature;
    }
    public readonly struct CuteCharm
    {
        public Gender FixedGender { get; }
        public CuteCharm(Gender gender)
            => FixedGender = gender;
    }

    static class GenerationExt
    {
        public static uint ToShinyValue(this uint val)
            => (val & 0xFFFF) ^ (val >> 16);

        public static ShinyType ToShinyType(this uint psv, uint tsv)
        {
            var sv = psv ^ tsv;
            if (sv >= 16) return ShinyType.NotShiny;

            return sv == 0 ? ShinyType.Square : ShinyType.Star;
        }

        public static uint[] GenerateIVs(ref this (uint s0, uint s1, uint s2, uint s3) seed, uint flawlessIVs)
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

        public static Gender? ToFixedGender(this GenderRatio ratio)
        {
            switch (ratio)
            {
                case GenderRatio.Genderless: return Gender.Genderless;
                case GenderRatio.FemaleOnly: return Gender.Female;
                case GenderRatio.MaleOnly: return Gender.Male;
                default: return null;
            }
        }

        public static Gender GenerateGender(ref this (uint s0, uint s1, uint s2, uint s3) seed, GenderRatio ratio)
            => ratio.ToFixedGender() ?? ((seed.GetRand(253) + 1) < (uint)ratio ? Gender.Female : Gender.Male);

        public static Gender GenerateGender(ref this (uint s0, uint s1, uint s2, uint s3) seed, GenderRatio ratio, Gender cuteCharmGender)
            => ratio.ToFixedGender() ??
                ((cuteCharmGender != Gender.Genderless && seed.GetRand(3) != 0) ? cuteCharmGender :
                ((seed.GetRand(253) + 1) < (uint)ratio ? Gender.Female : Gender.Male));
    }
}
