using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public GameObject prefab;
    public string abilityName;
    public float cooldownTime;
    public float activationCost;
    public Sprite abilityIcon;

    [TextArea(10, 20)]
    public string Description;

    public virtual void Activate(GameObject user, int SkillLevel)
    {
        // Implement activation logic in derived classes
        Debug.Log(abilityName + " activated by " + user.name);
    }
}