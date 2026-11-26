using System;
using UnityEngine;

// One number that changes as waves go up.
//   Flat        = always baseValue
//   Linear      = baseValue + perWave * (wave - 1)
//   Exponential = baseValue * (1 + perWave) ^ (wave - 1)   (perWave 0.1 = +10% a wave)
//   Curve       = read the curve at x = wave
[Serializable]
public class ScalingStat
{
    public enum Mode { Flat, Linear, Exponential, Curve }

    public Mode mode = Mode.Flat;
    public float baseValue = 1f;
    public float perWave = 0f;
    public AnimationCurve curve;
    public float min = 0f;
    public float max = 9999f;

    public ScalingStat(float baseValue, float min, float max)
    {
        this.baseValue = baseValue;
        this.min = min;
        this.max = max;
        curve = AnimationCurve.Linear(1f, baseValue, 50f, baseValue);
    }

    public float Evaluate(int wave)
    {
        int steps = Mathf.Max(0, wave - 1);
        float v;
        switch (mode)
        {
            case Mode.Linear: v = baseValue + perWave * steps; break;
            case Mode.Exponential: v = baseValue * Mathf.Pow(1f + perWave, steps); break;
            case Mode.Curve: v = curve.Evaluate(wave); break;
            default: v = baseValue; break;
        }
        return Mathf.Clamp(v, min, max);
    }

    public int EvaluateInt(int wave) => Mathf.RoundToInt(Evaluate(wave));
}

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Robo Raiders/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Spawning")]
    public ScalingStat enemyCount = new ScalingStat(10f, 1f, 200f);
    public ScalingStat spawnInterval = new ScalingStat(2f, 0.2f, 10f);
    public ScalingStat groupSizeMin = new ScalingStat(1f, 1f, 8f);
    public ScalingStat groupSizeMax = new ScalingStat(3f, 1f, 8f);
    public ScalingStat maxAlive = new ScalingStat(12f, 1f, 40f);

    [Header("Enemy Modifiers")]
    public ScalingStat healthMultiplier = new ScalingStat(1f, 0.1f, 20f);
    public ScalingStat speedMultiplier = new ScalingStat(1f, 0.3f, 3f);
    public ScalingStat damageMultiplier = new ScalingStat(1f, 0.1f, 10f);
    public ScalingStat scrapMultiplier = new ScalingStat(1f, 0f, 10f);

    [Header("Pacing")]
    public ScalingStat intermission = new ScalingStat(8f, 0f, 60f);

    public WaveParams Evaluate(int wave)
    {
        WaveParams p = new WaveParams
        {
            waveNumber = wave,
            enemyCount = enemyCount.EvaluateInt(wave),
            spawnInterval = spawnInterval.Evaluate(wave),
            groupSizeMin = groupSizeMin.EvaluateInt(wave),
            groupSizeMax = groupSizeMax.EvaluateInt(wave),
            maxAlive = maxAlive.EvaluateInt(wave),
            healthMultiplier = healthMultiplier.Evaluate(wave),
            speedMultiplier = speedMultiplier.Evaluate(wave),
            damageMultiplier = damageMultiplier.Evaluate(wave),
            scrapMultiplier = scrapMultiplier.Evaluate(wave),
            intermission = intermission.Evaluate(wave),
        };
        p.groupSizeMax = Mathf.Max(p.groupSizeMin, p.groupSizeMax);
        return p;
    }

    // Right-click the asset's header in the Inspector to see what waves 1-10 look like
    [ContextMenu("Print Waves 1-10")]
    private void PrintWaves()
    {
        for (int w = 1; w <= 10; w++) Debug.Log(Evaluate(w));
    }
}