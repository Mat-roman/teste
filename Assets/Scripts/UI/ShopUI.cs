using UnityEngine;

public class ShopUI : MonoBehaviour
{
    private NPCBase _activeShopkeeper;

    public void OpenShop(NPCBase npc)
    {
        _activeShopkeeper = npc;
    }

    public NPCBase GetActiveShopkeeper()
    {
        return _activeShopkeeper;
    }
}
