using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuideMessage : MonoBehaviour
{
    public RectTransform panelTextTf;
    public TextMeshProUGUI tutorialText;
    public Image fill;

    private float time;
    private bool isShowing = false;
    private float currentTime = 0f;

    public void ShowTutorialText(string text, float time)
    {
        //panelTextTf.gameObject.SetActive(true);
        SetTutorialText(text);
        this.time = time;
        Invoke(nameof(HideTutorialText), time);
    }

    private void Update()
    {
        if (isShowing)
        {
            currentTime += Time.deltaTime;
            fill.fillAmount = 1 - currentTime / time;
        }
    }

    private void HideTutorialText()
    {
        //panelTextTf.gameObject.SetActive(false);
        panelTextTf.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(this.gameObject);
        });
    }

    private void SetTutorialText(string text)
    {
        tutorialText.text = text;
        ResizeImageToFitText();

        panelTextTf.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

        isShowing = true;
    }

    private void ResizeImageToFitText()
    {
        // Force text to update its layout
        tutorialText.ForceMeshUpdate();

        // Get preferred width and height of the text
        Vector2 textSize = new Vector2(
            tutorialText.preferredWidth,
            tutorialText.preferredHeight
        );

        // Apply padding and set size
        panelTextTf.sizeDelta = textSize + new Vector2(200f, 100f);
    }
}
