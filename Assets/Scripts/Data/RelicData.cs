using UnityEngine;

[System.Serializable]
public class RelicData
{
    public string name;
    public int sprite;
    public TriggerData trigger;
    public EffectData effect;
}

[System.Serializable]
public class TriggerData
{
    public string description;
    public string type;
    public string amount;   
}

[System.Serializable]
public class EffectData
{
    public string description;
    public string type;
    public string amount;
    public string until; 
}