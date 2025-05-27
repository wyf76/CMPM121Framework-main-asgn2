using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ClassSelector : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown classDropdown;

    private Dictionary<string, CharacterClassDefinition> classDefs;

    void Start()
    {
        classDefs = GameDataLoader.Classes;

        classDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var kv in classDefs)
            options.Add(kv.Key);
        classDropdown.AddOptions(options);

        classDropdown.RefreshShownValue(); 

        // Set the default class when the game starts
        if (classDefs.Count > 0)
        {
            var className = classDropdown.options[0].text;
            GameManager.Instance.SetSelectedClass(classDefs[className]);
            Debug.Log($"Default class selected: {className}");
        }
        
        // Listen for when the user changes the selection
        classDropdown.onValueChanged.AddListener(OnClassSelected);
    }

    void OnClassSelected(int index)
    {
        var className = classDropdown.options[index].text;
        var def = classDefs[className];
        
        GameManager.Instance.SetSelectedClass(def);
        Debug.Log($"Class selected: {className}");
    }
}