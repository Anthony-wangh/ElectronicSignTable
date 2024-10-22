using System;
using UnityEngine;
using UnityEngine.UI;

public class TipViewEventArgs : EventArgs
{
    public string Context;
}

/// <summary>
/// »ıÃ· æ
/// </summary>
public class TipView : MonoBehaviour
{
    public const string TipViewShowEvent = "TipViewShowEvent";

    [SerializeField]
    private Text Text;
    [SerializeField]
    private RectTransform BgRect;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener(TipViewShowEvent, OnTip);
    }

    private void OnTip(object sender, EventArgs e)
    {
        TipViewEventArgs args = (TipViewEventArgs)e;
        if (args != null) {
            BgRect.gameObject.SetActive(true);
            BgRect.localScale = new Vector3(1, 0, 1);
            Text.text = args.Context;
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
