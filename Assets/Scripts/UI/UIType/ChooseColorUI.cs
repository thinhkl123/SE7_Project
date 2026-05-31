using UnityEngine;
using UnityEngine.UI;

public class ChooseColorUI : UICanvas
{
    public Transform PanelTf;

    private void OnEnable()
    {
        ShowPanel();
    }

    public void ShowPanel()
    {
        for (int i = 0; i < PanelTf.childCount; i++)
        {
            PanelTf.GetChild(i).gameObject.SetActive(false);
            PanelTf.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        for (int i = 1; i < (int)CardColor.Black; i++)
        {
            int colorIndex = i;
            PanelTf.GetChild(i).gameObject.SetActive(true);
            switch ((CardColor)colorIndex)
            {
                case CardColor.Red:
                    PanelTf.GetChild(i).GetComponent<Image>().color = Color.red;
                    break;
                case CardColor.Green:
                    PanelTf.GetChild(i).GetComponent<Image>().color = Color.green;
                    break;
                case CardColor.Blue:
                    PanelTf.GetChild(i).GetComponent<Image>().color = Color.blue;
                    break;
                case CardColor.Yellow:
                    PanelTf.GetChild(i).GetComponent<Image>().color = Color.yellow;
                    break;
            }
            PanelTf.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
            {
                UnoManager.Instance.Rpc_ChangeColorCard((CardColor)colorIndex);
                UIManager.Instance.CloseUI<ChooseColorUI>();
            });
        }
    }
}
