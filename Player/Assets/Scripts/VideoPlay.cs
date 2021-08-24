using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.IO;
public class VideoPlay : MonoBehaviour {

    [Serializable]
    public class Message
    {
        public string _ip;
        public string _path;
    }
    [SerializeField]
    public List<Message> messages = new List<Message>();
    public Text videoTimeText;          // 视频的时间 Text
    public Text videoNameText;          // 视频的名字 Text
    public Slider videoTimeSlider;      // 视频的时间 Slider
    public Slider videoVoiceSlider;      // 视频的时间 Slider
    public int change_speed = 100; //加的次数
    private int currentHour;
    private int currentMinute;
    private int currentSecond;
    private int clipHour;
    private int clipMinute;
    private int clipSecond;
    private int alltime = 0;
    public Image BackImg;//底图
    public RawImage vp_RawImage;
    public VideoPlayer vp_Player;
    public AudioSource VideoAudio;//视频音量
    bool IsAdd = false;
    bool IsDel = false;
    private float fVol;
    private bool isDrag;
    public VideoPlayer Video;
    private string INIPath;
    public GameObject PlayCtrlObj;
    private int iTimer;
    private int iTimerCount = 60*20;
    private bool bShow = false;
    public Canvas canvans;
    public float width;
    public float height;
    public RawImage rawImage;
    private bool autoplay;
    private int m;
    void Awake()
    {
        INIPath = Application.streamingAssetsPath + "/分辨率配置.ini";
        IniReadFile(INIPath);
        autoplay = true;
        m = 0;
        videoTimeSlider.GetComponent<Slider>().onValueChanged.AddListener(ValueChanged);
    }
    void IniReadFile(string path)
    {
        INIParser iniParser = new INIParser();
        iniParser.Open(path);
        width = Convert.ToSingle(iniParser.ReadValue("canvas.width", "width", 0d));
        height = Convert.ToSingle(iniParser.ReadValue("canva.height", "height", 0d));
        rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(Convert.ToSingle(iniParser.ReadValue("image.width", "width", 0d)), Convert.ToSingle(iniParser.ReadValue("image.height", "height", 0d)));
        rawImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(Convert.ToSingle(iniParser.ReadValue("image.posx", "posx", 0d)), Convert.ToSingle(iniParser.ReadValue("image.posy", "posy", 0d)));
        canvans.GetComponent<CanvasScaler>().referenceResolution = new Vector2(width, height);
        PlayCtrlObj.GetComponent<RectTransform>().anchoredPosition= new Vector2(Convert.ToSingle(iniParser.ReadValue("PlayCtrl.posx", "posx", 0d)), Convert.ToSingle(iniParser.ReadValue("PlayCtrl.posy", "posy", 0d)));
        iniParser.Close();
    }
    void Start () {
       
        StartCoroutine(LoadToImage(BackImg, "" + "Texture.jpg"));
        HideObj(PlayCtrlObj);
        HClient.Instance.event_chuLiXiaoXi += Instance_event_chuLiXiaoXi;
        HClient.Instance.event_lianJieChengGongChuFa += Instance_event_lianJieChengGongChuFa;
        string path = Application.streamingAssetsPath + "/视频路径配置/";

        //获取指定路径下面的所有资源文件  
        if (Directory.Exists(path))
        {
            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files = direction.GetFiles("*");
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".json"))
                {
                    string _str = File.ReadAllText(files[i].FullName, System.Text.Encoding.UTF8);
                    Message message1 = new Message();
                    message1 = JsonUtility.FromJson<Message>(_str);
                    messages.Add(message1);
                }                                             
            }
           
        }
        Instance_event_chuLiXiaoXi("播放视频:19:0");
        Instance_event_chuLiXiaoXi("播放:19:");
    }
    private void ValueChanged(float value)
    {
        if (autoplay == true)
        {
            if (videoTimeSlider.value >=0.999)
            {
                m++;
                if (m > messages.Count)
                {
                    m = 0;
                }
                StartCoroutine(Hide());
                AutoPlay(m);
            }
        }
    }
    public IEnumerator Hide()
    {
        vp_RawImage.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        vp_RawImage.gameObject.SetActive(true);
    }
    private void Instance_event_chuLiXiaoXi(string message)
    {
        log_wm.wmlog("收到的消息"+message);
        if (!string.IsNullOrEmpty(message))
        {
            string[] astr = message.Split(':');

            if (astr[0].ToString() == ("播放视频"))
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    if (astr[2].ToString().Equals(messages[i]._ip))
                    {
                        m = int.Parse(astr[2]);
                        StartCoroutine(Hide());
                        Init(Application.streamingAssetsPath + "/视频文件/"+ messages[i]._path);
                    }
                }
            }
            if (astr[0].ToString() == ("播放"))
            {
                vp_Player.Play();
            }
            if (astr[0].ToString() == ("暂停"))
            {
                vp_Player.Pause();
            }
            if (astr[0].ToString() == ("停止"))
            {
                vp_Player.Stop();
            }
            if (astr[0].ToString() == ("快进"))
            {
                Add();
                StartCoroutine(VideoStop());
            }
            if (astr[0].ToString() == ("快退"))
            {
                Delete();
                StartCoroutine(VideoStop());
            }


        }
    }
    public IEnumerator VideoStop()
    {
        yield return new WaitForSeconds(1f);
        Stop();
    }
    private void Instance_event_lianJieChengGongChuFa()
    {
        HClient.Instance.sendMessageToServer("身份标识:0:19");
    }


    void Init(string path)
    {
        fVol = (float)1.0;
        vp_RawImage.texture = BackImg.mainTexture;
        Video.url = path;//默认视频地址
        vp_Player.url = Video.url;
        vp_Player.Play();
       

    }

   
    void ShowVideoTime()
    {
        clipHour = (int)((float)vp_Player.frameCount / (float)25 / (float)3600);
        clipMinute = (int)((float)vp_Player.frameCount / (float)25 - (float)clipHour * (float)3600) / 60;
        clipSecond = (int)((float)vp_Player.frameCount / (float)25 - (float)clipHour * (float)3600 - (float)clipMinute * (float)60);

        // 当前的视频播放时间
        currentHour = (int)vp_Player.time / 3600;
        currentMinute = (int)(vp_Player.time - currentHour * 3600) / 60;
        currentSecond = (int)(vp_Player.time - currentHour * 3600 - currentMinute * 60);
        // 把当前视频播放的时间显示在 Text 上
        videoTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D2} / {3:D2}:{4:D2}:{5:D2}",
            currentHour, currentMinute, currentSecond, clipHour, clipMinute, clipSecond);
        // 把当前视频播放的时间比例赋值到 Slider 上
        videoTimeSlider.value = (float)vp_Player.frame / (float)vp_Player.frameCount;
        // 把当前视频播放的音量比例赋值到 Slider 上
        videoVoiceSlider.value = VideoAudio.volume;
        alltime = clipHour * 60 * 60 + clipMinute * 60 + clipSecond;
    }

	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            bShow = true;
            iTimer = 0;
        }

        Show_Hide();
       
		if(vp_Player.isPlaying)
        {
            vp_RawImage.texture = vp_Player.texture;
            ShowVideoTime();
        }
        if (IsAdd == true)
        {
            videoTimeSlider.value += 0.001f;
            vp_Player.time = videoTimeSlider.value * alltime;
        }
        if (IsDel == true)
        {
            videoTimeSlider.value -= 0.001f;
            vp_Player.time = videoTimeSlider.value * alltime;
        }
      
	}
    public void AutoPlay(int j)
    {
      
        for (int i = 0; i < messages.Count; i++)
        {
            if (j.ToString().Equals(messages[i]._ip))
            {
                Debug.Log(j);
                Init(Application.streamingAssetsPath + "/视频文件/" + messages[i]._path);
            }
        }
    }

    void Show_Hide()//是否显示控制条
    { 
        if(bShow)
        {
            ShowObj(PlayCtrlObj);
            
            iTimer++;
            if (iTimer >= iTimerCount)
            {
                iTimer = 0;
                HideObj(PlayCtrlObj);
                bShow = false;
            }
        }
    }
    public void VideoPlayCtrl(int _iWhich)
    { 
        switch(_iWhich)
        {
            case 0:
                vp_Player.Play();
                break;
            case 1:
                vp_Player.Pause();
                break;
            case 2:
                videoTimeSlider.value = 0;
                vp_Player.Stop();
                break;
            case 3:
                fVol += (float)0.1;
                if (fVol >= (float)1.0)
                {
                    fVol = (float)1.0;
                }
                VideoAudio.volume = fVol;
                break;
            case 4:
                fVol -= (float)0.1;
                if (fVol <= (float)0.0)
                {
                    fVol = (float)0.0;
                }
                VideoAudio.volume = fVol;
                break;
            default:
                break;
        }
    }

    //修改进度条
    public void XiuGaiJinDuTiao()
    {
        if (!isDrag)
        {
            videoTimeSlider.value = (float)(vp_Player.time / alltime);
        }
    }
    //开始拖拽
    public void OnDragdrop()
    {
        isDrag = true;
        vp_Player.Pause();
        
    }
    //结束拖拽
    public void OnEndDrag()
    {
        isDrag = false;
        vp_Player.Play();
        vp_Player.time = videoTimeSlider.value * alltime;
    }

    public void SetVideoVoiceValueChange()
    {
        VideoAudio.volume = fVol = videoVoiceSlider.value;
    }

    string GetFilePath(string foldPath, string fileName, string fileFormat)//
    {
        string filePath;


#if UNITY_IPHONE
                if (!string.IsNullOrEmpty(foldPath))
                {
                    filePath=Application.dataPath + "/Raw/"+foldPath+"/"+fileName+fileFormat;
                }
                else
                {
                    filePath=Application.dataPath + "/Raw/"+fileName+fileFormat;
                }
#elif UNITY_ANDROID
                if (!string.IsNullOrEmpty(foldPath))
                {
                    filePath="jar:file://" + Application.dataPath + "!/assets/"+foldPath+"/"+fileName+fileFormat;
                }
                else
                {
                    filePath="jar:file://" + Application.dataPath + "!/assets/"+fileName+fileFormat;
                }
#endif
        if (!string.IsNullOrEmpty(foldPath))
        {
            filePath = UnityEngine.Application.dataPath + "/StreamingAssets/" + foldPath + "/" + fileName + fileFormat;
        }
        else
        {
            filePath = UnityEngine.Application.dataPath + "/StreamingAssets/" + fileName + fileFormat;
        }

        return filePath;
    }



    private IEnumerator LoadToImage(Image _image, string _filePath)
    {
        WWW www = new WWW("file://" + UnityEngine.Application.dataPath + "/StreamingAssets/" + _filePath);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.error);
        }
        else
        {
            if (www.isDone)
            {
                Texture2D tex = new Texture2D(64, 64);
                www.LoadImageIntoTexture(tex);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                _image.sprite = sprite;
            }
        }
    }

    void ShowObj(GameObject _obj)
    {
        _obj.gameObject.SetActive(true);
    }
    
    void HideObj(GameObject _obj)
    {
        _obj.gameObject.SetActive(false);
    }
    public void Add()
    {
        vp_Player.Pause();
        IsAdd = true;
    }
    public void Delete()
    {
        vp_Player.Pause();
        IsDel = true;

    }
    public void Stop()
    {
        HideObj(PlayCtrlObj);
        IsAdd = false;
        IsDel = false;
        StartCoroutine(ShowA());
    }
    private IEnumerator ShowA()
    {
        yield return new WaitForSeconds(1f);
        vp_Player.Play();
    }
}
