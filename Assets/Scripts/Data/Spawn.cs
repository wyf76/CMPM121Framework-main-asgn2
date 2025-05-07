using System.Collections.Generic;

[System.Serializable]
public class Spawn
{
    public string enemy;
    public string count;
    public List<int> sequence;
    public string delay = "2";
    public string location = "random";
    public string hp = "base";
    public string speed = "base";
    public string damage = "base";
}
