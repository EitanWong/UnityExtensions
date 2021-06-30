using System;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Condition
{
    public BooleanCondition callback;
    public bool value;
}

[System.Serializable]
public class BooleanCondition : SerializableCallback<bool>
{
}