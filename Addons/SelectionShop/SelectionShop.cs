using AkaitoAi.Advertisement;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectionShop : MonoBehaviour
{
    public Shop[] shops;

    public static event Action OnCashChanged;

    [Serializable]
    public class Shop
    {
        [Header("Data")]
        public string name = "Default";
        public string bankKey = "TotalCash";
        public int rewardCoins = 1000;
        public RewardType rewardShopType = RewardType.None;
        public ShopData[] items;
        private int currentIndex = 0;

        public string SelectKey => "Selection" + name;
        public string TryOnceKey => "TryOnce" + name;
        public string BoughtKey => "Bought" + name;

        [Header("UI")]
        public Button selectButton;
        public Button buyButton;
        public Button tryOnceButton;
        public Button incrementButton, decrementButton;
        public Button rewardCoinsButton;
        public GameObject lockObject;
        public GameObject priceContainer;
        public Text priceText;
        public Image[] specsFillers;
        public Text[] specsTexts;
        public Text notification;

        [Serializable]
        public class ShopData
        {
            public GameObject selectable;
            public float[] specs;
            public int price;
        }

        public void Init()
        {
            SelectionShop.OnCashChanged -= CheckStatus;
            SelectionShop.OnCashChanged += CheckStatus;

            currentIndex = PlayerPrefs.GetInt(SelectKey, 0);

            for (int i = 0; i <= items.Length - 1; i++)
                PlayerPrefs.SetInt(TryOnceKey + i, 0);

            int length = items.Length;
            for (int i = 0; i < length; i++)
            {
                items[i].selectable.SetActive(false);

                if (items[i].price <= 0)
                    PlayerPrefs.SetInt(BoughtKey + i, 1);
            }

            items[currentIndex].selectable.SetActive(true);

            UpdateSpecsUI(items[currentIndex]);
            CheckStatus();

            if (selectButton != null) {/* selectButton.onClick.RemoveAllListeners();*/ selectButton.onClick.AddListener(OnSelect); }
            if (buyButton != null) { /*buyButton.onClick.RemoveAllListeners();*/ buyButton.onClick.AddListener(OnBuy); }
            if (tryOnceButton != null) { /*tryOnceButton.onClick.RemoveAllListeners(); */ tryOnceButton.onClick.AddListener(OnTryOnce); }
            if (rewardCoinsButton != null) { /*tryOnceButton.onClick.RemoveAllListeners(); */ rewardCoinsButton.onClick.AddListener(OnRewardCoins); }
            if (incrementButton != null) { /*incrementButton.onClick.RemoveAllListeners();*/  incrementButton.onClick.AddListener(() => Navigate(1)); }
            if (decrementButton != null) { /*decrementButton.onClick.RemoveAllListeners();*/  decrementButton.onClick.AddListener(() => Navigate(-1)); }

            if (notification != null) { notification.gameObject.SetActive(false); notification.text = ""; }
        }

        private void OnSelect()
        {
            PlayerPrefs.SetInt(SelectKey, currentIndex);
        }

        private void OnBuy()
        {
            bool canBuy = PlayerPrefs.GetInt(bankKey)
            >= items[currentIndex].price;

            if (!canBuy)
            {
                if (notification != null) { notification.text = "Not enough coins".ToUpper(); notification.gameObject.SetActive(true); }
                DOVirtual.DelayedCall(2.5f, () => { if (notification != null) { notification.gameObject.SetActive(false); notification.text = ""; } });

                return;
            }

            PlayerPrefs.SetInt(BoughtKey + currentIndex, 1);

            int totalCoins = PlayerPrefs.GetInt(bankKey);
            int chargedCoins = totalCoins - items[currentIndex].price;

            PlayerPrefs.SetInt(bankKey, chargedCoins);
            CheckStatus();

            if (notification != null) { notification.text = "Permanently unlocked".ToUpper(); notification.gameObject.SetActive(true); }
            DOVirtual.DelayedCall(2.5f, () => { if (notification != null) { notification.gameObject.SetActive(false); notification.text = ""; } });

            SelectionShop.OnCashChanged?.Invoke();
        }

        public void UnlockAll()
        {
            int length = items.Length;
            for (int i = 0; i < length; i++)
                PlayerPrefs.SetInt(BoughtKey + i, 1);

            CheckStatus();
        }

        private void CheckStatus()
        {
            bool isUnlocked = PlayerPrefs.GetInt(BoughtKey + currentIndex) == 1 ||
                PlayerPrefs.GetInt(TryOnceKey + currentIndex) == 1;

            if (isUnlocked) OnSelect();

            if (tryOnceButton != null) tryOnceButton.gameObject.SetActive(!isUnlocked);
            if (buyButton != null) buyButton.gameObject.SetActive(!isUnlocked);
            if (lockObject) lockObject.SetActive(!isUnlocked);
            if (priceContainer) priceContainer.SetActive(!isUnlocked);
            if (selectButton != null) selectButton.gameObject.SetActive(isUnlocked);

            //if (priceText != null) priceText.text = isUnlocked ? "Purchased" : items[currentIndex].price.ToString("n0");
            DOVirtual.Int(0, items[currentIndex].price, .5f, value =>
            {
                if (priceText != null) priceText.text = isUnlocked ? "Purchased" : "$" + value.ToString();
            });
        }

        private void Navigate(int direction)
        {
            items[currentIndex].selectable.SetActive(false);

            currentIndex += direction;
            currentIndex = (currentIndex + items.Length) % items.Length;

            items[currentIndex].selectable.SetActive(true);

            UpdateSpecsUI(items[currentIndex]);
            CheckStatus();
        }

        private void OnTryOnce()
        {
            //TODO Ads Calling (Mohib)
            AdsWrapper.GetInstance()?.ShowRewardedAds(() =>
            {

                PlayerPrefs.SetInt(TryOnceKey + currentIndex, 1);

                if (notification != null) { notification.text = "Temporarily unlocked".ToUpper(); notification.gameObject.SetActive(true); }
                DOVirtual.DelayedCall(2.5f, () => { if (notification != null) { notification.gameObject.SetActive(false); notification.text = ""; } });

                CheckStatus();

            }, null);
        }

        private void OnRewardCoins()
        {
            //TODO Ads Calling (Mohib)
            AdsWrapper.GetInstance()?.ShowRewardedAds(() =>
            {

                PlayerPrefs.SetInt(bankKey, PlayerPrefs.GetInt(bankKey) + 1000);
                SelectionShop.OnCashChanged?.Invoke();

            }, null);
        }

        private void UpdateSpecsUI(ShopData data)
        {
            FillerTween(data);
            TextTween(data);
        }

        private void FillerTween(ShopData data)
        {
            if (specsFillers == null || specsFillers.Length == 0) return;

            var localFillers = specsFillers;

            for (int i = 0; i < localFillers.Length; i++)
            {
                float currentValue = localFillers[i].fillAmount * 100f;
                float targetValue = data.specs[i];
                int index = i;

                DOVirtual.Float(currentValue, targetValue, 1f, value =>
                {
                    localFillers[index].fillAmount = value / 100f;
                });
            }
        }

        private void TextTween(ShopData data)
        {
            if (specsTexts == null || specsTexts.Length == 0) return;

            var localTexts = specsTexts;

            for (int i = 0; i < localTexts.Length; i++)
            {
                int targetValue = Mathf.FloorToInt(data.specs[i]);
                int index = i;

                DOVirtual.Int(0, targetValue, 1f, value =>
                {
                    localTexts[index].text = value.ToString() + "%";
                });
            }
        }

        public void Dispose()
        {
            SelectionShop.OnCashChanged -= CheckStatus;

            if (notification != null) { notification.gameObject.SetActive(false); notification.text = ""; }
        }
    }

    private void Awake()
    {
        foreach (Shop s in shops) s.Init();

        RewardEvents.OnRewardGranted += RewardShopGranted;
    }

    private void OnDestroy()
    {
        foreach (Shop s in shops) s.Dispose();

        RewardEvents.OnRewardGranted -= RewardShopGranted;
    }

    private void RewardShopGranted(RewardType reward)
    {
        foreach (Shop s in shops)
        {
            if (s.rewardShopType == reward) s.UnlockAll();
        }
    }
}
