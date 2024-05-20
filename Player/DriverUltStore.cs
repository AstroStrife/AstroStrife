using System.Collections.Generic;
using Unity.Netcode;

public class DriverUltStore : NetworkBehaviour
{
    public static DriverUltStore Instance { get; private set; }

    public Ability Ahriman;
    public Ability Menhit;
    public Ability Nova;
    public Ability Soteria;
    public Ability Zeus;

    private Dictionary<string, Ability> driverAbilities;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            InitializeDriverAbilities();
        }
    }

    private void InitializeDriverAbilities()
    {
        driverAbilities = new Dictionary<string, Ability>
        {
            { "Ahriman", Ahriman },
            { "Menhit", Menhit },
            { "Nova", Nova },
            { "Soteria", Soteria },
            { "Zeus", Zeus }
        };
    }

    public Ability GetDriverAbility(string driverName)
    {
        driverAbilities.TryGetValue(driverName, out Ability ability);
        return ability;
    }
}