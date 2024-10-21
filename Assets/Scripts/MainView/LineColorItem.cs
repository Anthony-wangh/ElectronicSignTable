using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��ˢ��ɫѡ��
/// </summary>
public class LineColorItem : MonoBehaviour
{
    [SerializeField]
    private Color color;
    // Start is called before the first frame update
    void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((isOn) => {

            PaintBrush.Inst.brushColor=color;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
