using DG.Tweening;
using Fusion;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnoCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UnoCardData cardData;
    public Image CardSprite;
    public Sprite BackSprite;

    public bool CanClick { get; private set; } = true;

    private bool isOpponentCard;
    private Vector3 startScale;
    private int childIndex;

    private void Awake()
    {
        startScale = transform.localScale;
    }

    public void SetCardData(UnoCardData data, bool isOpponent)
    {
        cardData = data;
        UpdateCardVisual(isOpponent);
    }

    private void UpdateCardVisual(bool isOpponet)
    {
        isOpponentCard = isOpponet;
        if (isOpponet)
        {
            CardSprite.sprite = BackSprite;
            return;
        }
        CardSprite.sprite = UnoManager.Instance.CardSO.GetSprite((CardColor)cardData.CardColor, (CardType)cardData.CardType, cardData.Value);
    }

    //public void ExecuteCard()
    //{
    //    if (ChessManager.Instance.IsPlayerTurn() == false)
    //    {
    //        Debug.LogWarning("It's not the player's turn!");
    //        return;
    //    }

    //    if (UnoManager.Instance.IsPlayerReleasedCard())
    //    {
    //        Debug.LogWarning("Player has already released a card this turn!");
    //        return;
    //    }

    //    Team playerTeam = ChessManager.Instance.GetPlayerTeam();

    //    switch ((CardType)cardData.CardType)
    //    {
    //        case CardType.Move:
    //            ReleaseMoveCard(playerTeam, cardData.Value);

    //            break;
    //        case CardType.Reverse:
    //            ReverseAllChessPiece(playerTeam);

    //            break;
    //        case CardType.ChangeColor:
    //            ReleaseChangeColorCard(playerTeam);

    //            break;
    //        case CardType.Block:


    //            break;
    //        case CardType.Add:
    //            ReleaseAddCard(playerTeam, cardData.Value);

    //            break;
    //        default:
    //            Debug.LogWarning("Unknown card type!");
    //            break;
    //    }
    //}
    public void ExecuteCard()
    {
        Team playerTeam = ChessManager.Instance.GetPlayerTeam();

        if ((CardType)cardData.CardType == CardType.Block)
        {
            if (!UnoManager.Instance.IsResponseWindowOpen) return;
            if (ChessManager.Instance.IsPlayerTurn()) return;
            UnoManager.Instance.Rpc_RemoveCard(cardData, playerTeam);
            UnoManager.Instance.Rpc_BlockCard(playerTeam, cardData);
            return;
        }

        // Guard giống version cũ
        if (!ChessManager.Instance.IsPlayerTurn()) return;
        if (UnoManager.Instance.IsPlayerReleasedCard()) return;

        UnoManager.Instance.Rpc_RemoveCard(cardData, playerTeam);
        UnoManager.Instance.Rpc_PreviewCard(cardData);
        UnoManager.Instance.Rpc_OpenResponseWindow(cardData, playerTeam);
    }
    //public void ReleaseMoveCard(Team playerTeam, int count)
    //{
    //    UnoManager.Instance.Rpc_ReleaseMoveCard(cardData, playerTeam, count);
    //}

    //public void ReverseAllChessPiece(Team playerTeam)
    //{
    //    UnoManager.Instance.Rpc_ReleaseReverseCard(cardData, playerTeam);
    //}

    //public void ReleaseChangeColorCard(Team playerTeam)
    //{
    //    UnoManager.Instance.ReleaseChangeColorCard(cardData, playerTeam);
    //}

    //public void ReleaseAddCard(Team playerTeam, int cardCount)
    //{
    //    UnoManager.Instance.ReleaseAddCard(cardData, playerTeam, cardCount);
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanClick)
            return;

        ExecuteCard();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanClick)
            return;

        if (isOpponentCard)
            return;

        childIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        transform.DOScale(startScale * 1.1f, 0.15f);

        transform.DOLocalMoveY(
            50f,
            0.15f
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CanClick)
            return;

        if (isOpponentCard)
            return;

        transform.SetSiblingIndex(childIndex);
        transform.DOScale(startScale, 0.15f);

        transform.DOLocalMoveY(
            0f,
            0.15f
        );
    }

    public void DestroyCard()
    {
        Destroy(gameObject);
        // Implement logic to remove the card from the player's hand and update the game state
    }

    //internal void UpdateActiveState(UnoCardData topCard)
    //{
    //    if (IsPlayable(topCard))
    //    {
    //        CanClick = true;
    //    }
    //    else
    //    {
    //        CanClick = false;
    //    }

    //    Color tempColor = CardSprite.color;
    //    tempColor.a = CanClick ? 1f : 0.5f;
    //    CardSprite.color = tempColor;
    //}
    internal void UpdateActiveState(UnoCardData topCard)
    {
        bool windowOpen = UnoManager.Instance.IsResponseWindowOpen;
        Team myTeam = ChessManager.Instance.GetPlayerTeam();
        bool iAmResponder = windowOpen && (myTeam != UnoManager.Instance.PendingCardTeam);

        if ((CardType)cardData.CardType == CardType.Block)
        {
            CanClick = iAmResponder; // chỉ block card mới cần check responder
        }
        else
        {
            // Giống version cũ: active nếu playable, không cần check turn
            // ExecuteCard() đã có guard IsPlayerTurn() rồi
            CanClick = !windowOpen && IsPlayable(topCard);
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