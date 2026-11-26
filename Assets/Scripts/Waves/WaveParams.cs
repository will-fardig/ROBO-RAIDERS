using System;

[Serializable]
public class WaveParams
{
    public int waveNumber = 1;

    // Spawning
    public int enemyCount = 10;        // total enemies this round
    public float spawnInterval = 2f;   // seconds between spawn groups
    public int groupSizeMin = 1;
    public int groupSizeMax = 3;
    public int maxAlive = 12;          // cap on enemies alive at once

    // Enemy modifiers (1 = normal)
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float scrapMultiplier = 1f;

    // Pacing
    public float intermission = 8f;    // break AFTER this round

    public WaveParams Clone() => (WaveParams)MemberwiseClone();

    public override string ToString()
    {
        return $"Wave {waveNumber}: {enemyCount} enemies, every {spawnInterval:0.##}s in groups of {groupSizeMin}-{groupSizeMax}, " +
               $"max {maxAlive} alive | HP x{healthMultiplier:0.##}, speed x{speedMultiplier:0.##}, " +
               $"dmg x{damageMultiplier:0.##}, scrap x{scrapMultiplier:0.##} | break {intermission:0.#}s";
    }
}