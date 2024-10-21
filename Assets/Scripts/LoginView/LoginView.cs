using System;
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
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener("ShowLoginView", ShowLoginView);
    }

    private void ShowLoginView(object sender, EventArgs e)
    {
        ShowLoginViewEventArgs args = (ShowLoginViewEventArgs)e;
        if (args != null)
        {
            SetPanelActive(args.Active);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Login() {

        if (string.IsNullOrEmpty(IPInput.text))
        {
            Debug.Log("请输入有效的IP地址！！");
            return;
        }

        this.TriggerEvent("LoginEvent", new LoginEventArgs() { IPText= IPInput.text });
        Close();
    }


    public void Close() {

        SetPanelActive(false);
    }

    private void SetPanelActive(bool active) {     
        Bg?.SetActive(active);
    }
}
