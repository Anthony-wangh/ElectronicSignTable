using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


//����
public class Paint : MonoBehaviour
{
    private GameObject _clone;
    private LineRenderer _line;
    private int _number;

    //����LineRender����
    [FormerlySerializedAs("Target")] [Header("LineRenderԤ����")]
    public GameObject target;

    [FormerlySerializedAs("PaintCamera")] public Camera paintCamera;

    //���ʲ���
    private List<GameObject> _lines=new List<GameObject>();
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //ʵ��������
            _clone = Instantiate(target, target.transform.position, Quaternion.identity);
            _lines.Add(_clone);
            //��ø������ϵ�LineRender���
            _line = _clone.GetComponent<LineRenderer>();
            //������ʼ�ͽ�������ɫ
            //_line.startColor = Color.white;
            //_line.endColor = Color.white;
            //������ʼ�ͽ����Ŀ��
            _line.startWidth = 0.2f;
            _line.endWidth = 0.1f;
            //����
            _number = 0;
        }

        if (Input.GetMouseButton(0))
        {
            //ÿһ֡��⣬��������ʱ��Խ��������Խ��
            _number++;
            //���ö�����
            _line.positionCount = _number;
            var screen = Input.mousePosition;
            var pos = paintCamera.ScreenToWorldPoint(screen);
            pos.z = 5;
            //���ö���λ��(����������������������Ļ����ת��Ϊ��������)
            _line.SetPosition(_number - 1, pos);
        }

        //����滭
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
        GUI.Label(new Rect(10, 20, 1000, 500), "�����ơ��϶���������������\n��ȡ�����ơ��������Ҽ� \n��ʶ��Space \n");
    }
}