using Fusion;
using UnityEngine;

public class UnoDeckManager : NetworkBehaviour
{
    public static UnoDeckManager Instance { get; private set; }

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

    [Header("UI")]
    public UnoCard CardPrefab;
    public RectTransform MyCardTf;
    public RectTransform OpponentCardTf;

    [Header("Runtime")]
    [Networked] public int CardNumber { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> CurrentDeck => default;
    [Networked] public int WhiteCardCount { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> WhiteHand => default;
    [Networked] public int BlackCardCount { get; set; }
    [Networked, Capacity(108)] public NetworkArray<UnoCardData> BlackHand => default;
    [Networked] public UnoCardData TopCard { get; set; }
    [Networked] public int NextCardID { get; set; }

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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_RenderCard()
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
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_SetTurnCount(int cardID, Team playerTeam, int count)
    {
        RemoveCard(cardID, playerTeam);
        ChessManager.Instance.SetIsReleasedCard(true);
        ChessManager.Instance.SetTurnCount(count);
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
                    Destroy(child.gameObject);
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
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
    }
}
