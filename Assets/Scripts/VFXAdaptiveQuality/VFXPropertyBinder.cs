using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
 /// <summary>
    /// Được EffectQualityController gọi một lần khi khởi tạo.
    /// Cache giá trị HIGH hiện tại từ VFX Graph.
    /// </summary>
public class VFXPropertyBinder : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    private bool debugLog = false;
    
    [SerializeField]
    private VisualEffect visualEffect;

    [SerializeField]
    private List<VFXProperties> properties = new();
    public void SetProperties(List<VFXProperties> newProperties)
    {
        properties.Clear();
        properties.AddRange(newProperties);
    }

    public VisualEffect GetVisualEffect()
    {
        if (visualEffect == null)
            visualEffect = GetComponent<VisualEffect>();

        return visualEffect;
    }
   
    public void Initialize()
    {
        if (visualEffect == null)
            visualEffect = GetComponent<VisualEffect>();

        if (visualEffect == null)
        {
            Debug.LogError($"{name} has no VisualEffect.");
            return;
        }

        CacheHighValues();
    }
    private void CacheHighValues()
    {
        foreach (VFXProperties property in properties)
        {
            switch (property.propertyType)
            {
                case PropertyType.Float:

                    if (visualEffect.HasFloat(property.propertyName))
                    {
                        property.highFloatValue =
                            visualEffect.GetFloat(property.propertyName);
                        
                        if (debugLog)
                        {
                            Debug.Log(
                                $"[VFX Binder] [{name}] Cached Float '{property.propertyName}' = {property.highFloatValue}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[{name}] Float property '{property.propertyName}' was not found.");
                    }

                    break;

                case PropertyType.Int:

                    if (visualEffect.HasInt(property.propertyName))
                    {
                        property.highIntValue =
                            visualEffect.GetInt(property.propertyName);
                            
                        if (debugLog)
                        {
                            Debug.Log(
                                $"[VFX Binder] [{name}] Cached Int '{property.propertyName}' = {property.highIntValue}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[{name}] Int property '{property.propertyName}' was not found.");
                    }

                    break;

                case PropertyType.Bool:
                if (visualEffect.HasBool(property.propertyName))
                {
                    property.highBoolValue =
                        visualEffect.GetBool(property.propertyName);
                }

                break;
            }
        }
    }
    /// <summary>
    /// Áp dụng Quality Scale.
    /// </summary>
    public void Apply(QualityLevel level, float scale)
    {
        if (visualEffect == null)
            return;

        foreach (VFXProperties property in properties)
        {
            if (!property.enableScaling)
                continue;

            switch (property.propertyType)
            {
                case PropertyType.Float:

                    if (visualEffect.HasFloat(property.propertyName))
                    {
                        float value = property.highFloatValue * scale;

                        visualEffect.SetFloat(
                            property.propertyName,
                            value);

                        if (debugLog)
                        {
                            Debug.Log(
                                $"[VFX Binder] [{name}] Float '{property.propertyName}' : " +
                                $"{property.highFloatValue} -> {value} (Scale={scale:F2})");
                        }                    
                    }
                    break;

                case PropertyType.Int:

                    if (visualEffect.HasInt(property.propertyName))
                    {
                        int value =
                            Mathf.RoundToInt(property.highIntValue * scale);

                        visualEffect.SetInt(
                            property.propertyName,
                            value);

                        if (debugLog)
                        {
                            Debug.Log(
                                $"[VFX Binder] [{name}] Int '{property.propertyName}' : " +
                                $"{property.highIntValue} -> {value} (Scale={scale:F2})");
                        }                    
                    }
                    break;

                case PropertyType.Bool:
                    if (visualEffect.HasBool(property.propertyName))
                    {
                        bool value =
                            level == QualityLevel.Low
                            ? false
                            : property.highBoolValue;

                        visualEffect.SetBool(
                            property.propertyName,
                            value);
                    }

                break;
            }
        }
    }
}