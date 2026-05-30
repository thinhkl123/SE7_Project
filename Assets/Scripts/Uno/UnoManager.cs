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
    public TextMeshProUGUI CardNumberLeft;
    public Button DrawCardButton;

    [Header("Runtime")]
    [Networked] public int CardNumber { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> CurrentDeck => default;
    [Networked] public int WhiteCardCount { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> WhiteHand => default;
    [Networked] public int BlackCardCount { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> BlackHand => default;
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
        UpdateCardNumberLeft();
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

    public void UpdateCardNumberLeft()
    {
        int cardsLeft = CardNumber - NextCardID;
        CardNumberLeft.text = cardsLeft.ToString();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_DrawCard(Team playerTeam)
    {
        if (NextCardID > CardNumber - 1)
        {
            Debug.LogWarning("No more cards to draw!");
            return;
        }

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
        UpdateCardNumberLeft();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_SetTurnCount(UnoCardData cardData, Team playerTeam, int count)
    {
        RemoveCard(cardData.ID, playerTeam);
        SetIsReleasedCard(true);
        ChessManager.Instance.SetTurnCount(count);
        SetTopCard(cardData);
    }

    public void ReverserCard()
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

    private void RemoveCard(int cardID, Team playerTeam)
    {
        if (Runner.IsServer)
        {
            if (playerTeam == Team.White)
            {
                for (int i = 0; i < WhiteCardCount; i++)
                {
                    if (WhiteHand.Get(i).ID == cardID)
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
                    if (BlackHand.Get(i).ID == cardID)
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
                if (cardUI.cardData.ID == cardID)
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
                if (cardUI.cardData.ID == cardID)
                {
                    cardUI.DestroyCard();
                    break;
                }
            }
        }
    }
}
