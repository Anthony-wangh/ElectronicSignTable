using Framework.Utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PaintBrush : MonoBehaviour
{
    public static PaintBrush Inst { get; private set; }
    private RenderTexture texRender;   //����
    public Material mat;     //������shader�½�����
    public Texture brushTypeTexture;   //����������͸��
    private float brushScale = 0.01f;
    public Color brushColor = Color.black;
    public RawImage raw;                   //ʹ��UGUI��RawImage��ʾ������������UI,��pivot��Ϊ(0.5,0.5)
    public float LineWidth=1;//�ʷ���
    private float lastDistance;
    private Vector3[] PositionArray = new Vector3[3];
    private int a = 0;
    private Vector3[] PositionArray1 = new Vector3[4];
    private int b = 0;
    private float[] speedArray = new float[4];
    private int s = 0;
    public int num = 1000;
    public bool isDraw = true;
    public RenderTexture TexRender => texRender;

    Vector2 rawMousePosition;            //rawͼƬ�����½Ƕ�Ӧ���λ��
    float rawWidth;                               //rawͼƬ���
    float rawHeight;                              //rawͼƬ����

    public bool ContentIsEmpty = true;

    void Awake()
    {
        Inst = this;
        
        UIEvent.Get<UIDrag>(raw.gameObject).BeginDrag = OnBeginDrag;
        UIEvent.Get<UIDrag>(raw.gameObject).Drag = OnDrag;
        UIEvent.Get<UIDrag>(raw.gameObject).EndDrag = EndDrag;
        OnSwitchScreen(false);

        EventManager.Instance.AddListener("SendSignToServer", OnSend);
    }


    private void OnSend(object sender, EventArgs e)
    {
        ClientEventArgs args = (ClientEventArgs)e;
        if (args != null)
        {
            if (string.IsNullOrEmpty(args.TexturePath))
                return;
            ClickClear();
        }
    }

    public void OnSwitchScreen(bool isVertical) {

        if (isVertical)
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        StartCoroutine(RefreshScreen());
    }

    private IEnumerator RefreshScreen() {

        yield return new WaitForSeconds(0.5f);
        //float screenScale = Screen.width / 2048f;
        var rectrf = raw.GetComponent<RectTransform>();
        //rawͼƬ���λ�ã���ȼ���
        rawWidth = rectrf.rect.width;
        rawHeight = rectrf.rect.height;

        //���ֱ��д���� ����Ӧ������ʾǩ����image.pos.x����Ļ�Ŀ��һ��  image.pos.y ����Ļ�ĸߵ�һ�� 
        Vector2 rawanchorPositon = new Vector2(-rawWidth * 0.5f, rectrf.anchoredPosition.y - rawHeight * 0.5f);
        rawMousePosition = rawanchorPositon + new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
        texRender = RenderTexture.GetTemporary((int)rawWidth, (int)rawHeight, 24);
        raw.texture = texRender;
        Clear(texRender);

    }

    Vector3 startPosition = Vector3.zero;
    Vector3 endPosition = Vector3.zero;
    private bool _canDrag = false;

    private void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraw)
        {
            return;
        }
        _canDrag = true;
        startPosition = new Vector3(eventData.position.x, eventData.position.y, 0);
    }

    private void OnDrag(PointerEventData eventData)
    {
        if (!_canDrag)
        {
            return;
        }
        endPosition = eventData.position;
        float distance = Vector3.Distance(startPosition, endPosition);
        brushScale = SetScale(distance);
        ThreeOrderB��zierCurse(eventData.position, distance, 1f);
        startPosition = endPosition;
        lastDistance = distance;
        ContentIsEmpty = false;
    }

    private void EndDrag(PointerEventData eventData)
    {
        if (!_canDrag)
        {
            return;
        }
        _canDrag = false;
        OnMouseUp();
    }

    private void OnDestroy()
    {
        if (texRender != null)
        {
            RenderTexture.ReleaseTemporary(texRender);
        }
    }

    void Update()
    {
        //if (isDraw)
        //{
        //    if (Input.GetMouseButton(0))
        //    {
        //        OnMouseMove(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        //    }
        //    if (Input.GetMouseButtonUp(0))
        //    {
        //        OnMouseUp();
        //    }
        //    DrawImage();
        //}

    }

    void OnMouseUp()
    {
        startPosition = Vector3.zero;
        //brushScale = 0.5f;
        a = 0;
        b = 0;
        s = 0;
    }
    //���û��ʿ��
    float SetScale(float distance)
    {
        float Scale = 0;
        if (distance < 100)
        {
            Scale = 0.8f - 0.005f * distance;
        }
        else
        {
            Scale = 0.425f - 0.00125f * distance;
        }
        if (Scale <= 0.05f)
        {
            Scale = 0.05f;
        }
        return Scale;
    }

    void OnMouseMove(Vector3 pos)
    {
        if (startPosition == Vector3.zero)
        {
            startPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        }
        endPosition = pos;
        float distance = Vector3.Distance(startPosition, endPosition);
        brushScale = SetScale(distance);
        ThreeOrderB��zierCurse(pos, distance, 1f);

        startPosition = endPosition;
        lastDistance = distance;
    }

    void Clear(RenderTexture destTexture)
    {
        Graphics.SetRenderTarget(destTexture);
        GL.PushMatrix();
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        GL.PopMatrix();
    }

    void DrawBrush(RenderTexture destTexture, int x, int y, Texture sourceTexture, Color color, float scale)
    {
        DrawBrush(destTexture, new Rect(x, y, sourceTexture.width, sourceTexture.height), sourceTexture, color, scale);
    }
    void DrawBrush(RenderTexture destTexture, Rect destRect, Texture sourceTexture, Color color, float scale)
    {
        //�������λ�ø���rawͼƬλ�û��㡣
        float left = (destRect.xMin - rawMousePosition.x) * Screen.width / rawWidth - destRect.width * scale / 2.0f;
        float right = (destRect.xMin - rawMousePosition.x) * Screen.width / rawWidth + destRect.width * scale / 2.0f;
        float top = (destRect.yMin - rawMousePosition.y) * Screen.height / rawHeight - destRect.height * scale / 2.0f;
        float bottom = (destRect.yMin - rawMousePosition.y) * Screen.height / rawHeight + destRect.height * scale / 2.0f;

        Graphics.SetRenderTarget(destTexture);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetTexture("_MainTex", brushTypeTexture);
        mat.SetColor("_Color", color);
        mat.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(left / Screen.width, top / Screen.height, 0);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(right / Screen.width, top / Screen.height, 0);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(right / Screen.width, bottom / Screen.height, 0);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(left / Screen.width, bottom / Screen.height, 0);
        //GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(left / 1000, top / 1000, 0);
        //GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(right / 1000, top / 1000, 0);
        //GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(right / 1000, bottom / 1000, 0);
        //GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(left / 1000, bottom / 1000, 0);


        GL.End();
        GL.PopMatrix();
    }

    void DrawImage()
    {
        raw.texture = texRender;
    }
    public void ClickClear()
    {
        ContentIsEmpty = true;
        Clear(texRender);
    }

    //���ױ���������
    public void TwoOrderB��zierCurse(Vector3 pos, float distance)
    {
        PositionArray[a] = pos;
        a++;
        if (a == 3)
        {
            for (int index = 0; index < num; index++)
            {
                Vector3 middle = (PositionArray[0] + PositionArray[2]) / 2;
                PositionArray[1] = (PositionArray[1] - middle) / 2 + middle;

                float t = (1.0f / num) * index / 2;
                Vector3 target = Mathf.Pow(1 - t, 2) * PositionArray[0] + 2 * (1 - t) * t * PositionArray[1] +
                                 Mathf.Pow(t, 2) * PositionArray[2];
                float deltaSpeed = (float)(distance - lastDistance) / num;
                DrawBrush(texRender, (int)target.x, (int)target.y, brushTypeTexture, brushColor, SetScale(lastDistance + (deltaSpeed * index)));
            }
            PositionArray[0] = PositionArray[1];
            PositionArray[1] = PositionArray[2];
            a = 2;
        }
        else
        {
            DrawBrush(texRender, (int)endPosition.x, (int)endPosition.y, brushTypeTexture,
                brushColor, brushScale);
        }
    }
    //���ױ��������ߣ���ȡ����4�������꣬ͨ�������м�2�����꣬�������֣���ʹ����num/1.5ʵ�ֻ����������ߣ���ʹ����ƽ��;ͨ���ٶȿ������߿�ȡ�
    private void ThreeOrderB��zierCurse(Vector3 pos, float distance, float targetPosOffset)
    {
        //��¼����
        PositionArray1[b] = pos;
        b++;
        //��¼�ٶ�
        speedArray[s] = distance;
        s++;
        if (b == 4)
        {
            Vector3 temp1 = PositionArray1[1];
            Vector3 temp2 = PositionArray1[2];

            //�޸��м���������
            Vector3 middle = (PositionArray1[0] + PositionArray1[2]) / 2;
            PositionArray1[1] = (PositionArray1[1] - middle) * 1.5f + middle;
            middle = (temp1 + PositionArray1[3]) / 2;
            PositionArray1[2] = (PositionArray1[2] - middle) * 2.1f + middle;

            for (int index1 = 0; index1 < num / 1.5f; index1++)
            {
                float t1 = (1.0f / num) * index1;
                Vector3 target = Mathf.Pow(1 - t1, 3) * PositionArray1[0] +
                                 3 * PositionArray1[1] * t1 * Mathf.Pow(1 - t1, 2) +
                                 3 * PositionArray1[2] * t1 * t1 * (1 - t1) + PositionArray1[3] * Mathf.Pow(t1, 3);
                //float deltaspeed = (float)(distance - lastDistance) / num;
                //��ȡ�ٶȲ�ֵ���������⣬�ο���
                float deltaspeed = (float)(speedArray[3] - speedArray[0]) / num;
                //float randomOffset = Random.Range(-1/(speedArray[0] + (deltaspeed * index1)), 1 / (speedArray[0] + (deltaspeed * index1)));
                //ģ��ë��Ч��
                float randomOffset = Random.Range(-targetPosOffset, targetPosOffset);
                DrawBrush(texRender, (int)(target.x), (int)(target.y), brushTypeTexture, brushColor, LineWidth*SetScale(speedArray[0] + (deltaspeed * index1)));
            }

            PositionArray1[0] = temp1;
            PositionArray1[1] = temp2;
            PositionArray1[2] = PositionArray1[3];

            speedArray[0] = speedArray[1];
            speedArray[1] = speedArray[2];
            speedArray[2] = speedArray[3];
            b = 3;
            s = 3;
        }
        else
        {
            DrawBrush(texRender, (int)endPosition.x, (int)endPosition.y, brushTypeTexture,
                brushColor, LineWidth * brushScale);
        }

    }
}