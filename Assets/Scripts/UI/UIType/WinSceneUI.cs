using UnityEngine.UI;

public class WinSceneUI : UICanvas
{
    public Button BackButton;
    private void Sleep()
    {
        gameObject.SetActive(false);
    }
}
