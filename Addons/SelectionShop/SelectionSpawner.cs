using UnityEngine;

public class SelectionSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ShopRegistry
    {
        [Tooltip("Must match the exact string Name given to the Shop in SelectionShop.")]
        public string name = "Default";

        [Tooltip("Drag the item prefabs or scene instances here in the EXACT same order as they appear in the Shop.")]
        public GameObject[] items;
    }

    [Header("Shop Registries")]
    [SerializeField] private ShopRegistry[] shops;

    private void Start()
    {
        ApplySelections();
    }

    public void ApplySelections()
    {
        if (shops == null || shops.Length == 0) return;

        foreach (ShopRegistry shop in shops)
        {
            if (shop.items == null || shop.items.Length == 0) continue;

            string selectKey = "Selection" + shop.name;

            int selectedIndex = PlayerPrefs.GetInt(selectKey, 0);

            for (int i = 0; i < shop.items.Length; i++)
            {
                if (shop.items[i] == null) continue;

                bool shouldBeActive = (i == selectedIndex);
                shop.items[i].SetActive(shouldBeActive);
                shop.items[i].transform.SetParent(null);
            }
        }
    }
}