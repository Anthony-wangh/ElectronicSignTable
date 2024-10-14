using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


//画笔
public class Paint : MonoBehaviour
{
    private GameObject _clone;
    private LineRenderer _line;
    private int _number;

    //带有LineRender物体
    [FormerlySerializedAs("Target")] [Header("LineRender预制体")]
    public GameObject target;

    [FormerlySerializedAs("PaintCamera")] public Camera paintCamera;

    //画笔残留
    private List<GameObject> _lines=new List<GameObject>();
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //实例化对象
            _clone = Instantiate(target, target.transform.position, Quaternion.identity);
            _lines.Add(_clone);
            //获得该物体上的LineRender组件
            _line = _clone.GetComponent<LineRenderer>();
            //设置起始和结束的颜色
            //_line.startColor = Color.white;
            //_line.endColor = Color.white;
            //设置起始和结束的宽度
            _line.startWidth = 0.2f;
            _line.endWidth = 0.1f;
            //计数
            _number = 0;
        }

        if (Input.GetMouseButton(0))
        {
            //每一帧检测，按下鼠标的时间越长，计数越多
            _number++;
            //设置顶点数
            _line.positionCount = _number;
            var screen = Input.mousePosition;
            var pos = paintCamera.ScreenToWorldPoint(screen);
            pos.z = 5;
            //设置顶点位置(顶点的索引，将鼠标点击的屏幕坐标转换为世界坐标)
            _line.SetPosition(_number - 1, pos);
        }

        //清除绘画
        if (Input.GetMouseButtonDown(1))
        {
            Release();
        }
    }


    private void Release()
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            Destroy(_lines[i]);
        }
        _lines.Clear();
    }
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 20, 1000, 500), "【绘制】拖动鼠标左键绘制数字\n【取消绘制】点击鼠标右键 \n【识别】Space \n");
    }
}