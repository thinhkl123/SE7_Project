using UnityEngine.UI;

public class LoseSceneUI : UICanvas
{
    public Button BackButton;
    private void Sleep()
    {
        gameObject.SetActive(false);
    }
}
