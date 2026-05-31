using Fusion;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnoCard : MonoBehaviour, IPointerClickHandler
{
    public UnoCardData cardData;
    public Image CardSprite;
    public Sprite BackSprite;

    public bool CanClick { get; private set; } = true;

    public void SetCardData(UnoCardData data, bool isOpponent)
    {
        cardData = data;
        UpdateCardVisual(isOpponent);
    }

    private void UpdateCardVisual(bool isOpponet)
    {
        if (isOpponet)
        {
            CardSprite.sprite = BackSprite;
            return;
        }
        CardSprite.sprite = UnoManager.Instance.CardSO.GetSprite((CardColor)cardData.CardColor, (CardType)cardData.CardType, cardData.Value);
    }

    public void ExecuteCard()
    {
        if (ChessManager.Instance.IsPlayerTurn() == false)
        {
            Debug.LogWarning("It's not the player's turn!");
            return;
        }

        if (UnoManager.Instance.IsPlayerReleasedCard())
        {
            Debug.LogWarning("Player has already released a card this turn!");
            return;
        }

        Team playerTeam = ChessManager.Instance.GetPlayerTeam();

        switch ((CardType)cardData.CardType)
        {
            case CardType.Move:
                ReleaseMoveCard(playerTeam, cardData.Value);

                break;
            case CardType.Reverse:
                ReverseAllChessPiece(playerTeam);

                break;
            case CardType.ChangeColor:
                ReleaseChangeColorCard(playerTeam);

                break;
            case CardType.Block:
                

                break;
            case CardType.Add:
                ReleaseAddCard(playerTeam, cardData.Value);

                break;
            default:
                Debug.LogWarning("Unknown card type!");
                break;
        }
    }

    private void ReleaseMoveCard(Team playerTeam, int count)
    {
        UnoManager.Instance.Rpc_ReleaseMoveCard(cardData, playerTeam, count);
    }

    private void ReverseAllChessPiece(Team playerTeam)
    {
        UnoManager.Instance.Rpc_ReleaseReverseCard(cardData, playerTeam);
    }

    public void ReleaseChangeColorCard(Team playerTeam)
    {
        UnoManager.Instance.ReleaseChangeColorCard(cardData, playerTeam);
    }

    public void ReleaseAddCard(Team playerTeam, int cardCount)
    {
        UnoManager.Instance.ReleaseAddCard(cardData, playerTeam, cardCount);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanClick)
            return;

        ExecuteCard();
    }

    public void DestroyCard()
    {
        Destroy(gameObject);
        // Implement logic to remove the card from the player's hand and update the game state
    }

    internal void UpdateActiveState(UnoCardData topCard)
    {
        if (IsPlayable(topCard))
        {
            CanClick = true;
        }
        else
        {
            CanClick = false;
        }

        Color tempColor = CardSprite.color;
        tempColor.a = CanClick ? 1f : 0.5f;
        CardSprite.color = tempColor;
    }

    private bool IsPlayable(UnoCardData topCard)
    {
        if (cardData.CardColor == (int)CardColor.Black || topCard.CardColor == (int)CardColor.Black)
        {
            return true;
        }

        if (cardData.CardColor == topCard.CardColor || cardData.Value == topCard.Value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public struct UnoCardData : INetworkStruct
{
    public int ID;
    public int CardColor;
    public int CardType;
    public int Value;
}
