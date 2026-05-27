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
        CardSprite.sprite = UnoDeckManager.Instance.CardSO.GetSprite((CardColor)cardData.CardColor, (CardType)cardData.CardType, cardData.Value);
    }

    public void ExecuteCard()
    {
        Team playerTeam = ChessManager.Instance.GetPlayerTeam();

        switch ((CardType)cardData.CardType)
        {
            case CardType.Move:
                SetPlayerTurnCount(playerTeam, cardData.Value);

                break;
            case CardType.Reverse:
                ReverseAllChessPiece();

                break;
            case CardType.ChangeColor:
                ChangeColor();

                break;
            case CardType.Block:
                

                break;
            case CardType.Add:
                AddCardForPlayer(playerTeam, cardData.Value);

                break;
            default:
                Debug.LogWarning("Unknown card type!");
                break;
        }
    }

    private void SetPlayerTurnCount(Team playerTeam, int count)
    {
        UnoDeckManager.Instance.Rpc_SetTurnCount(cardData.ID, playerTeam, count);
    }

    private void ReverseAllChessPiece()
    {
        // Implement logic to reverse all chess pieces on the board
    }

    public void ChangeColor()
    {
        // Implement logic to change the current color in play
    }

    public void AddCardForPlayer(Team playerTeam, int cardCount)
    {
        // Implement logic to add cards to the player's hand
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ExecuteCard();
    }

    public void DestroyCard()
    {
        Destroy(gameObject);
        // Implement logic to remove the card from the player's hand and update the game state
    }
}

public struct UnoCardData : INetworkStruct
{
    public int ID;
    public int CardColor;
    public int CardType;
    public int Value;
}
