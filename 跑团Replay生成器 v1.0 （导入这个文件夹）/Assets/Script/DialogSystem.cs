using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/*
 * Copyright (c) DaNGo_iz. All rights reserved.
 * 更多工具和游戏请访问网站：www.dangoiz.com
 * 默认素材：COC模组《请勿点击》的Replay Log
*/

public class DialogSystem : MonoBehaviour
{
    public string[] namesOfNPC;
    Dictionary<string, int> namesToNum = new Dictionary<string, int>();
    Dictionary<int, string> numToNames = new Dictionary<int, string>();
    Dictionary<string, Sprite> strToFaces = new Dictionary<string, Sprite>();

    public float textSpeed;
    public float autoTextWaitingSpeed;
    Dictionary<string, int> strToIntTexts = new Dictionary<string, int>();
    Dictionary<int, string> intToStrTexts = new Dictionary<int, string>();
    int startChapterIndex; //

    float tempTextSpeed;
    int index;
    Dictionary<string, Sprite> strToScenes = new Dictionary<string, Sprite>();
    Dictionary<string, Sprite> strToNewChapterFitstScene = new Dictionary<string, Sprite>();
    Dictionary<int, string> intToNCFSstr = new Dictionary<int, string>();
    Dictionary<string, Sprite> strToForegroundScenes = new Dictionary<string, Sprite>();

    [Header("Music")]
    public AudioClip[] bgmClips;
    public AudioClip[] bgsClips;
    public AudioClip[] typingSoundClips;
    [Range(0, 1)]
    public float typingVolume;
    [Space(15)]
    public AudioSource diceSound;

    AudioSource bgmSources;
    bool changeBGM;
    int bgmIndex;

    AudioSource bgsSources;
    bool changeBGS;
    int bgsIndex;

    AudioSource typingSound;

    [Header("UI Components")]
    [Space(50)]
    public GameObject chapterButtonPrefab;
    public Text textLabel;
    public Image littleArrow;
    public Image shining;
    public Image background;
    public Canvas foreground;
    public float spriteSpeed;
    public Button menuButton;
    public Button autoButton;
    public Button autoButtonClosed;
    List<string> textList = new List<string>();
    public Canvas menu;
    int chapterNumber;
    int maxChapter;

    float shakeTime = 1.0f;
    private float currentTime = 0.0f;
    private List<Vector3> gameobjpons = new List<Vector3>();
    public Camera cameraToShake;
    bool shake;

    public Image[] faceImage;
    public Image[] nameImage;
    public Text[] nameLabel;
    bool[] slotEmpty = new bool[4];

    bool textFinished;
    bool cancelTyping;
    bool bigger;
    bool betterInterface;
    bool isChangingScene;
    bool menuIsOpen;
    bool foregroundIsOpen;
    bool auto;
    bool nextLine;
    bool getTypingSound;

    bool textCreateFinished;

    void Awake()     {
        TextCreate();
        menu.gameObject.SetActive(false);

        GetTextFromFile(intToStrTexts[startChapterIndex]); //

        tempTextSpeed = textSpeed;
        for (int i = 0; i < 4; i++)
        {
            slotEmpty[i] = true;
            SlotInvisible(faceImage[i], nameImage[i], nameLabel[i]);
        }

        for (int i = 0; i < namesOfNPC.Length; i++)
        {
            namesToNum.Add(namesOfNPC[i], i);
            numToNames.Add(i, namesOfNPC[i]);
        }

        FileInfo[] backgroundsFileName = AllFileNames(Application.streamingAssetsPath + "/Background");
        for (int i = 0; i < backgroundsFileName.Length; i++)
        {
            strToScenes.Add(backgroundsFileName[i].Name, GetSpriteFromFilePath(Application.streamingAssetsPath + "/Background/" + backgroundsFileName[i].Name));
        }

        FileInfo[] backgroundsNewFileName = AllFileNames(Application.streamingAssetsPath + "/BackgroundInNewChapter");
        for (int i = 0; i < backgroundsNewFileName.Length; i++)
        {
            strToNewChapterFitstScene.Add(backgroundsNewFileName[i].Name, GetSpriteFromFilePath(Application.streamingAssetsPath + "/BackgroundInNewChapter/" + backgroundsNewFileName[i].Name));
            intToNCFSstr.Add(i, backgroundsNewFileName[i].Name);
        }
        background.sprite = strToNewChapterFitstScene["sc00.png"];

        FileInfo[] foregroundsFileName = AllFileNames(Application.streamingAssetsPath + "/Foreground");
        for (int i = 0; i < foregroundsFileName.Length; i++)
        {
            strToForegroundScenes.Add(foregroundsFileName[i].Name, GetSpriteFromFilePath(Application.streamingAssetsPath + "/Foreground/" + foregroundsFileName[i].Name));
        }

        FileInfo[] characterFacesFileName = AllFileNames(Application.streamingAssetsPath + "/Characters");
        for (int i = 0; i < characterFacesFileName.Length; i++)
        {
            strToFaces.Add(characterFacesFileName[i].Name, GetSpriteFromFilePath(Application.streamingAssetsPath + "/Characters/" + characterFacesFileName[i].Name));
        }
    }

