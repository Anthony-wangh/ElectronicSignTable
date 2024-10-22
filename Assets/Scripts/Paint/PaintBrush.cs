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
    private RenderTexture texRender;   //画布
    public Material mat;     //给定的shader新建材质
    public Texture brushTypeTexture;   //画笔纹理，半透明
    private float brushScale = 0.01f;
    public Color brushColor = Color.black;
    public RawImage raw;                   //使用UGUI的RawImage显示，方便进行添加UI,将pivot设为(0.5,0.5)
    public float LineWidth=1;//笔锋宽度
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

    Vector2 rawMousePosition;            //raw图片的左下角对应鼠标位置
    float rawWidth;                               //raw图片宽度
    float rawHeight;                              //raw图片长度

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
        //raw图片鼠标位置，宽度计算
        rawWidth = rectrf.rect.width;
        rawHeight = rectrf.rect.height;

        //这边直接写死了 正常应该用显示签名的image.pos.x减屏幕的宽的一半  image.pos.y 减屏幕的高的一半 
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
        ThreeOrderBézierCurse(eventData.position, distance, 1f);
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
    //设置画笔宽度
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
        ThreeOrderBézierCurse(pos, distance, 1f);

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
        //增加鼠标位置根据raw图片位置换算。
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

    //二阶贝塞尔曲线
    public void TwoOrderBézierCurse(Vector3 pos, float distance)
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
    //三阶贝塞尔曲线，获取连续4个点坐标，通过调整中间2点坐标，画出部分（我使用了num/1.5实现画出部分曲线）来使曲线平滑;通过速度控制曲线宽度。
    private void ThreeOrderBézierCurse(Vector3 pos, float distance, float targetPosOffset)
    {
        //记录坐标
        PositionArray1[b] = pos;
        b++;
        //记录速度
        speedArray[s] = distance;
        s++;
        if (b == 4)
        {
            Vector3 temp1 = PositionArray1[1];
            Vector3 temp2 = PositionArray1[2];

            //修改中间两点坐标
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
                //获取速度差值（存在问题，参考）
                float deltaspeed = (float)(speedArray[3] - speedArray[0]) / num;
                //float randomOffset = Random.Range(-1/(speedArray[0] + (deltaspeed * index1)), 1 / (speedArray[0] + (deltaspeed * index1)));
                //模拟毛刺效果
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