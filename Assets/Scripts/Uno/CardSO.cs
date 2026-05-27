using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData", order = 1)]
public class CardSO : ScriptableObject
{
    public CardData[] cards;

    public Sprite GetSprite(CardColor color, CardType type, int value)
    {
        foreach (var card in cards)
        {
            if (card.color == color && card.type == type && card.value == value)
            {
                return card.sprite;
            }
        }

        Debug.LogWarning($"Card not found: Color={color}, Type={type}, Value={value}");
        return null; // Return null if no matching card is found
    }
}

[Serializable]
public class CardData
{
    public CardColor color;
    public CardType type;
    public Sprite sprite;
    public int value = 0; // For number cards, this will hold the number (0-9). For action cards, it can be set to a default value or ignored.
    public int amount;
    [TextArea] public string description;
}
