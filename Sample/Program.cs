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

while (true)
{
    var seed = GenerateSeed();
    var inverter = new MunchlaxInverter();

    (uint, uint, uint, uint) restored;

    var rand = seed;
    while (true)
    {
        // 観測値を取得
        // 0.1以内でランダムに誤差を挿入する
        var interval = rand.BlinkMunchlax().Randomize(0.01f);
        inverter.AddInterval(interval);

        if (inverter.TryRestoreState(out restored))
            break;
    }

    WriteLine($"expected: {seed.ToHexString()}");
    WriteLine($"restored: {restored.ToHexString()}");
    WriteLine(restored == seed ? "Successfully restored." : "Failed...");
    WriteLine($"blink: {inverter.BlinkCount} times");
    ReadKey();
    WriteLine();
}

static class Ext
{
    public static float Randomize(this float value, float ep = 0.1f, float min = 3.0f, float max = 12.0f)
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
