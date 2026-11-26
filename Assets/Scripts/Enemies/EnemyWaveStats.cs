// What the WaveManager hands every enemy it spawns.
// Step 3 adds the health/speed/damage/scrap multipliers here.
public struct EnemyWaveStats
{
    public bool huntPlayer;   // chase from anywhere on the map, not just inside detection range
}

// Any enemy component that reacts to wave settings implements this.
// The WaveManager calls it on every component of a new enemy that has it.
public interface IWaveScalable
{
    void ApplyWaveStats(EnemyWaveStats stats);
}