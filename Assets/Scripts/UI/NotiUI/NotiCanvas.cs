using CustomUtils;
using UnityEngine;

public class NotiCanvas : SingletonMono<NotiCanvas>
{
    public Transform messageListTf;
    public GuideMessage messagePrefab;

    public void ShowTutorialText(string text, float time)
    {
        GuideMessage gmOb = Instantiate(messagePrefab, messageListTf);
        gmOb.ShowTutorialText(text, time);
    }
}
