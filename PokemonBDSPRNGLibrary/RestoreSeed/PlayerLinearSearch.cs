using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonBDSPRNGLibrary.RestoreSeed
{
    public class PlayerLinearSearch
    {
        private readonly List<uint> intervals = new List<uint>();
        public int BlinkCount { get => intervals.Count; }

        public void AddInterval(uint interval)
        {
            intervals.Add(interval);
        }
        public IEnumerable<(uint index, (uint S0, uint S1, uint S2, uint S3) state)> Search((uint S0, uint S1, uint S2, uint S3) state, uint max)
        {
            var indexQueue = new Queue<uint>();
            var intervalQueue = new Queue<uint>();

            var idx = state.GetNextPlayerBlink();
            for(int i = 0; i < intervals.Count; i++)
            {
                var interval = state.GetNextPlayerBlink();
                idx += interval;

                indexQueue.Enqueue(idx);
                intervalQueue.Enqueue(interval);
            }
            
            for (int head = 0, tail = intervals.Count; head <= max; head++, tail++)
            {
                if (intervals.SequenceEqual(intervalQueue)) yield return (idx, state);

                var interval = state.GetNextPlayerBlink();
                idx += interval;
                indexQueue.Enqueue(idx);
                intervalQueue.Enqueue(interval);
                indexQueue.Dequeue();
                intervalQueue.Dequeue();
            }
        }

        public IEnumerable<(uint index, double nextPokeBlink, (uint S0, uint S1, uint S2, uint S3) state)> SearchInNoisy((uint S0, uint S1, uint S2, uint S3) state, uint max, double dt = 1.0/60.0)
        {
            for (uint i = 1; i <= max; i++)
            {
                if (state.BlinkPlayer() == PlayerBlink.None) continue;

                for (var offset = 0.0; offset < 12.3; offset += dt)
                {
                    var rand = state;
                    if (CheckNoisy(ref rand, out var advance, intervals, offset, out var rest))
                        yield return (i + advance, rest, rand);
                }
            }
        }

        private static bool CheckNoisy(ref (uint S0, uint S1, uint S2, uint S3) state, out uint advance, in IEnumerable<uint> intervals, double pkTimerOffset, out double restTimer)
        {
            advance = 0;
            restTimer = 0.0;
            var pkTimer = pkTimerOffset;
            foreach (var observed in intervals)
            {
                var interval = 0;
                while (true)
                {
                    if (++interval > observed) return false;

                    // ポケモンが瞬きをするフレームの場合
                    pkTimer -= 61.0 / 60.0;
                    if (pkTimer <= 0)
                    {
                        advance++;
                        pkTimer += state.BlinkPokemon();
                    }

                    advance++;
                    if (state.BlinkPlayer() != PlayerBlink.None)
                    {
                        if (interval != observed) return false;

                        break;
                    }
                }
            }

            restTimer = pkTimer;
            return true;
        }  
    }
}