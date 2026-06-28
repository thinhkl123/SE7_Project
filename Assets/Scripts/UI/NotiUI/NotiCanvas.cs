using CustomUtils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotiCanvas : SingletonMono<NotiCanvas>
{
    public event EventHandler OnYes;
    public event EventHandler OnNo;

    [Header("Message")]
    public Transform messageListTf;
    public GuideMessage messagePrefab;

    [Header("Popup")]
    public Transform PopupTf;
    public TextMeshProUGUI PopupText;
    public Button YesBtn;
    public Button NoBtn;

    private void Start()
    {
        YesBtn.onClick.AddListener(() =>
        {
            PopupTf.gameObject.SetActive(false);
            UIManager.Instance.OpenUI<HomeUI>();
            OnYes?.Invoke(this, EventArgs.Empty);
        });

        NoBtn.onClick.AddListener(() =>
        {
            PopupTf.gameObject.SetActive(false);
            OnNo?.Invoke(this, EventArgs.Empty);
        });
    }

    public void ShowTutorialText(string text, float time)
    {
        GuideMessage gmOb = Instantiate(messagePrefab, messageListTf);
        gmOb.ShowTutorialText(text, time);
    }

    public void ShowPopup(string text, bool isShowYes = true, bool isShowNo = true)
    {
        PopupTf.gameObject.SetActive(true);
        PopupText.text = text;

        if (isShowYes)
        {
            YesBtn.gameObject.SetActive(true);
        }
        else
        {
            YesBtn.gameObject.SetActive(false);
        }

        if (isShowNo)
        {
            NoBtn.gameObject.SetActive(true);
        }
        else
        {
            NoBtn.gameObject.SetActive(false);
        }
    }

    public void ClosePopup()
    {
        PopupTf.gameObject.SetActive(false);
    }
}