    private void OnEnable()     {
        bgmSources = this.gameObject.AddComponent<AudioSource>();
        bgmSources.loop = true;
        bgmSources.volume = 0.6f;
        bgmSources.pitch = 1;
        bgsSources = this.gameObject.AddComponent<AudioSource>();
        bgsSources.loop = false;
        bgsSources.volume = 1f;

        typingSound = this.gameObject.AddComponent<AudioSource>();
        typingSound.loop = false;
        typingSound.playOnAwake = false;
        typingSound.clip = typingSoundClips[typingSoundClips.Length - 1];

        textFinished = true;
        StartCoroutine(SetTextUI());
    }

    void Update()     {

        if (changeBGM)
        {
            bgmSources.clip = bgmClips[bgmIndex];
            bgmSources.volume = 0.6f;
            bgmSources.Play();
            changeBGM = false;
        }

        if (changeBGS)
        {
            bgsSources.clip = bgsClips[bgsIndex];
            bgsSources.Play();
            changeBGS = false;
        }

        if (!auto)
        {
            if (Input.GetKeyDown(KeyCode.Space) && index == textList.Count && !menuIsOpen)
            {
                textLabel.text = "——本章完——";
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) && !menuIsOpen)
            {
                if (textFinished && !cancelTyping)
                {
                    StartCoroutine(SetTextUI());

                }
                else if (!textFinished)
                {
                    cancelTyping = !cancelTyping;
                }
            }
        }
        else
        {
            if (nextLine && index == textList.Count && !menuIsOpen)
            {
                textLabel.text = "——本章完——";
                return;
            }

            if (nextLine && !menuIsOpen)
            {
                if (textFinished && !cancelTyping)
                {
                    StartCoroutine(SetTextUI());
                }
                else if (!textFinished)
                {
                    cancelTyping = !cancelTyping;
                }
            }
        }

