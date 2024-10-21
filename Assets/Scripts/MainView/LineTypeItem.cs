using UnityEngine;
using UnityEngine.UI;

public class LineTypeItem : MonoBehaviour
{
    [SerializeField]
    private Texture2D Icon;
    // Start is called before the first frame update
    void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((isOn) => {

            PaintBrush.Inst.brushTypeTexture = Icon;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
