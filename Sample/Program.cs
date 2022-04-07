﻿using PokemonBDSPRNGLibrary.RestoreSeed;
using PokemonBDSPRNGLibrary.StarterRNG;
using static System.Console;

uint RandomUint()
{
    var random = new Random();
    return (uint)random.Next(0x10000) << 16 | (uint)random.Next(0x10000);
}
(uint, uint, uint, uint) GenerateSeed()
{
    return (RandomUint(), RandomUint(), RandomUint(), RandomUint());
}

const int MODE = 2; // 0: ゴンベのデモ, 1: 主人公瞬きのデモ, 2:主人公瞬きからの再特定のデモ

if(MODE == 0)
{
    const float esp = 0;
    WriteLine($"esp = {esp}[sec]");

    while (true)
    {
        var seed = GenerateSeed();
        var inverter = new MunchlaxInverter(esp);

        (uint, uint, uint, uint) restored;

        var rand = seed;
        while (true)
        {
            // 観測値を取得
            // 0.1以内でランダムに誤差を挿入する
            var interval = rand.BlinkMunchlax().Randomize();
            inverter.AddInterval(interval);

            if (inverter.TryRestoreState(out restored))
                break;
        }

        WriteLine();

        WriteLine($"expected: {seed.ToHexString()}");
        WriteLine($"restored: {restored.ToHexString()}");
        WriteLine(restored == seed ? "Successfully restored." : "Failed...");
        WriteLine($"blink: {inverter.BlinkCount} times");
        ReadKey();
        WriteLine();
    }
}
else if(MODE == 1)
{
    while (true)
    {
        var seed = GenerateSeed();
        var inverter = new PlayerBlinkInverter();

        (uint, uint, uint, uint) restored;

        var rand = seed;
        while (true)
        {
            // 観測値を取得
            var blink = rand.BlinkPlayer();
            inverter.AddBlink(blink);

            Write(blink == PlayerBlink.None ? "- " : blink == PlayerBlink.Single ? "s " : "d ");

            if (inverter.TryRestoreState(out restored))
                break;
        }

        WriteLine();

        WriteLine($"expected: {seed.ToHexString()}");
        WriteLine($"restored: {restored.ToHexString()}");
        WriteLine(restored == seed ? "Successfully restored." : "Failed...");
        WriteLine($"blink roll: {inverter.BlinkCount} times");
        ReadKey();
        WriteLine();
    }
}
else if(MODE == 2)
{
    
    while (true)
    {
        var seed = GenerateSeed();
        var seacher = new PlayerLinearSearch();

        uint idx;
        (uint, uint, uint, uint) restored;

        var rand = seed;

        var adv = 82782;
        for (int i = 0; i < adv; i++) rand.BlinkPlayer();

        var prev = -1;
        var count = 0;
        
        while (true)
        {
            adv++;
            // 観測値を取得
            var blink = rand.BlinkPlayer();
            if (blink != PlayerBlink.None)
            {
                var interval = adv - prev;
                if (prev != -1)
                {
                    count++;
                    seacher.AddInterval(interval);
                }
                prev = adv;
            }

            Write(blink == PlayerBlink.None ? "- " : blink == PlayerBlink.Single ? "s " : "d ");

            if (count > 8)
            {
                (idx, restored) = seacher.Search(seed, 100000u).FirstOrDefault();
                break;
            }
        }

        WriteLine();

        WriteLine($"expected: {prev.ToString()}");
        WriteLine($"index: {idx.ToString()}");
        WriteLine(adv == idx ? "Successfully restored." : "Failed...");
        //WriteLine($"blink roll: {inverter.BlinkCount} times");
        ReadKey();
        WriteLine();
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
