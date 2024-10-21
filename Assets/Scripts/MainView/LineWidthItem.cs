using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ª≠± œﬂøÌ—°œÓ
/// </summary>
public class LineWidthItem : MonoBehaviour
{
    public float WidthValue = 1;
    // Start is called before the first frame update
    void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((isOn) => {

            PaintBrush.Inst.LineWidth = WidthValue;
        });
    }

}
