using DG.Tweening;
using Fusion;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SoundManager;

public class UnoManager : NetworkBehaviour
{
    public static UnoManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("Config")]
    public CardSO CardSO;
    private int InitialHandSize = 6;

    [Header("Player Card List")]
    public UnoCard CardPrefab;
    public RectTransform MyCardTf;
    private List<UnoCard> myCardList = new List<UnoCard>();
    public RectTransform OpponentCardTf;

    [Header("Card Deck")]
    public RectTransform DeckPoint;
    public Image TopCardImage;
    public Button DrawCardButton;
    public Button QuitBtn;

    [Header("Runtime")]
    [Networked] public int CardNumber { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> CurrentDeck => default;
    [Networked] public int WhiteCardCount { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> WhiteHand => default;
    [Networked] public int BlackCardCount { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> BlackHand => default;
    [Networked] public int ReleaseCardCount { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> ReleaseCardDeck => default;
    [Networked] public UnoCardData TopCard { get; set; }
    [Networked] public int NextCardID { get; set; }
    [Networked] public bool IsReleasedCard { get; set; } = false;


    [Header("Response Window")]
    [Networked] public bool IsResponseWindowOpen { get; set; } = false;
    [Networked] public float ResponseWindowStartTime { get; set; }
    [Networked] public UnoCardData PendingCard { get; set; }
    [Networked] public Team PendingCardTeam { get; set; }

    private const float ResponseWindowDuration = 7f;

    private bool isDrawingMultiple = false;
    private Button muteMusicBtn;
    private Slider musicSlider;
    private TextMeshProUGUI musicVolumeText;
    private Button muteAllBtn;

    private void Start()
    {
        if (DrawCardButton != null)
        {
            DrawCardButton.onClick.AddListener(OnDrawCardClicked);
        }

        if (QuitBtn != null)
        {
            QuitBtn.onClick.AddListener(OnQuitClicked);
            CreateMuteButtons();
        }
    }

    private void OnDisable()
    {
        if (DrawCardButton != null)
        {
            DrawCardButton.onClick.RemoveListener(OnDrawCardClicked);
        }

        if (QuitBtn != null)
        {
            QuitBtn.onClick.RemoveListener(OnQuitClicked);
        }
    }

    private void OnDrawCardClicked()
    {
        Rpc_DrawCard(ChessManager.Instance.GetPlayerTeam());
    }

    private void OnQuitClicked()
    {
        ChessManager.Instance.PlayerPressQuitButton();
    }

    private void CreateMuteButtons()
    {
        if (QuitBtn == null) return;

        // Position calculations
        RectTransform quitRect = QuitBtn.GetComponent<RectTransform>();
        float width = quitRect.rect.width;
        float spacing = 15f;
        float offset = width + spacing;

        // 1. Create Mute Music (Setting) Button next to QuitBtn
        muteMusicBtn = Instantiate(QuitBtn, QuitBtn.transform.parent);
        muteMusicBtn.name = "MuteMusicBtn";
        muteMusicBtn.onClick.RemoveAllListeners();
        muteMusicBtn.onClick.AddListener(OnMusicSettingClicked);

        RectTransform musicRect = muteMusicBtn.GetComponent<RectTransform>();
        musicRect.anchoredPosition = new Vector2(quitRect.anchoredPosition.x - offset, quitRect.anchoredPosition.y);

        if (muteMusicBtn.transform.childCount > 0)
        {
            Image musicImg = muteMusicBtn.transform.GetChild(0).GetComponent<Image>();
            if (musicImg != null) musicImg.gameObject.SetActive(false);
        }

        GameObject musicTextGo = new GameObject("Text");
        musicTextGo.transform.SetParent(muteMusicBtn.transform, false);
        var musicLabel = musicTextGo.AddComponent<TextMeshProUGUI>();
        musicLabel.text = "MUSIC\nSETTING";
        musicLabel.fontSize = 14;
        musicLabel.alignment = TextAlignmentOptions.Center;
        musicLabel.color = Color.white;

        RectTransform musicTextRect = musicTextGo.GetComponent<RectTransform>();
        musicTextRect.anchorMin = Vector2.zero;
        musicTextRect.anchorMax = Vector2.one;
        musicTextRect.sizeDelta = Vector2.zero;

        // 2. Create Slider Container (Popup below MuteMusicBtn)
        float sliderWidth = 200f;
        GameObject sliderContainer = new GameObject("MusicVolumeSliderContainer");
        sliderContainer.transform.SetParent(QuitBtn.transform.parent, false);
        
        RectTransform containerRect = sliderContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = quitRect.anchorMin;
        containerRect.anchorMax = quitRect.anchorMax;
        containerRect.pivot = quitRect.pivot;
        // Position directly below MuteMusicBtn
        containerRect.anchoredPosition = new Vector2(musicRect.anchoredPosition.x, quitRect.anchoredPosition.y - width - 10f);
        containerRect.sizeDelta = new Vector2(sliderWidth, 60f);
        sliderContainer.SetActive(false); // Hidden by default

        // 3. Create Label Text inside container
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderContainer.transform, false);
        musicVolumeText = labelObj.AddComponent<TextMeshProUGUI>();
        musicVolumeText.fontSize = 16;
        musicVolumeText.alignment = TextAlignmentOptions.Center;
        musicVolumeText.color = Color.white;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.6f);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = Vector2.zero;

        // 4. Create Slider root
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(sliderContainer.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0.1f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.anchoredPosition = Vector2.zero;
        sliderRect.sizeDelta = Vector2.zero;

        Slider slider = sliderObj.AddComponent<Slider>();

        // 5. Create Slider Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 6. Create Slider Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.anchoredPosition = Vector2.zero;
        fillAreaRect.sizeDelta = new Vector2(-10, 0);

        // 7. Create Slider Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // 8. Create Slider Handle Slide Area
        GameObject handleAreaObj = new GameObject("Handle Slide Area");
        handleAreaObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-20, 0);

        // 9. Create Slider Handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleAreaObj.transform, false);
        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = Color.white;
        
        RectTransform handleRect = handleObj.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.sizeDelta = new Vector2(20, 0);

        // Link Slider fields
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 100f;

        float currentVol = 100f;
        if (SoundsManager.Instance != null)
        {
            currentVol = SoundsManager.Instance.GetMusicVolume() * 100f;
            if (SoundsManager.Instance.IsMusicMuted || SoundsManager.Instance.IsAllMuted)
            {
                currentVol = 0f;
            }
        }
        slider.value = currentVol;
        slider.onValueChanged.AddListener(OnMusicVolumeChanged);
        musicSlider = slider;

        // 10. Create Mute All Button next to MuteMusicBtn
        muteAllBtn = Instantiate(QuitBtn, QuitBtn.transform.parent);
        muteAllBtn.name = "MuteAllBtn";
        muteAllBtn.onClick.RemoveAllListeners();
        muteAllBtn.onClick.AddListener(OnMuteAllClicked);

        RectTransform allRect = muteAllBtn.GetComponent<RectTransform>();
        allRect.anchoredPosition = new Vector2(quitRect.anchoredPosition.x - 2 * offset, quitRect.anchoredPosition.y);

        if (muteAllBtn.transform.childCount > 0)
        {
            Image allImg = muteAllBtn.transform.GetChild(0).GetComponent<Image>();
            if (allImg != null) allImg.gameObject.SetActive(false);
        }

        GameObject allTextGo = new GameObject("Text");
        allTextGo.transform.SetParent(muteAllBtn.transform, false);
        var allTmp = allTextGo.AddComponent<TextMeshProUGUI>();
        allTmp.fontSize = 20;
        allTmp.alignment = TextAlignmentOptions.Center;
        allTmp.color = Color.white;

        RectTransform allTextRect = allTextGo.GetComponent<RectTransform>();
        allTextRect.anchorMin = Vector2.zero;
        allTextRect.anchorMax = Vector2.one;
        allTextRect.sizeDelta = Vector2.zero;

        // Initialize label values
        UpdateMuteButtonLabels(allTmp);
    }

    private void OnMusicSettingClicked()
    {
        if (musicSlider != null && musicSlider.transform.parent != null)
        {
            GameObject container = musicSlider.transform.parent.gameObject;
            container.SetActive(!container.activeSelf);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (SoundsManager.Instance != null)
        {
            if (value > 0f && SoundsManager.Instance.IsMusicMuted)
            {
                SoundsManager.Instance.ToggleMusicMute(); // Auto-unmute if sliding up
            }
            SoundsManager.Instance.SetMusicVolume(value / 100f);
        }
        UpdateVolumeLabel(value);
    }

    private void UpdateVolumeLabel(float value)
    {
        if (musicVolumeText != null)
        {
            if (SoundsManager.Instance != null && (SoundsManager.Instance.IsMusicMuted || SoundsManager.Instance.IsAllMuted))
            {
                musicVolumeText.text = "MUSIC: MUTED";
                musicVolumeText.color = Color.red;
            }
            else
            {
                musicVolumeText.text = $"MUSIC: {(int)value}%";
                musicVolumeText.color = Color.green;
            }
        }
    }

    private void OnMuteAllClicked()
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.ToggleAllMute();
            UpdateAllMuteLabels();
        }
    }

    private void UpdateAllMuteLabels()
    {
        if (muteAllBtn == null) return;
        var allTmp = muteAllBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        UpdateMuteButtonLabels(allTmp);
    }

    private void UpdateMuteButtonLabels(TextMeshProUGUI allTmp)
    {
        if (SoundsManager.Instance == null) return;

        bool isMuted = SoundsManager.Instance.IsMusicMuted || SoundsManager.Instance.IsAllMuted;
        float currentVol = isMuted ? 0f : (SoundsManager.Instance.GetMusicVolume() * 100f);

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            musicSlider.value = currentVol;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        UpdateVolumeLabel(currentVol);

        if (allTmp != null)
        {
            allTmp.text = SoundsManager.Instance.IsAllMuted ? "SOUND\nOFF" : "SOUND\nON";
            allTmp.color = SoundsManager.Instance.IsAllMuted ? Color.red : Color.green;
        }
    }

    public void SetIsReleasedCard(bool value)
    {
        if (Runner.IsServer)
            IsReleasedCard = value;
    }

    public bool IsPlayerReleasedCard()
    {
        return IsReleasedCard;
    }

    public void InitializeDeck()
    {
        if (Runner.IsServer)
        {
            Debug.Log("Initializing Uno Deck");

            CardNumber = 0;
            ReleaseCardCount = 0;

            // Initialize the deck with cards from CardSO
            for (int i = 0; i < CardSO.cards.Length; i++)
            {
                for (int j = 0; j < CardSO.cards[i].amount; j++)
                {
                    CardNumber++;
                    UnoCardData cardData = new UnoCardData
                    {
                        ID = CardNumber,
                        CardType = (int)CardSO.cards[i].type,
                        CardColor = (int)CardSO.cards[i].color,
                        Value = CardSO.cards[i].value,
                    };
                    CurrentDeck.Set(CardNumber - 1, cardData);
                }
            }

            //Shuffle the deck
            for (int i = CardNumber - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                UnoCardData temp = CurrentDeck.Get(i);
                CurrentDeck.Set(i, CurrentDeck.Get(j));
                CurrentDeck.Set(j, temp);
            }

            InitHand();
        }

    }

    private void InitHand()
    {
        if (Runner.IsServer)
        {
            for (int i = 0; i < InitialHandSize; i++)
            {
                WhiteHand.Set(i, CurrentDeck.Get(i));
                BlackHand.Set(i, CurrentDeck.Get(InitialHandSize + i));
            }
            WhiteCardCount = InitialHandSize;
            BlackCardCount = InitialHandSize;

            // Set the top card

            TopCard = CurrentDeck.Get(InitialHandSize * 2);
            NextCardID = InitialHandSize * 2 + 1;

            //Rpc_RenderCard();
            DOVirtual.DelayedCall(0.2f, () => Rpc_RenderCard());
        }
    }

    private void SetTopCard(UnoCardData cardData)
    {
        if (Runner.IsServer)
            TopCard = cardData;

        RenderTopCard();
        UpdateActiveCard();
        Rpc_UpdateDrawCardButton();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_RenderCard()
    {
        myCardList.Clear();

        RenderAllCardList();

        RenderTopCard();
    }

    private void RenderAllCardList()
    {
        DealCards();
    }

    public void DealCards()
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < MyCardTf.childCount; i++)
        {
            Destroy(MyCardTf.GetChild(i).gameObject);
        }

        for (int i = 0; i < OpponentCardTf.childCount; i++)
        {
            Destroy(OpponentCardTf.GetChild(i).gameObject);
        }

        for (int i = 0; i < InitialHandSize; i++)
        {
            int index = i;

            seq.AppendCallback(() =>
            {
                DealOneCard(MyCardTf);
            });

            seq.AppendInterval(0.08f);

            seq.AppendCallback(() =>
            {
                DealOneCard(OpponentCardTf);
            });

            seq.AppendInterval(0.08f);
        }
        seq.AppendInterval(0.3f);
        seq.OnComplete(() =>
        {
            if (ChessManager.Instance.GetPlayerTeam() == Team.White)
            {
                RenderAllHand(WhiteHand, WhiteCardCount, MyCardTf, false);
                RenderAllHand(BlackHand, BlackCardCount, OpponentCardTf, true);
            }
            else
            {
                RenderAllHand(BlackHand, BlackCardCount, MyCardTf, false);
                RenderAllHand(WhiteHand, WhiteCardCount, OpponentCardTf, true);
            }

            RefreshHand(MyCardTf);
            RefreshHand(OpponentCardTf);
            UpdateActiveCard();
            Rpc_UpdateDrawCardButton();
        });
    }

    public void RefreshHand(Transform hand)
    {
        float spacing = 90f;
        int count = hand.childCount;
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;
        int middle = count / 2;

        for (int i = 0; i < count; i++)
        {
            RectTransform card = hand.GetChild(i).GetComponent<RectTransform>();

            Vector3 targetPos = new(startX + i * spacing, 0, 0);
            card.DOLocalMove(targetPos, 0.25f);
            card.DOLocalRotate(Vector3.zero, 0.25f);
        }
    }

    private void DealOneCard(Transform hand)
    {
        var card = Instantiate(CardPrefab, DeckPoint.parent);

        RectTransform cardRect = card.GetComponent<RectTransform>();

        cardRect.position = DeckPoint.position;
        cardRect.localScale = Vector3.one * 0.7f;

        card.transform.SetParent(hand);

        cardRect.DOScale(1f, 0.25f);

        cardRect
            .DOLocalMove(Vector3.zero, 0.3f)
            .SetEase(Ease.OutCubic);
    }

    private void RenderAllHand(NetworkArray<UnoCardData> hand, int length, RectTransform parentTf, bool isOpponent)
    {
        for (int i = 0; i < length; i++)
        {
            UnoCardData cardData = hand.Get(i);
            if (cardData.ID != -1)
            {
                UnoCard cardUI = parentTf.GetChild(i).GetComponent<UnoCard>();
                cardUI.SetCardData(cardData, isOpponent);

                if (!isOpponent)
                {
                    myCardList.Add(cardUI);
                }
            }
        }
    }

    private void RenderAddCard(UnoCardData cardData, RectTransform parentTf, bool isOpponent)
    {
        UnoCard cardUI = Instantiate(CardPrefab, parentTf);
        cardUI.SetCardData(cardData, isOpponent);

        if (!isOpponent)
        {
            myCardList.Add(cardUI);
        }

        // Set vị trí khởi đầu ở DeckPoint
        RectTransform cardRect = cardUI.GetComponent<RectTransform>();
        cardRect.position = DeckPoint.position;
        cardRect.localScale = Vector3.one;
        cardRect.localRotation = Quaternion.identity;

        // Sorting bài ngay lập tức để tránh bị che bởi bài khác khi đang di chuyển
        Canvas canvas = cardRect.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = cardRect.gameObject.AddComponent<Canvas>();
            cardRect.gameObject.AddComponent<GraphicRaycaster>();
        }
        canvas.overrideSorting = true;
        canvas.sortingOrder = parentTf.childCount; // Luôn ở trên cùng
    }

    private void RenderTopCard()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(
            TopCardImage.rectTransform
                .DOScaleX(0, 0.15f)
                .SetEase(Ease.InBack)
        );

        seq.AppendCallback(() =>
        {
            Sprite cardSprite = CardSO.GetSprite((CardColor)TopCard.CardColor, (CardType)TopCard.CardType, TopCard.Value);
            TopCardImage.sprite = cardSprite;
        });

        seq.Append(
            TopCardImage.rectTransform
                .DOScaleX(1, 0.15f)
                .SetEase(Ease.OutBack)
        );

        //Sprite cardSprite = CardSO.GetSprite((CardColor)TopCard.CardColor, (CardType)TopCard.CardType, TopCard.Value);
        //TopCardImage.sprite = cardSprite;
    }

    private void UpdateActiveCard()
    {
        foreach (var card in myCardList)
        {
            card.UpdateActiveState(TopCard);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateDrawCardButton()
    {
        Debug.Log($"IsPlayerTurn={ChessManager.Instance.IsPlayerTurn()} | " +
          $"IsReleasedCard={IsReleasedCard} | " +
          $"IsResponseWindowOpen={IsResponseWindowOpen}");
        if (ChessManager.Instance.IsPlayerTurn() == false)
        {
            DrawCardButton.interactable = false;
            return;
        }

        if( IsResponseWindowOpen )
        {
            DrawCardButton.interactable = false;
            return;
        }

        if (IsPlayerReleasedCard())
        {
            DrawCardButton.interactable = false;
            return;
        }

        if (myCardList.Count <= 0)
        {
            DrawCardButton.interactable = true;
            return;
        }

        foreach (var card in myCardList)
        {
            if (card.CanClick)
            {
                DrawCardButton.interactable = false;
                return;
            }
        }

        DrawCardButton.interactable = true;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_DrawCard(Team playerTeam, bool isOneTime = true)
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.PlaySFX(SoundType.Card_Draw);
        }

        if (NextCardID >= CardNumber)
            ShuffleCardDeckAgain();

        UnoCardData drawnCard = CurrentDeck.Get(NextCardID);

        NextCardID++;

        if (playerTeam == Team.White)
        {
            WhiteHand.Set(WhiteCardCount, drawnCard);
            WhiteCardCount++;
        }
        else
        {
            BlackHand.Set(BlackCardCount, drawnCard);
            BlackCardCount++;
        }

        Rpc_AddCardVisual(drawnCard, playerTeam);

        if (!isDrawingMultiple)
        {
            ChessManager.Instance.SwitchTurn();
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_AddCardVisual(UnoCardData cardData, Team playerTeam)
    {
        RectTransform targetTf =
            playerTeam == ChessManager.Instance.GetPlayerTeam()
            ? MyCardTf
            : OpponentCardTf;

        RenderAddCard(cardData,
            targetTf,
            playerTeam != ChessManager.Instance.GetPlayerTeam());

        RefreshHand(targetTf);
        UpdateActiveCard();
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_PreviewCard(UnoCardData cardData)
    {
        // Chỉ update visual, không thay đổi state
        Sprite cardSprite = CardSO.GetSprite((CardColor)cardData.CardColor, (CardType)cardData.CardType, cardData.Value);

        Sequence seq = DOTween.Sequence();
        seq.Append(TopCardImage.rectTransform.DOScaleX(0, 0.15f).SetEase(Ease.InBack));
        seq.AppendCallback(() => TopCardImage.sprite = cardSprite);
        seq.Append(TopCardImage.rectTransform.DOScaleX(1, 0.15f).SetEase(Ease.OutBack));
    }
    private void ShuffleCardDeckAgain()
    {
        for (int i = ReleaseCardCount - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            UnoCardData temp = ReleaseCardDeck.Get(i);
            ReleaseCardDeck.Set(i, ReleaseCardDeck.Get(j));
            ReleaseCardDeck.Set(j, temp);
        }

        for (int i = 0; i < ReleaseCardCount; i++)
        {
            CurrentDeck.Set(i, ReleaseCardDeck.Get(i));
        }

        CardNumber = ReleaseCardCount;
        ReleaseCardCount = 0;
        NextCardID = 0;
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_RemoveCard(UnoCardData cardData, Team playerTeam)
    {
        RemoveCard(cardData, playerTeam);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ReleaseMoveCard(UnoCardData cardData, Team playerTeam, int count)
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.PlaySFX(SoundType.Card_Play);
        }
        SetIsReleasedCard(true);
        ChessManager.Instance.SetTurnCount(count);
        SetTopCard(cardData);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ReleaseReverseCard(UnoCardData cardData, Team playerTeam)
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.PlaySFX(SoundType.Card_Play);
        }
        SetIsReleasedCard(true);
        ReverserCard();
        ChessManager.Instance.SwitchTeam();
        ChessManager.Instance.SwitchTurn();
        SetTopCard(cardData);
    }

    private void ReverserCard()
    {
        if (Runner.IsServer)
        {
            int maxCount = Mathf.Max(WhiteCardCount, BlackCardCount);

            for (int i = 0; i < maxCount; i++)
            {
                UnoCardData temp = WhiteHand[i];
                WhiteHand.Set(i, CurrentDeck[i]);
                CurrentDeck.Set(i, temp);
            }

            int tempCount = WhiteCardCount;
            WhiteCardCount = BlackCardCount;
            BlackCardCount = tempCount;
        }
    }

    public void ReleaseChangeColorCard(UnoCardData cardData, Team playerTeam)
    {

        // Chỉ mở UI cho người đã đánh card
        if (ChessManager.Instance.GetPlayerTeam() == playerTeam)
        {
            UIManager.Instance.OpenUI<ChooseColorUI>();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ChangeColorCard(CardColor newColor)
    {
        UnoCardData newTopCard = new UnoCardData
        {
            ID = TopCard.ID,
            CardType = (int)CardType.ChangeColor,
            CardColor = (int)newColor,
            Value = 0,
        };
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.PlaySFX(SoundType.Card_Play);
        }
        SetIsReleasedCard(true);
        SetTopCard(newTopCard);
        ChessManager.Instance.SwitchTurn();
    }

    public void ReleaseAddCard(UnoCardData cardData, Team playerTeam, int count)
    {
        SetIsReleasedCard(true);
        SetTopCard(cardData);

        RectTransform targetTf = playerTeam == ChessManager.Instance.GetPlayerTeam()
            ? MyCardTf : OpponentCardTf;

        isDrawingMultiple = true; 

        for (int i = 0; i < count; i++)
        {
            Rpc_DrawCard(playerTeam, false);
        }

        isDrawingMultiple = false; // tắt flag sau khi rút xong

        // Switch turn 1 lần duy nhất sau khi rút đủ bài
        ChessManager.Instance.SwitchTurn();

        DOVirtual.DelayedCall(0.2f, () => RefreshHand(targetTf));
    }

    public void RemoveCard(UnoCardData cardData, Team playerTeam, bool skipRefresh = false)
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.PlaySFX(SoundType.Card_Play);
        }

        if (Runner.IsServer)
        {
            ReleaseCardCount++;
            ReleaseCardDeck.Set(ReleaseCardCount - 1, cardData);

            if (playerTeam == Team.White)
            {
                for (int i = 0; i < WhiteCardCount; i++)
                {
                    if (WhiteHand.Get(i).ID == cardData.ID)
                    {
                        // Shift cards down
                        for (int j = i; j < WhiteCardCount - 1; j++)
                        {
                            WhiteHand.Set(j, WhiteHand.Get(j + 1));
                        }
                        WhiteHand.Set(WhiteCardCount - 1, new UnoCardData { ID = -1 }); // Mark last slot as empty
                        WhiteCardCount--;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < BlackCardCount; i++)
                {
                    if (BlackHand.Get(i).ID == cardData.ID)
                    {
                        // Shift cards down
                        for (int j = i; j < BlackCardCount - 1; j++)
                        {
                            BlackHand.Set(j, BlackHand.Get(j + 1));
                        }
                        BlackHand.Set(BlackCardCount - 1, new UnoCardData { ID = -1 }); // Mark last slot as empty
                        BlackCardCount--;
                        break;
                    }
                }
            }
        }

        //RectTransform targetHandTf = ((int)ChessManager.Instance.GetPlayerTeam() == ChessManager.Instance.currentTurn) ? MyCardTf : OpponentCardTf;

        //foreach (Transform child in targetHandTf)
        //{
        //    UnoCard cardUI = child.GetComponent<UnoCard>();
        //    if (cardUI.cardData.ID == cardData.ID)
        //    {
        //        cardUI.CanClick = false; // Disable interaction ngay lập tức
        //        RectTransform cardRect = cardUI.GetComponent<RectTransform>();
        //        cardRect.SetParent(TopCardImage.transform.parent);

        //        //Canvas canvas = cardRect.GetComponent<Canvas>();
        //        //if (canvas == null)
        //        //{
        //        //    canvas = cardRect.gameObject.AddComponent<Canvas>();
        //        //    cardRect.gameObject.AddComponent<GraphicRaycaster>();
        //        //}
        //        //canvas.overrideSorting = true;
        //        //canvas.sortingOrder = 100; // Luôn ở trên cùng

        //        cardRect.DOMove(TopCardImage.rectTransform.position, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
        //        {
        //            cardUI.DestroyCard();
        //        });
        //        break;
        //    }
        //}

        //if (!skipRefresh)
        //{
        //    RefreshHand(targetHandTf);
        //}

        if (ChessManager.Instance.GetPlayerTeam() == playerTeam)
        {
            foreach (Transform child in MyCardTf)
            {
                UnoCard cardUI = child.GetComponent<UnoCard>();
                if (cardUI.cardData.ID == cardData.ID)
                {
                    cardUI.CanClick = false; // Disable interaction ngay lập tức
                    RectTransform cardRect = cardUI.GetComponent<RectTransform>();
                    cardRect.SetParent(TopCardImage.transform.parent);

                    cardRect.DOMove(TopCardImage.rectTransform.position, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        cardUI.DestroyCard();
                    });

                    if (!skipRefresh)
                        DOVirtual.DelayedCall(0.2f, () => RefreshHand(MyCardTf)); // ← delay 0.2f
                    break;
                }
            }
        }
        else
        {
            // Opponent cards là face-down, không match được ID
            // Chỉ cần destroy 1 card bất kỳ để giảm số lượng hiển thị
            if (OpponentCardTf.childCount > 0)
            {
                UnoCard cardUI = OpponentCardTf.GetChild(0).GetComponent<UnoCard>();
                cardUI.CanClick = false; // Disable interaction ngay lập tức
                RectTransform cardRect = cardUI.GetComponent<RectTransform>();
                cardRect.SetParent(TopCardImage.transform.parent);

                cardRect.DOMove(TopCardImage.rectTransform.position, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    cardUI.DestroyCard();
                });
                if (!skipRefresh)
                    DOVirtual.DelayedCall(0.2f, () => RefreshHand(OpponentCardTf));
            }
        }

        if (playerTeam == ChessManager.Instance.GetPlayerTeam())
        {
            myCardList.RemoveAll(card => card.cardData.ID == cardData.ID);
        }
    }

    //response window methods   
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_OpenResponseWindow(UnoCardData cardData, Team playerTeam)
    {
        if (Runner.IsServer)
        {
            ResponseWindowStartTime = Runner.SimulationTime;
            PendingCard = cardData;
            PendingCardTeam = playerTeam;
            ChessManager.Instance.TurnTimeElapsedBeforeWindow =
                Runner.SimulationTime - ChessManager.Instance.turnStartTime;
        }

        // Set trực tiếp trên tất cả clients, không đợi sync
        IsResponseWindowOpen = true;
        PendingCard = cardData;
        PendingCardTeam = playerTeam;


        DOVirtual.DelayedCall(0.4f, () =>
        {
            UpdateActiveCard();
            Rpc_UpdateDrawCardButton();
        });
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_BlockCard(Team blockerTeam, UnoCardData blockCardData)
    {
        if (Runner.IsServer)
        {
            ChessManager.Instance.turnStartTime =
                Runner.SimulationTime - ChessManager.Instance.TurnTimeElapsedBeforeWindow;
        }
        SetIsReleasedCard(true);
        IsResponseWindowOpen = false;
        SetTopCardVisualOnly(blockCardData); // chỉ render, chưa update active
        ChessManager.Instance.SwitchTurn(); // currentTurn đổi trước

        // Delay nhỏ để đảm bảo SwitchTurn sync xong mới evaluate
        DOVirtual.DelayedCall(0.4f, () =>
        {
            UpdateActiveCard();
            Rpc_UpdateDrawCardButton();
        });
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ResolveCard()
    {
        if (Runner.IsServer)
        {
            IsResponseWindowOpen = false;

            // Resume turn timer from where it paused
            ChessManager.Instance.turnStartTime =
                Runner.SimulationTime - ChessManager.Instance.TurnTimeElapsedBeforeWindow;
        }
        IsResponseWindowOpen = false;
        // Dispatch to the original card's effect
        switch ((CardType)PendingCard.CardType)
        {
            case CardType.Move:
                Rpc_ReleaseMoveCard(PendingCard, PendingCardTeam, PendingCard.Value);
                break;
            case CardType.Reverse:
                Rpc_ReleaseReverseCard(PendingCard, PendingCardTeam);
                break;
            case CardType.ChangeColor:
                ReleaseChangeColorCard(PendingCard, PendingCardTeam);
                break;
            case CardType.Add:
                ReleaseAddCard(PendingCard, PendingCardTeam, PendingCard.Value);
                break;
            default:
                Debug.LogWarning("Unhandled card type in ResolveCard");
                break;
        }
    }

    public float GetResponseWindowTimeRemaining()
    {
        if (!IsResponseWindowOpen) return 0f;
        return Mathf.Max(0f, ResponseWindowDuration - (Runner.SimulationTime - ResponseWindowStartTime));
    }

    private void SetTopCardVisualOnly(UnoCardData cardData)
    {
        TopCard = cardData; // bỏ if (Runner.IsServer), cả hai phía đều set
        RenderTopCard();
    }
}