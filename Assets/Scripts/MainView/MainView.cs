using UnityEngine;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{
    [SerializeField]
    private Button SendBtn;

    [SerializeField]
    private Toggle ScreenSwitch;

    private int _connectState = -1;
    // Start is called before the first frame update
    void Start()
    {
        ScreenSwitch.onValueChanged.AddListener((isOn) => {

            PaintBrush.Inst.OnSwitchScreen(isOn);
        });
        SendBtn.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_connectState == NewUDPClient.instance.ConnectState)
            return;
        _connectState = NewUDPClient.instance.ConnectState;
        TipView.Inst.Tip(_connectState == 1 ? "连接成功！！" : "连接异常，请检查网络并重试！！");
        SendBtn.interactable = NewUDPClient.instance.ConnectState == 1;
    }
}
