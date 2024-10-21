using UnityEngine;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{
    [SerializeField]
    private Button SendBtn;

    [SerializeField]
    private Toggle ScreenSwitch;
    // Start is called before the first frame update
    void Start()
    {
        ScreenSwitch.onValueChanged.AddListener((isOn) => {

            PaintBrush.Inst.OnSwitchScreen(isOn);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (SendBtn!=null)
        {
            SendBtn.interactable = NewUDPClient.instance.IsConnected;
        }       

    }
}
