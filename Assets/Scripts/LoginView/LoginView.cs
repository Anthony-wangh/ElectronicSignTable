using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ShowLoginViewEventArgs : EventArgs {

    public bool Active;
}
public class LoginEventArgs : EventArgs
{
    public string IPText;
}

/// <summary>
/// 登录界面
/// </summary>
public class LoginView : MonoBehaviour
{
    [SerializeField]
    private InputField IPInput;

    [SerializeField]
    private GameObject Bg;

    private int _connectState = -1;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener("ShowLoginView", ShowLoginView);
    }



    private void Update()
    {
        if (_connectState == NewUDPClient.instance.ConnectState)
            return;
        _connectState = NewUDPClient.instance.ConnectState;
        SetPanelActive(_connectState!=1);
    }


    private void ShowLoginView(object sender, EventArgs e)
    {
        ShowLoginViewEventArgs args = (ShowLoginViewEventArgs)e;
        if (args != null)
        {
            SetPanelActive(args.Active);
        }
    }

    
    public void Login() {

        if (string.IsNullOrEmpty(IPInput.text)|| !IsIPv4(IPInput.text))
        {
            TipView.Inst.Tip("请输入有效的IP地址！！");
            return;
        }

        this.TriggerEvent("LoginEvent", new LoginEventArgs() { IPText= IPInput.text });
    }


    private bool IsIPv4(string ip)
    {
        string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        return Regex.IsMatch(ip, pattern);
    }


    public void Close() {

        SetPanelActive(false);
    }

    private void SetPanelActive(bool active) {     
        Bg?.SetActive(active);
    }
}
