using UnityEngine;

public enum DamageType
{
    Default,
    Melee,
    Piercing,
    Siege
}

public struct DamageData
{
    public int BaseDamage;
    public DamageType Type;
    public Vector3 SourcePosition; // Posición desde donde se origina el daño, para cálculos de dirección y de daño por elevación
}
