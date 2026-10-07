using System;
using UnityEngine;

[Serializable]
public class VFXProperties
{
    [Header("Property")]

    [Tooltip("Tên Exposed Property trong VFX Graph")]
    public string propertyName;

    [Tooltip("Kiểu dữ liệu của Property")]
    public PropertyType propertyType;

    [Tooltip("Có áp dụng Quality Scale hay không")]
    public bool enableScaling = true;

    //==================================================
    // Cached High Value
    // Được Binder tự động đọc từ VFX Graph.
    //==================================================

    [HideInInspector]
    public float highFloatValue;

    [HideInInspector]
    public int highIntValue;

    [HideInInspector]
    public bool highBoolValue = true;
    public bool mediumBoolValue = true;
    public bool lowBoolValue = false;
}