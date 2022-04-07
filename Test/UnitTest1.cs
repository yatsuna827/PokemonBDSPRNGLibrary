using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using PokemonBDSPRNGLibrary.RestoreSeed;
using PokemonBDSPRNGLibrary.StarterRNG;
using PokemonPRNG.XorShift128;

namespace Test
{
    public class UnitTest1
    {
        private static uint RandomUint()
        {
            var random = new Random();
            return (uint)random.Next(0x10000) << 16 | (uint)random.Next(0x10000);
        }
        private static double RandomRange(double min, double max)
        {
            var random = new Random();
            return (double)random.NextDouble() * (max - min) + min;
        }
        private static (uint, uint, uint, uint) GenerateSeed()
        {
            return (RandomUint(), RandomUint(), RandomUint(), RandomUint());
        }

        [Fact]
        public void TestMunchlaxInverter()
        {
            // 本当はこういうの行儀良くは無いんだけど
            // その場で100件のサンプルケースを生やしてテスト
            var testCases = new List<((uint, uint, uint, uint) correct, (uint, uint, uint, uint) restored, float[] intervals)>();

            for (int i = 0; i < 100; i++)
            {
                var seed = GenerateSeed();
                var intervals = new List<float>();
                var inverter = new MunchlaxInverter();

                (uint, uint, uint, uint) restored;

                var rand = seed;
                while (true)
                {
                    // 観測値を取得
                    // 0.1以内でランダムに誤差を挿入する
                    var interval = rand.BlinkMunchlax().Randomize();
                    intervals.Add(interval);
                    inverter.AddInterval(interval);

                    if (inverter.TryRestoreState(out restored))
                        break;
                }
                testCases.Add((seed, restored, intervals.ToArray()));
            }

            testCases.TrueForAll(_ => _.restored == _.correct);
        }

        [Fact]
        public void TestPlayerInverter()
        {
            // 本当はこういうの行儀良くは無いんだけど
            // その場で100件のサンプルケースを生やしてテスト
            var testCases = new List<((uint, uint, uint, uint) correct, (uint, uint, uint, uint) restored)>();

            for (int i = 0; i < 100; i++)
            {
                var seed = GenerateSeed();
                var inverter = new PlayerBlinkInverter();

                (uint, uint, uint, uint) restored;

                var rand = seed;
                while (true)
                {
                    var blink = rand.BlinkPlayer();
                    inverter.AddBlink(blink);

                    if (inverter.TryRestoreState(out restored))
                        break;
                }
                testCases.Add((seed, restored));
            }

            testCases.TrueForAll(_ => _.restored == _.correct);
        }

        [Fact]
        public void TestGetNextPlayerBlink()
        {
            var seed = GenerateSeed();
            var rand = seed;

            var idx = rand.GetNextPlayerBlink();
            rand.Equals(seed.Next(idx));
        }

        [Fact]
        public void TestMunchlaxLinearSearch()
        {
            var seed = GenerateSeed();
            var rand = seed.Next(827);

            var searcher = new MunchlaxLinearSearch();

            searcher.AddInterval(rand.BlinkMunchlax().Randomize());
            searcher.AddInterval(rand.BlinkMunchlax().Randomize());
            searcher.AddInterval(rand.BlinkMunchlax().Randomize());
            searcher.AddInterval(rand.BlinkMunchlax().Randomize());
            searcher.AddInterval(rand.BlinkMunchlax().Randomize());

            var (i, restored) = searcher.Search(seed, 1000).FirstOrDefault();

            rand.Equals(restored);
            rand.Equals(seed.Next(i));
        }

        [Fact]
        public void TestPlayerLinearSearch()
        {
            var seed = GenerateSeed();
            var rand = seed.Next(827);
            var adv = 827u + rand.GetNextPlayerBlink();

            var searcher = new PlayerLinearSearch();
            for(int i=0; i<8; i++) 
            {
                // 観測値を取得
                var interval = rand.GetNextPlayerBlink(out var blink);
                searcher.AddInterval(interval);
                adv += interval;
            }

            var (index, restored) = searcher.Search(seed, 1000).FirstOrDefault();

            rand.Equals(restored);
            rand.Equals(seed.Next(index));
        }

        [Fact]
        public void TestPlayerLinearSearchNoisy()
        {
            var seed = GenerateSeed();
            var adv = 8270;
            var rand = seed.Next((uint)adv);

            var searcher = new PlayerLinearSearch();
            var prev = -1;
            var count = 0;

            var pktimer = RandomRange(0.0, 12.0);
            var t = 0.0;
            for (int i = 0; count < 8; i++)
            {
                t += 61.0 / 60.0;
                if (pktimer < t)
                {
                    adv++;
                    pktimer += rand.BlinkPokemon();
                }

                adv++;
                if (rand.BlinkPlayer() != PlayerBlink.None)
                {
                    if (prev != -1)
                    {
                        var interval = i - prev;
                        searcher.AddInterval((uint)interval);
                        count++;
                    }
                    prev = i;
                }
            }

            var (index, restored) = searcher.Search(seed, 10000).FirstOrDefault();

            rand.Equals(restored);
            rand.Equals(seed.Next(index));
        }
    }

    static class Ext
    {
        public static float Randomize(this float value, float ep = 0.1f, float min = 3.0f + 0.285f, float max = 12.0f + 0.285f)
        {
            var random = new Random();
            value += (float)(random.NextDouble() * ep) * (random.Next() % 2 == 0 ? 1 : -1);

            if (value < min) value = min;
            if (value > max) value = max;

            return value;
        }

        public static string ToHexString(this (uint s1, uint s2, uint s3, uint s4) state)
            => $"(0x{state.s1:X8}, 0x{state.s2:X8}, 0x{state.s3:X8}, 0x{state.s4:X8})";
    }

}