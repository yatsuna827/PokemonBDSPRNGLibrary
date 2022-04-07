using System;
using System.Collections.Generic;
using System.Text;
using PokemonPRNG.XorShift128;

namespace PokemonBDSPRNGLibrary.RestoreSeed
{
    public class PlayerLinearSearch
    {
        private readonly List<int> intervals = new List<int>();

        public void AddInterval(int interval)
        {
            intervals.Add(interval);
        }
        public IEnumerable<(uint index, (uint s0, uint s1, uint s2, uint s3) state)> Search((uint s0, uint s1, uint s2, uint s3) state, uint max)
        {
            var blinkCache = new int[256];
            var lastBlink = -1;
            var idx = 0;
            for(int i = 0; i < intervals.Count; idx++)
            {
                if (state.BlinkPlayer() != PlayerBlink.None)
                {
                    if (lastBlink != -1)
                    {
                        var b = idx - lastBlink;
                        blinkCache[i] = b;
                    }
                    lastBlink = idx;
                }
            }
            bool check(int k)
            {
                for (int i = 0; i < intervals.Count; i++)
                {
                    var b = blinkCache[(k + i) & 0xFF];
                    if (intervals[i] != b) return false;
                }

                return true;
            }
            int head = 0, tail = intervals.Count;
            while (head <= max)
            {
                while (state.BlinkPlayer() != PlayerBlink.None) idx++;
                var b = idx - lastBlink;
                blinkCache[tail++ & 0xFF] = b;
                lastBlink = idx;
                if (check(head++)) yield return ((uint)tail, state);
            }
        }
    }
}