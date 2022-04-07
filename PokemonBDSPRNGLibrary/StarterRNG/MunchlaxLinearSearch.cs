using System;
using System.Collections.Generic;
using System.Text;
using PokemonPRNG.XorShift128;

namespace PokemonBDSPRNGLibrary.StarterRNG
{
    public class MunchlaxLinearSearch
    {
        private readonly List<float> intervals = new List<float>();

        public void AddInterval(float interval)
        {
            intervals.Add(interval);
        }

        public IEnumerable<(uint index, (uint s0, uint s1, uint s2, uint s3) state)> Search((uint s0, uint s1, uint s2, uint s3) state, uint max, float epsilon = 0.1f)
        {
            var blinkCache = new float[256]; // 瞬き間隔をキャッシュしておく配列.
            for (int i = 0; i < intervals.Count; i++)
            {
                blinkCache[i] = state.BlinkMunchlax();
            }

            bool check(int k)
            {
                for (int i = 0; i < intervals.Count; i++)
                {
                    var b = blinkCache[(k + i) & 0xFF];
                    if (intervals[i] + epsilon < b || b < intervals[i] - epsilon) return false;
                }

                return true;
            };
            int head = 0, tail = intervals.Count;
            while (head <= max)
            {
                if (check(head++)) yield return ((uint)tail, state);

                blinkCache[tail++ & 0xFF] = state.BlinkMunchlax();
            }
        }
    }
}
