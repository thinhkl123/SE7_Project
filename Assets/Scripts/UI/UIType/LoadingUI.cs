using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : UICanvas
{
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Image bg;

    public void ShowLoading(string message = "Loading ...", float alpha = 1)
    {
        loadingText.text = message;

        Color tempColor = bg.color;
        tempColor.a = alpha; 
        bg.color = tempColor;
    }
}
