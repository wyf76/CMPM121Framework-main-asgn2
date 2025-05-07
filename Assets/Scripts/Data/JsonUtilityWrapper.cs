using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityWrapper
{
    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }

    public static List<T> FromJsonList<T>(string json)
    {
        string wrappedJson = "{\"items\":" + json + "}";
        return JsonUtility.FromJson<Wrapper<T>>(wrappedJson).items;
    }
}
