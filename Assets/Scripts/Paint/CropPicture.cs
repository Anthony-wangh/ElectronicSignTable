using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 截图
/// </summary>
public class CropPicture : MonoBehaviour
{
    public Camera cropCamera; //待截图的目标摄像机
    public Vector2 Size = new Vector2(2048,2048);
    private RenderTexture _renderTexture;
    private Texture2D _texture2D;
    public RawImage RawImage;
    public RawImage SignRawImage;
    //调试时，保存在工程本地
    public bool DebugTexture = true;

    private string _fileName => Application.dataPath + "/Capture.png";

    private void Start()
    {
            
    }

    public void Send() {

        SignRawImage.texture = PaintBrush.Inst.TexRender;
        _renderTexture = RenderTexture.GetTemporary((int)Size.x, (int)Size.y, 24);
        _texture2D = new Texture2D((int)Size.x, (int)Size.y);
        cropCamera.targetTexture = _renderTexture;
        RenderTexture.active = _renderTexture;
        cropCamera.Render();
        _texture2D.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        _texture2D.Apply();
        RenderTexture.active = null;
        cropCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(_renderTexture);
        byte[] screenShotBytes = _texture2D.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fileName, screenShotBytes);
        //交给客户端发送给服务端
        this.TriggerEvent("SendSignToServer", new ClientEventArgs() {TexturePath= _fileName });
        Destroy(_texture2D);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)&& DebugTexture)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(_fileName);
            _texture2D = new Texture2D(Screen.width, Screen.height);
            _texture2D.LoadImage(bytes);
            if(RawImage)
                RawImage.texture = _texture2D;
        }
    }
}