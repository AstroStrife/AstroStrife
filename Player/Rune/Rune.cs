using Unity.Netcode;

public enum RuneType
{
    DoubleDamage,
    Haste
}

public class Rune : NetworkBehaviour
{
    public RuneType runeType;
}