        if (shake)         {
            currentTime = shakeTime;
            shake = false;
        }
    }

    void LateUpdate() {
        UpdateShake();
    }

    void TextCreate()     {
        FileInfo[] textFileName = AllFileNames(Application.streamingAssetsPath + "/TextFile");
        int buttonPosition = 3;
        int childIndex = 0;
        for (int i = 0; i < textFileName.Length; i++)
        {
            strToIntTexts.Add(Application.streamingAssetsPath + "/TextFile/" + textFileName[i].Name, i);
            intToStrTexts.Add(i, Application.streamingAssetsPath + "/TextFile/" + textFileName[i].Name);
            if(textFileName[i].Name.Substring(0, 2) == "00" && textFileName[i].Name.EndsWith(".txt")) //
            {
                startChapterIndex = i;
            }
            if (textFileName[i].Name.EndsWith(".txt"))
            {
                GameObject menu0 = GameObject.Find("Menu");
                GameObject button0 = Instantiate(chapterButtonPrefab);
                button0.transform.SetParent(menu0.transform);
                button0.transform.position = new Vector3(0, buttonPosition, 0);
                buttonPosition -= 1;
                Text currencyText = GameObject.Find("Menu").transform.GetChild(childIndex + 1).gameObject.GetComponent<Text>();
                maxChapter = childIndex;
                currencyText.text = textFileName[i].Name.Substring(2, textFileName[i].Name.Length - 6);
                button0.GetComponent<Button>().onClick.AddListener(delegate ()
                {
                    for (int j = 0; j < textFileName.Length; j++)
                    {
                        char[] buttonChar = intToStrTexts[j].ToCharArray();
                        int buttonCharIndex = 0;
                        for (int k = 0; k < buttonChar.Length; k++)
                        {
                            if(buttonChar[k] == '/')
                            {
                                buttonCharIndex = k;
                            }
                        }
                        string buttonStr = "";
                        string fullButtonStr = "";
                        for (int k = buttonCharIndex + 3; k < buttonChar.Length - 4; k++)
                        {
                            buttonStr += buttonChar[k];
                        }
                        for (int k = buttonCharIndex + 1; k < buttonChar.Length - 4; k++)
                        {
                            fullButtonStr += buttonChar[k];
                        }
                        if (button0.GetComponent<Text>().text == buttonStr)
                        {
                            auto = false;
                            autoButton.gameObject.SetActive(true);
                            autoButtonClosed.gameObject.SetActive(false);
                            ChooseChaper(fullButtonStr);
                            chapterNumber = int.Parse(intToStrTexts[j].Substring(28, intToStrTexts[j].Length - 32));
                        }
                    }
                });
                childIndex++;
            }
        }
    }

    FileInfo[] AllFileNames(string path)     {
        if (Directory.Exists(path))
        {
            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".meta"))
                {
                    continue;
                }
            }
            return files;
        }
        return null;
    }

    Sprite GetSpriteFromFilePath(string path)     {
        byte[] ImgByte = getImageByte(path);
        Texture2D texture2D = new Texture2D(1048, 1632);
        texture2D.LoadImage(ImgByte);
        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    void ChangeImage(string name, Image image)     {
        string pathStr = Application.streamingAssetsPath + "/" + name + ".png";
        TextureToSprite(getImageByte(pathStr), image);
    }

    private void TextureToSprite(byte[] ImgByte, Image image)     {
        Texture2D texture2D = new Texture2D(1080, 1920);
        texture2D.LoadImage(ImgByte);
        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
    }

    private static byte[] getImageByte(string imagePath)     {
        FileStream files = new FileStream(imagePath, FileMode.Open);
        byte[] imgByte = new byte[files.Length];
        files.Read(imgByte, 0, imgByte.Length);
        files.Close();
        return imgByte;
    }

    void GetTextFromFile(string path)     {
        string[] line = File.ReadAllLines(path);
        textList.Clear();
        index = 0;
        for (int i = 0; i < line.Length; i++)
        {
            textList.Add(line[i]);
        }
    }

    void BetterInterface()     {
        int num = NumberOfFullSlots();
        int[] index = IndexOfFullSlots();

        if(num == 1)
        {
            faceImage[index[0]].transform.position = new Vector3(0, 0, 0);
            nameImage[index[0]].transform.position = new Vector3(0, -1, 0);
        }
        else if(num == 2)
        {
            faceImage[index[0]].transform.position = new Vector3(-3, 0, 0);
            nameImage[index[0]].transform.position = new Vector3(-3, -1, 0);
            faceImage[index[1]].transform.position = new Vector3(3, 0, 0);
            nameImage[index[1]].transform.position = new Vector3(3, -1, 0);
        }
        else if(num == 3)
        {
            faceImage[index[0]].transform.position = new Vector3(-4.5f, 0, 0);
            nameImage[index[0]].transform.position = new Vector3(-4.5f, -1, 0);
            faceImage[index[1]].transform.position = new Vector3(0, 0, 0);
            nameImage[index[1]].transform.position = new Vector3(0, -1, 0);
            faceImage[index[2]].transform.position = new Vector3(4.5f, 0, 0);
            nameImage[index[2]].transform.position = new Vector3(4.5f, -1, 0);
        }
        else if(num == 4)
        {
            faceImage[index[0]].transform.position = new Vector3(-6, 0, 0);
            nameImage[index[0]].transform.position = new Vector3(-6, -1, 0);
            faceImage[index[1]].transform.position = new Vector3(-2, 0, 0);
            nameImage[index[1]].transform.position = new Vector3(-2, -1, 0);
            faceImage[index[2]].transform.position = new Vector3(2, 0, 0);
            nameImage[index[2]].transform.position = new Vector3(2, -1, 0);
            faceImage[index[3]].transform.position = new Vector3(6, 0, 0);
            nameImage[index[3]].transform.position = new Vector3(6, -1, 0);
        }
    }

    void AddCharacterToSreen(string name, Sprite face)     {
        int i = 0;
        while (i < 4)
        {
            if (slotEmpty[i])
            {
                faceImage[i].sprite = face;
                nameLabel[i].text = name;
                slotEmpty[i] = false;
                i = 4;
            }
            i++;
        }
    }

    int[] RestOfFour(int num)     {
        int[] three = new int[3];
        int j = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i != num)
            {
                three[j] = i;
                j++;
            }
        }
        return three;
    }

    int NumberOfFullSlots()     {
        int num = 0;
        for (int i = 0; i < 4; i++)
        {
            if (!slotEmpty[i])
            {
                num++;
            }
        }
        return num;
    }

    int[] IndexOfFullSlots()     {
        int num = NumberOfFullSlots();
        int[] index = new int[num];
        int j = 0;
        for (int i = 0; i < 4; i++)
        {
            if (!slotEmpty[i])
            {
                index[j] = i;
                j++;
            }
        }
        return index;
    }

    public void ChooseChaper(string chapter)     {
        int temp = strToIntTexts[Application.streamingAssetsPath + "/TextFile/" + chapter + ".txt"];
        GetTextFromFile(intToStrTexts[temp]);
        CloseMenu();
        chapterNumber = int.Parse(chapter.Substring(0,2));
        StartCoroutine(ChangeChapter(chapterNumber));
    }

    IEnumerator ChangeChapter(int chapter)     {
        shining.color = new Vector4(0, 0, 0, 0);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.75f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 1);
        for (int i = 0; i < 4; i++)
        {
            SlotInvisible(faceImage[i], nameImage[i], nameLabel[i]);
            SlotClear(faceImage[i], nameImage[i], nameLabel[i]);
            slotEmpty[i] = true;
            nameLabel[i].text = "";
        }
        StartCoroutine(SetTextUI());
        if (chapter < 10)
        {
            background.sprite = strToNewChapterFitstScene["sc0" + chapter.ToString() + ".png"];
        }
        else
        {
            background.sprite = strToNewChapterFitstScene["sc" + chapter.ToString() + ".png"];
        }

        yield return new WaitForSeconds(0.5f);
        shining.color = new Vector4(0, 0, 0, 0.75f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0);
    }

    public void OpenMenu()     {
        menuIsOpen = true;
        menu.gameObject.SetActive(true);
        if (!Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Show(0.75f, menu.GetComponent<Image>()));
            StartCoroutine(Hide(0, menuButton.image));
            StartCoroutine(Hide(0, autoButton.image));
            StartCoroutine(Hide(0, autoButtonClosed.image));
        }
    }

    public void CloseMenu()     {
        menuIsOpen = false;
        StartCoroutine(Hide(0, menu.GetComponent<Image>()));
        menu.gameObject.SetActive(false);
        StartCoroutine(Show(1, menuButton.image));
        StartCoroutine(Show(1, autoButton.image));
        StartCoroutine(Show(1, autoButtonClosed.image));
    }

    IEnumerator Show(float f, Image image)     {
        image.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(0, 0, 0, f);
    }

    IEnumerator ShowButton(float f, Image image)     {
        image.color = new Vector4(1, 1, 1, 0.25f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(1, 1, 1, f);
    }

    IEnumerator Hide(float f, Image image)     {
        image.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(0, 0, 0, f);
    }

    public void Auto(bool a)
    {
        auto = a;
        if (auto && textFinished && !cancelTyping)
        {
            StartCoroutine(SetTextUI());
        }
    }

    IEnumerator ChangeScene(Sprite sprite)     {
        for (int j = 0; j < 4; j++)
        {
            CharacterGrey(slotEmpty[j], faceImage[j], nameImage[j], nameLabel[j]);
        }
        shining.color = new Vector4(0, 0, 0, 0);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.75f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 1);
        background.sprite = sprite;
        yield return new WaitForSeconds(0.5f);
        shining.color = new Vector4(0, 0, 0, 0.75f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0);
    }

    IEnumerator ShowForeground(Sprite sprite)     {
        for (int j = 0; j < 4; j++)
        {
            CharacterGrey(slotEmpty[j], faceImage[j], nameImage[j], nameLabel[j]);
        }
        shining.color = new Vector4(0, 0, 0, 0);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.75f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 1);
        foreground.gameObject.SetActive(true);
        foreground.GetComponent<Image>().sprite = sprite;
        yield return new WaitForSeconds(0.5f);
        shining.color = new Vector4(0, 0, 0, 0.75f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0);
        foregroundIsOpen = true;
        yield return new WaitForSeconds(2f);
    }

    IEnumerator CloseForeground()     {
        shining.color = new Vector4(0, 0, 0, 0);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.75f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 1);
        foreground.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        shining.color = new Vector4(0, 0, 0, 0.75f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0.25f);
        yield return new WaitForSeconds(0.1f);
        shining.color = new Vector4(0, 0, 0, 0);
    }

    IEnumerator StopPlayingBGM()
    {
        bgmSources.volume = 0.3f;
        yield return new WaitForSeconds(0.5f);
        bgmSources.volume = 0.15f;
        yield return new WaitForSeconds(0.5f);
        bgmSources.volume = 0f;
    }

    IEnumerator StartPlayingBGM()
    {
        bgmSources.volume = 0.15f;
        yield return new WaitForSeconds(0.5f);
        bgmSources.volume = 0.3f;
        yield return new WaitForSeconds(0.5f);
        bgmSources.volume = 0.6f;
    }

    void CharacterGrey(bool empty, Image faceImage, Image nameImage, Text nameLabel)     {
        if (!empty)
        {
            faceImage.color = new Vector4(0.5f, 0.5f, 0.5f, 1);
            nameImage.color = new Vector4(0.5f, 0.5f, 0.5f, 1);
            nameLabel.color = new Vector4(0.5f, 0.5f, 0.5f, 1);
        }
    }

    void CharacterBright(bool empty, Image faceImage, Image nameImage, Text nameLabel)     {
        if (!empty)
        {
            faceImage.color = new Vector4(1, 1, 1, 1);
            nameImage.color = new Vector4(1, 1, 1, 1);
            nameLabel.color = new Vector4(1, 1, 1, 1);
        }
    }

    IEnumerator Wait(Image image)     {
        if (bigger == false)         {
            image.transform.localScale += new Vector3(0.003f, 0.003f, 0);
            bigger = true;
            yield return new WaitForSeconds(spriteSpeed);
            image.transform.localScale -= new Vector3(0.003f, 0.003f, 0);
        }
        bigger = false;
    }

    IEnumerator Shine(Image image, int r, int g, int b)     {
        image.color = new Vector4(r, g, b, 0);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(r, g, b, 0.25f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(r, g, b, 0.5f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(r, g, b, 0.75f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(r, g, b, 1);
        yield return new WaitForSeconds(0.3f);
        image.color = new Vector4(r, g, b, 0.75f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(r, g, b, 0.5f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(r, g, b, 0.25f);
        yield return new WaitForSeconds(0.1f);
        image.color = new Vector4(r, g, b, 0);
        yield return new WaitForSeconds(0.1f);
    }

    void SlotInvisible(Image faceImage,Image nameImage,Text nameLabel)     {
        faceImage.color = new Vector4(0, 0, 0, 0);
        nameImage.color = new Vector4(0, 0, 0, 0);
        nameLabel.color = new Vector4(0, 0, 0, 0);
    }

    void UpdateShake()     {
        if (currentTime > 0.0f)
        {
            currentTime -= Time.deltaTime;
            cameraToShake.rect = new Rect(0.04f * (-1.0f + 2.0f * Random.value) * Mathf.Pow(currentTime, 2), 0.04f * (-1.0f + 2.0f * Random.value) * Mathf.Pow(currentTime, 2), 1.0f, 1.0f);
        }
        else
        {
            currentTime = 0.0f;
        }
    }

    void SlotClear(Image faceImage, Image nameImage, Text nameLabel)
    {
        faceImage = null;
        nameImage = null;
        nameLabel = null;
    }

    void OnOff()     {
        if (textList[index].Substring(textList[index].Length - 2, 2) == "上场")
        {
            for (int k = 0; k < namesOfNPC.Length; k++)
            {
                if (textList[index] == numToNames[k] + textList[index].Substring(textList[index].Length - 4, 4))
                {
                    AddCharacterToSreen(numToNames[k], strToFaces[numToNames[k] + textList[index].Substring(textList[index].Length - 4, 2) + ".png"]);
                    index++;
                }
            }
        }
        else if (textList[index].Substring(textList[index].Length - 2, 2) == "退场")
        {
            for (int k = 0; k < namesOfNPC.Length; k++)
            {
                if (textList[index] == numToNames[k] + textList[index].Substring(textList[index].Length - 2, 2))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (nameLabel[i].text == numToNames[k])
                        {
                            nameLabel[i].text = "";
                            slotEmpty[i] = true;
                            SlotInvisible(faceImage[i], nameImage[i], nameLabel[i]);
                            index++;
                        }
                    }
                }
            }
        }
    }

    IEnumerator SetTextUI()     {
        typingSound.volume = typingVolume;
        textLabel.color = new Vector4(0, 0, 0, 1);
        textFinished = false;
        textLabel.text = "";
        littleArrow.color = new Vector4(1, 1, 1, 0);
        nextLine = false;
        if (foregroundIsOpen)
        {
            StartCoroutine(CloseForeground());
            foregroundIsOpen = false;
            yield return new WaitForSeconds(0.5f);
        }

        for (int i = 0; i < 8; i++)
        {
            OnOff();
        }

        if (textList[index].Substring(textList[index].Length - 3, 1) == "：")
        {
            for (int k = 0; k < namesOfNPC.Length; k++)
            {
                if (textList[index] == numToNames[k] + "：" + textList[index].Substring(textList[index].Length - 2, 2))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (nameLabel[i].text == numToNames[k])
                        {
                            int[] rest = RestOfFour(i);
                            for (int j = 0; j < 3; j++)
                            {
                                CharacterGrey(slotEmpty[rest[j]], faceImage[rest[j]], nameImage[rest[j]], nameLabel[rest[j]]);
                            }
                            faceImage[i].sprite = strToFaces[numToNames[k] + textList[index].Substring(textList[index].Length - 2, 2) + ".png"];
                            nameLabel[i].text = numToNames[k];
                            CharacterBright(slotEmpty[i], faceImage[i], nameImage[i], nameLabel[i]);
                            bigger = false;
                            StartCoroutine(Wait(faceImage[i]));

                            if (k < typingSoundClips.Length - 1)
                            {
                                typingSound.clip = typingSoundClips[k];
                            }
                            else
                            {
                                typingSound.clip = typingSoundClips[typingSoundClips.Length - 1];
                            }
                        }
                    }
                    index++;
                }
            }
        }
        else if (textList[index].Substring(textList[index].Length - 1, 1) == "：")         {
            for (int k = 0; k < namesOfNPC.Length; k++)
            {
                if (textList[index] == numToNames[k] + "：")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (nameLabel[i].text == numToNames[k])
                        {

                            int[] rest = RestOfFour(i);
                            for (int j = 0; j < 3; j++)
                            {
                                CharacterGrey(slotEmpty[rest[j]], faceImage[rest[j]], nameImage[rest[j]], nameLabel[rest[j]]);
                            }
                            faceImage[i].sprite = strToFaces[numToNames[k] + "00.png"];
                            nameLabel[i].text = numToNames[k];
                            CharacterBright(slotEmpty[i], faceImage[i], nameImage[i], nameLabel[i]);
                            bigger = false;
                            StartCoroutine(Wait(faceImage[i]));
                        }
                    }
                    index++;
                }
            }
        }

        if (textList[index].Length == 1)
        {
            Debug.Log("目前一行只有一个字会报错，可以先在后面加句号/省略号等");
        }

        int letter = 0;
        while (letter < textList[index].Length)
        {
            bool typing = true;
            if (textList[index][letter] == '#')
            {
                typing = false;
                letter++;
                switch (textList[index][letter])
                {
                case '>':
                    textLabel.fontSize += 10;
                    break;
                case '<':
                    textLabel.fontSize -= 10;
                    break;
                case 'b':
                    letter++;
                    switch (textList[index][letter])
                    {
                    case 'g':
                        letter++;
                        switch (textList[index][letter])
                        {
                        case 'm':
                            letter++;

                            if(textList[index][letter] == 'x')
                            {
                                StartCoroutine(StopPlayingBGM());
                            }
                            else if(textList[index][letter] == 'o')
                            {
                                StartCoroutine(StartPlayingBGM());
                            }
                            else
                            {
                                char[] csBgm = { textList[index][letter], textList[index][letter + 1] };
                                letter++;
                                string strBgm = new string(csBgm);
                                bgmIndex = int.Parse(strBgm);
                                changeBGM = true;
                            }
                            break;
                        case 's':
                            letter++;
                            char[] csBgs = { textList[index][letter], textList[index][letter + 1] };
                            letter++;
                            string strBgs = new string(csBgs);
                            bgsIndex = int.Parse(strBgs);
                            changeBGS = true;
                            break;
                        }
                        break;
                    }
                    break;
                case 'c':
                    letter++;
                    switch (textList[index][letter])
                    {
                    case 'b':
                        textLabel.color = new Vector4(0, 0, 1, 1);
                        break;
                    case 'g':
                        letter++;
                        char[] cs = { textList[index][letter], textList[index][letter + 1] };
                        letter++;
                        string str = new string(cs);
                        str = "cg" + str + ".png";
                        StartCoroutine(ShowForeground(strToForegroundScenes[str]));
                        break;
                    case 'r':
                        textLabel.color = new Vector4(1, 0, 0, 1);
                        break;
                    case 'h':
                        textLabel.color = new Vector4(0, 0, 0, 1);
                        break;
                    }
                    break;
                case 'd':
                    diceSound.Play();
                    break;
                case 'k':
                    letter++;
                    switch (textList[index][letter])
                    {
                    case 'p':
                        for (int i = 0; i < 4; i++)
                        {
                            CharacterGrey(slotEmpty[i], faceImage[i], nameImage[i], nameLabel[i]);
                        }
                        typingSound.clip = typingSoundClips[typingSoundClips.Length - 1];
                        break;
                    }
                    break;
                case 'l':
                    letter++;
                    switch (textList[index][letter])
                    {
                    case 'b':
                        StartCoroutine(Shine(shining, 0, 0, 0));
                        break;
                    case 'r':
                        StartCoroutine(Shine(shining, 1, 0, 0));
                        break;
                    case 'w':
                        StartCoroutine(Shine(shining, 1, 1, 1));
                        break;
                    }
                    break;
                case 's':
                    letter++;
                    switch (textList[index][letter])
                    {
                    case '>':
                        textSpeed *= 1 / 3;
                        break;
                    case '<':
                        textSpeed *= 6;
                        break;
                    case 'c':
                        letter++;
                        char[] cs = { textList[index][letter], textList[index][letter + 1] };
                        letter++;
                        string str = new string(cs);
                        str = "sc" + str + ".png";
                        StartCoroutine(ChangeScene(strToScenes[str]));
                        isChangingScene = true;
                        break;
                    }
                    break;
                case 'y':
                    shake = true;
                    break;
                }
                if(letter < textList[index].Length - 1)                 {
                    letter++;
                }
            }

            if(typing)
            {
                typingSound.Play();
            }

            textLabel.text += textList[index][letter];
            letter++;

            if (!betterInterface)
            {
                BetterInterface();
            }

            if (cancelTyping)
            {
                typingSound.volume = 0;
                textSpeed = 0.000001f;
            }

            if (isChangingScene)
            {
                yield return new WaitForSeconds(1);
                isChangingScene = false;
            }

            yield return new WaitForSeconds(textSpeed);
        }

        cancelTyping = false;
        textFinished = true;
        textSpeed = tempTextSpeed;
        littleArrow.color = new Vector4(0, 0, 0, 1);
        if (auto)
        {
            yield return new WaitForSeconds(autoTextWaitingSpeed);
            nextLine = true;
        }

        index++;
    }
}
