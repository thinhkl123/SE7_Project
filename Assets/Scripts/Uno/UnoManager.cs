using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public int InitialHandSize = 7;

    [Header("Player Card List")]
    public UnoCard CardPrefab;
    public RectTransform MyCardTf;
    private List<UnoCard> myCardList = new List<UnoCard>();
    public RectTransform OpponentCardTf;

    [Header("Card Deck")]
    public Image TopCardImage;
    public Button DrawCardButton;

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

    private void Start()
    {
        DrawCardButton.onClick.AddListener(() =>
        {
            Rpc_DrawCard(ChessManager.Instance.GetPlayerTeam());
        });
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

            Rpc_RenderCard();
        }
    }

    private void SetTopCard(UnoCardData cardData)
    {
        if (Runner.IsServer)
            TopCard = cardData;

        RenderTopCard();
        UpdateActiveCard();
        UpdateDrawCardButton();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_RenderCard()
    {
        myCardList.Clear();

        RenderCardList();

        RenderTopCard();
        UpdateActiveCard();
        UpdateDrawCardButton();
    }

    private void RenderCardList()
    {
        if (ChessManager.Instance.GetPlayerTeam() == Team.White)
        {
            RenderHand(WhiteHand, WhiteCardCount, MyCardTf, false);
            RenderHand(BlackHand, BlackCardCount, OpponentCardTf, true);
        }
        else
        {
            RenderHand(BlackHand, BlackCardCount, MyCardTf, false);
            RenderHand(WhiteHand, WhiteCardCount, OpponentCardTf, true);
        }
    }

    private void RenderHand(NetworkArray<UnoCardData> hand, int length, RectTransform parentTf, bool isOpponent)
    {
        // Clear existing cards
        foreach (Transform child in parentTf)
        {
            Destroy(child.gameObject);
        }
        // Instantiate new card UI elements
        for (int i = 0; i < length; i++)
        {
            UnoCardData cardData = hand.Get(i);
            if (cardData.ID != -1) // Assuming -1 means empty slot
            {
                UnoCard cardUI = Instantiate(CardPrefab, parentTf);
                cardUI.SetCardData(cardData, isOpponent);

                if (!isOpponent)
                {
                    myCardList.Add(cardUI);
                }
            }
        }
    }

    private void RenderTopCard()
    {
        Sprite cardSprite = CardSO.GetSprite((CardColor)TopCard.CardColor, (CardType)TopCard.CardType, TopCard.Value);
        TopCardImage.sprite = cardSprite;
    }

    private void UpdateActiveCard()
    {
        foreach (var card in myCardList)
        {
            card.UpdateActiveState(TopCard);
        }
    }

    public void UpdateDrawCardButton()
    {
        if (ChessManager.Instance.IsPlayerTurn() == false)
        {
            DrawCardButton.interactable = false;
            return;
        }

        if (IsPlayerReleasedCard())
        {
            DrawCardButton.interactable = false;
            return;
        }

        if (NextCardID > CardNumber - 1)
        {
            DrawCardButton.interactable = false;
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_DrawCard(Team playerTeam, bool isOneTime = true)
    {
        if (NextCardID > CardNumber - 1)
        {
            ShuffleCardDeckAgain();
        }
        
        if (isOneTime)
            DrawCardButton.interactable = false;

        UnoCardData drawnCard = CurrentDeck.Get(NextCardID - 1);

        if (Runner.IsServer)
        {
            NextCardID++;
        }

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
        RenderCardList();
        UpdateActiveCard();
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
    public void Rpc_ReleaseMoveCard(UnoCardData cardData, Team playerTeam, int count)
    {
        RemoveCard(cardData, playerTeam);
        SetIsReleasedCard(true);
        ChessManager.Instance.SetTurnCount(count);
        SetTopCard(cardData);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ReleaseReverseCard(UnoCardData cardData, Team playerTeam)
    {
        RemoveCard(cardData, playerTeam);
        SetIsReleasedCard(true);
        ReverserCard();
        ChessManager.Instance.SwitchTeam();
        ChessManager.Instance.SwitchTurn();
        SetTopCard(TopCard);
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
        RemoveCard(cardData, playerTeam);
        UIManager.Instance.OpenUI<ChooseColorUI>();
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
        SetIsReleasedCard(true);
        SetTopCard(newTopCard);
        ChessManager.Instance.SwitchTurn();
    }

    private void RemoveCard(UnoCardData cardData, Team playerTeam)
    {
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

        if (ChessManager.Instance.GetPlayerTeam() == playerTeam)
        {
            foreach (Transform child in MyCardTf)
            {
                UnoCard cardUI = child.GetComponent<UnoCard>();
                if (cardUI.cardData.ID == cardData.ID)
                {
                    cardUI.DestroyCard();
                    break;
                }
            }
        }
        else
        {
            foreach (Transform child in OpponentCardTf)
            {
                UnoCard cardUI = child.GetComponent<UnoCard>();
                if (cardUI.cardData.ID == cardData.ID)
                {
                    cardUI.DestroyCard();
                    break;
                }
            }
        }

        if (playerTeam == ChessManager.Instance.GetPlayerTeam())
        {
            myCardList.RemoveAll(card => card.cardData.ID == cardData.ID);
        }
    }
}
