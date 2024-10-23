using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class TipViewEventArgs : EventArgs
{
    public string Context;
}

/// <summary>
/// »ıÃ· æ
/// </summary>
public class TipView : MonoBehaviour
{
    public static TipView Inst;

    [SerializeField]
    private Text Text;
    [SerializeField]
    private RectTransform BgRect;


    private Tweener _tweener;
    private Coroutine _coroutine;

    // Start is called before the first frame update
    void Awake()
    {
        Inst = this;
    }

    public void Tip(string context) {

        if (string.IsNullOrEmpty(context))
            return;

        if (_tweener != null)
            _tweener.Kill();

        if(_coroutine!=null)
            StopCoroutine(_coroutine);
        BgRect.localScale = new Vector3(1, 0, 1);
        BgRect.gameObject.SetActive(true);        
        Text.text = context;
        _tweener=BgRect.DOScaleY(1, 0.5f).SetEase(Ease.InOutBack);
        _coroutine=StartCoroutine(CloseDelay());
    }


    private IEnumerator CloseDelay() {

        yield return new WaitForSeconds(3);

        BgRect.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
