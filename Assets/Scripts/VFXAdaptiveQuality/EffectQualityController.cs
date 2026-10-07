using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


/// <summary>
/// Quản lý chất lượng của toàn bộ Particle System và
/// các VFXPropertyBinder trong một prefab.
/// Giá trị ban đầu được xem là chất lượng HIGH.
/// </summary>
public class EffectQualityController : MonoBehaviour, IQualityScalable
{
    #region Particle Data

    [System.Serializable]
    private class ParticleData
    {
        public ParticleSystem particleSystem;

        public float emission;
        public float lifetime;
        public float startSpeed;
        public float simulationSpeed;
        public int maxParticles;
    }

    #endregion

    [Header("Quality Scale")]

    [Range(0.1f, 1f)]
    [SerializeField] private float lowScale = 0.4f;

    [Range(0.1f, 1f)]
    [SerializeField] private float mediumScale = 0.7f;
    [SerializeField] private bool debugLog = false;

    private readonly List<ParticleData> particleList = new();

    private readonly List<VFXPropertyBinder> vfxBinders = new();

    /// <summary>
    /// Được ObjectPoolManager gọi ngay sau AddComponent().
    /// </summary>
    public void Initialize()
    {
        CacheParticleSystems();
        CacheVFXBinders();
    }

    #region Cache

    private void CacheParticleSystems()
    {
        particleList.Clear();

        ParticleSystem[] systems =
            GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in systems)
        {
            var main = ps.main;
            var emission = ps.emission;

            ParticleData data = new ParticleData
            {
                particleSystem = ps,
                emission = emission.rateOverTime.constant,
                lifetime = main.startLifetime.constant,
                startSpeed = main.startSpeed.constant,
                simulationSpeed = main.simulationSpeed,
                maxParticles = main.maxParticles
            };

            particleList.Add(data);
        }
    }

    private void CacheVFXBinders()
    {
        vfxBinders.Clear();

        VisualEffect[] effects =
            GetComponentsInChildren<VisualEffect>(true);

        foreach (VisualEffect effect in effects)
        {
            if (effect == null)
                continue;

            VFXPropertyBinder binder =
                effect.GetComponent<VFXPropertyBinder>();

            if (binder == null)
            {
                binder =
                    effect.gameObject.AddComponent<VFXPropertyBinder>();

                Debug.Log(
                    $"[EffectQualityController] Added VFXPropertyBinder to {effect.name}");
            }

            binder.Initialize();

            vfxBinders.Add(binder);
        }
    }

    #endregion

    public void ApplyQuality(QualityLevel level)
    {
        float scale = GetScale(level);

        //----------------------------------
        // Particle System
        //----------------------------------

        foreach (ParticleData data in particleList)
        {
            if (data.particleSystem == null)
                continue;

            var main = data.particleSystem.main;
            var emission = data.particleSystem.emission;

            main.maxParticles =
                Mathf.RoundToInt(data.maxParticles * scale);

            main.startLifetime =
                data.lifetime * scale;

            main.startSpeed =
                data.startSpeed * scale;

            main.simulationSpeed =
                data.simulationSpeed * scale;

            emission.rateOverTime =
                data.emission * scale;

            LogParticleState(data.particleSystem, scale);
        }

        //----------------------------------
        // VFX Graph
        //----------------------------------

        foreach (VFXPropertyBinder binder in vfxBinders)
        {
            if (binder == null)
                continue;

            binder.Apply(level, scale);       
        }
    }

    private float GetScale(QualityLevel level)
    {
        switch (level)
        {
            case QualityLevel.Low:
                return lowScale;

            case QualityLevel.Medium:
                return mediumScale;

            default:
                return 1f;
        }
    }

    private void LogParticleState(ParticleSystem ps,float scale)
    {
        if (!debugLog)
        return;
        if (ps == null)
        return;
        
        var main = ps.main;
        var emission = ps.emission;

        Debug.Log(
            $"[{ps.name}] " +
            $"Scale={scale:F2} " +
            $"Emission={emission.rateOverTime.constant:F1} " +
            $"Lifetime={main.startLifetime.constant:F2} " +
            $"Speed={main.startSpeed.constant:F2} " +
            $"Simulation={main.simulationSpeed:F2} " +
            $"Max={main.maxParticles}");
    }
}