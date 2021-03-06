using I2.Loc;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public GameObject mainUI, arPanel, scanTitle, bottomPanel, qrDownPanel, localizePhonicsPanel, localizeUIPanel, localizePopupButton,
        swapCamButton, replayButton, recordToastPanel, exitPanel, usePanel, aboutUsPanel, bookPanel, loadingPanel, toastMsgPanel, mrPanel;
    public Image localizeUiImage, localizePhonicsImage;
    public Text localUITitle, localUiToggle, txt_version, txt_version_main;
    public Camera arCamera;
    public Button btn_myVoice;
    public Button btn_AboutUs;
    public Button btn_Scan;

    private GameObject coverVideo;

    [HideInInspector]
    public string scanString, scanSingleString, scanMultiString, btnSingleString, btnMultiString, phoSentenceString, phoWordString;
    [HideInInspector]
    public int botFontSize;
    [HideInInspector]
    public Font localizeFont;
    private Text btnStartScan, btnHowtouse, btnColoring, btnMRGallery, btnBotScanButton, serialFail, loginFail, signinFail, successText;
    private Button[] localizePhonicsButton;
    private Button pushedBtn;

    ARManager arManager;
    AccountManager accountManager;
    CheckCode checkCode;
    PrefabLoader prefabLoader;
    PrefabShelter prefabShelter;
    AnimalDataSetLoader aDSL;
    [HideInInspector]
    public Phonics phonics;

    public string ui_CurrentLang;
    public bool isStartLangPanel;
    public bool isToastOn = false;
    public bool isCoverTarget = false;
    public bool isNotToastAgain;
    public bool isNotCautionAgain;
    public bool isMR = false;
    bool isPhonics = false;
    bool isTitle = true;
    bool isFrontCam = false;
    bool isSingleTarget;


    #region AWAKE_AND_QUIT
    void Awake()
    {
        Manager.isMR = false;
        StartPanelSetting();

        Button[] mainbtn = mainUI.GetComponentsInChildren<Button>();
        foreach (Button button in mainbtn)
        {
            button.onClick.AddListener(() => OnClick(button));
            if (button.name.Equals("StartScan"))
                btnStartScan = button.GetComponentInChildren<Text>();
            if (button.name.Equals("HowToUse"))
                btnHowtouse = button.GetComponentInChildren<Text>();
            if (button.name.Equals("Coloring"))
                btnColoring = button.GetComponentInChildren<Text>();
            if (button.name.Equals("MRGallery"))
                btnMRGallery = button.GetComponentInChildren<Text>();
        }

        Button[] bottomButton = bottomPanel.GetComponentsInChildren<Button>();
        foreach (Button button in bottomButton)
        {
            Button temp = button;
            button.onClick.AddListener(() => BottomPanelButton(temp));
            if (temp.name.Equals("btn_Scan"))
                btnBotScanButton = temp.GetComponentInChildren<Text>();
        }

        Button[] exitButton = exitPanel.GetComponentsInChildren<Button>();
        foreach (Button button in exitButton)
        {
            button.onClick.AddListener(() => ExitControll(button));
        }

        localizePhonicsButton = localizePhonicsPanel.GetComponentsInChildren<Button>();
        foreach (Button button in localizePhonicsButton)
        {
            Button temp = button;
            button.onClick.AddListener(() => LocalizeUI(temp));

            if (button.name.Equals("custom"))
                btn_myVoice = button;
        }

        Button[] localizeUIButton = localizeUIPanel.GetComponentsInChildren<Button>();
        foreach (Button button in localizeUIButton)
        {
            Button temp = button;
            button.onClick.AddListener(() => LocalizeUI(temp));
        }
        localizePopupButton.GetComponent<Button>().onClick.AddListener(() => LocalizeButton(localizePhonicsPanel));

        Toggle localizeToggle = localizeUIPanel.GetComponentInChildren<Toggle>();
        localizeToggle.onValueChanged.AddListener(delegate
        {
            LocalizeUiValueChanged(localizeToggle.isOn);
        });

        arPanel.GetComponentInChildren<Button>().onClick.AddListener(() => OnTargetOffObject(false));
        swapCamButton.GetComponent<Button>().onClick.AddListener(() => SwapCamera());
        replayButton.GetComponent<Button>().onClick.AddListener(() => ReplayPhonics());

        Button[] recordToastButton = recordToastPanel.GetComponentsInChildren<Button>();
        foreach (Button btn in recordToastButton)
        {
            btn.onClick.AddListener(() => RecordToastController(btn));
        }

        btn_AboutUs.onClick.AddListener(() =>
        {
            aboutUsPanel.SetActive(true);
            aboutUsPanel.GetComponentInChildren<Scrollbar>().value = 1;

            BottomBarOpen();
        });
    }


    void OnApplicationQuit()
    {
        LocalizationManager.CurrentLanguage = ui_CurrentLang;
        checkCode.SaveCurrentUiLanguage();
        checkCode.SaveOnTrackingObject();

        Button[] mainbtn = mainUI.GetComponentsInChildren<Button>();
        foreach (Button button in mainbtn)
        {
            Button temp = button;
            button.onClick.RemoveListener(() => OnClick(temp));
        }

        Button[] bottomButton = bottomPanel.GetComponentsInChildren<Button>();
        foreach (Button button in bottomButton)
        {
            Button temp = button;
            button.onClick.RemoveListener(() => BottomPanelButton(temp));
        }

        Button[] exitButton = exitPanel.GetComponentsInChildren<Button>();
        foreach (Button button in exitButton)
        {
            button.onClick.RemoveListener(() => ExitControll(button));
        }

        foreach (Button button in localizePhonicsButton)
        {
            Button temp = button;
            button.onClick.RemoveListener(() => LocalizeUI(temp));
        }

        Button[] localizeUIButton = localizeUIPanel.GetComponentsInChildren<Button>();
        foreach (Button button in localizeUIButton)
        {
            Button temp = button;
            button.onClick.RemoveListener(() => LocalizeUI(temp));
        }
        localizePopupButton.GetComponent<Button>().onClick.RemoveListener(() => LocalizeButton(localizePhonicsPanel));

        Toggle localizeToggle = localizeUIPanel.GetComponentInChildren<Toggle>();
        localizeToggle.onValueChanged.RemoveAllListeners();

        arPanel.transform.GetChild(3).gameObject.SetActive(true);
        arPanel.GetComponentInChildren<Button>().onClick.RemoveListener(() => OnTargetOffObject(false));
        swapCamButton.GetComponent<Button>().onClick.RemoveListener(() => SwapCamera());
        replayButton.GetComponent<Button>().onClick.RemoveListener(() => ReplayPhonics());

        Button[] recordToastButton = recordToastPanel.GetComponentsInChildren<Button>();
        foreach (Button btn in recordToastButton)
        {
            btn.onClick.RemoveListener(() => RecordToastController(btn));
        }
    }
    #endregion


    #region ENABLE_and_UPDATE

    private void OnEnable()
    {
        arCamera.enabled = true;
    }

    private void OnDisable()
    {
        arCamera.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitPanel.SetActive(!exitPanel.activeSelf);
        }
    }

    #endregion


    //Splash → LoadingPanel → Loading Finish
    public void OnLoadingDone()
    {
        //loadingPanel.SetActive(false);
        Destroy(loadingPanel);
        PanelManager(false);
    }

    public void StartLanguage()
    {
        I2.Loc.LocalizationManager.CurrentLanguage = ui_CurrentLang;

        SetLocalizeUIString();

        localizeUIPanel.SetActive(!isStartLangPanel);
        localizeUIPanel.GetComponentInChildren<Toggle>().isOn = isStartLangPanel;
    }

    void StartPanelSetting()
    {
        accountManager = FindObjectOfType<AccountManager>();
        prefabLoader = FindObjectOfType<PrefabLoader>();
        prefabShelter = FindObjectOfType<PrefabShelter>();
        checkCode = FindObjectOfType<CheckCode>();
        aDSL = FindObjectOfType<AnimalDataSetLoader>();

        arManager = ARManager.Instance;

        arPanel.SetActive(false);
        localizeUIPanel.SetActive(false);
        localizePhonicsPanel.SetActive(false);
        bookPanel.SetActive(false);

        recordToastPanel.SetActive(false);
        exitPanel.SetActive(false);
        loadingPanel.SetActive(true);
        txt_version.text = txt_version_main.text = string.Format("v.{0}", Application.version);
    }

    #region BUTTON_ACTIONS

    //mainPanel buttons
    private void OnClick(Button temp)
    {
        switch (temp.name)
        {
            case "btn_Local":
                LocalizeButton(localizeUIPanel);

                break;
            case "btn_Account":
                accountManager.SerialCheck();

                break;

            case "StartScan":
                Manager.isMR = false;
                PanelManager(true);

                break;
            case "HowToUse":
                usePanel.SetActive(true);
                usePanel.GetComponentInChildren<Scrollbar>().value = 1;

                BottomBarOpen();

                break;
            case "Coloring":
                LoadSceneManager.instance.ChangeScene(true, false);

                break;
            case "btn_down":
                bookPanel.SetActive(true);
                bookPanel.GetComponentInChildren<PanelMovingController>().TouchOn();

                break;
            case "MRGallery":
                Manager.isMR = true;
                mrPanel.SetActive(true);
                BottomBarOpen();
                break;
        }
    }

    //Bottom Bar Button Action
    private void BottomPanelButton(Button temp)
    {
        switch (temp.transform.name)
        {
            case "btn_Home":
                if (mainUI.activeSelf)
                    accountManager.AllDisable();

                PanelManager(false);

                break;
            case "btn_Scan":
                if (mainUI.activeSelf)
                {
                    accountManager.AllDisable();
                    PanelManager(true);
                }
                else
                {
                    if (Manager.isMR)
                    {
                        Manager.isMR = false;
                        PanelManager(false);
                        PanelManager(true);
                    }
                    else
                    {
                        ChoiceControll();
                    }
                }
                break;
        }
    }

    //Phonics Replay Method
    private void ReplayPhonics()
    {
        PushedButtonReset();
        LocalPanelInitialSetting(LocalizationManager.CurrentLanguage);
        localizePhonicsImage.sprite =
            Resources.Load<Sprite>(string.Format("Sprites/Localize/btn_language_{0}(70x70)", LocalizationManager.CurrentLanguage));

        phonics.PlayPhonics();
    }

    //Front Camera - Back Camera Swap Method
    private void SwapCamera()
    {
        arManager.UseFrontCamera(!isFrontCam);
        if (prefabLoader.isTargetoff)
            prefabLoader.ModelChangePos();

        isFrontCam = !isFrontCam;
    }

    //RecordToast PopUpPanel
    private void RecordToastController(Button btn)
    {
        switch (btn.name)
        {
            case "btn_Record":
                phonics.RecordController();
                recordToastPanel.SetActive(false);

                break;
            case "btn_Cancel":
                recordToastPanel.SetActive(false);

                break;
        }
    }

    //LocalizeUI Toggle Controller
    private void LocalizeUiValueChanged(bool on)
    {
        isStartLangPanel = on;
        checkCode.SaveCurrentUiLanguage();
    }

    //Exit!!
    private void ExitControll(Button button)
    {
        switch (button.transform.name)
        {
            case "btn_Exit":
                Application.Quit();
                break;

            case "btn_Cancel":
                exitPanel.SetActive(false);
                break;
        }
    }
    #endregion //BUTTON_ACTIONS


    #region LOCALIZATION_METHOD

    //16Language Panel Button Reset
    private void PushedButtonReset()
    {
        if (pushedBtn != null)
        {
            if (pushedBtn.name.Equals("custom"))
            {
                pushedBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Localize/btn_custom(632x82)");
            }
            else
            {
                pushedBtn.transform.GetChild(1).GetComponent<Image>().sprite =
                    Resources.Load<Sprite>(string.Format("Sprites/Localize/btn_{0}(302x82)", pushedBtn.name));
            }
            pushedBtn = null;
        }
    }

    //16Language Panel Setting when to start Phonics
    public void LocalPanelInitialSetting(string nowLang)
    {
        for (int i = 0; i < localizePhonicsButton.Length; i++)
        {
            if (localizePhonicsButton[i].name.Equals(nowLang))
            {
                if (nowLang.Equals("custom"))
                {
                    PushedButtonReset();

                    Image btnImg = localizePhonicsButton[i].GetComponent<Image>();
                    btnImg.sprite = Resources.Load<Sprite>("Sprites/Localize/btn_custom_on(632x82)");
                    pushedBtn = localizePhonicsButton[i];
                    break;
                }
                else
                {
                    Image btnImg = localizePhonicsButton[i].transform.GetChild(1).GetComponent<Image>();
                    btnImg.sprite = Resources.Load<Sprite>(string.Format("Sprites/Localize/btn_{0}_on(302x82)", nowLang));
                    pushedBtn = localizePhonicsButton[i];
                    break;
                }
            }
        }
    }

    //Localization UI & Phonics Panel On-Off
    private void LocalizeButton(GameObject panel)
    {
        bool isLocalPanel = panel.activeSelf;
        panel.SetActive(!isLocalPanel);
        isToastOn = true;
    }

    //Localization Button Controller
    private void LocalizeUI(Button temp)
    {
        isToastOn = false;

        if (isPhonics && !temp.name.Equals("BGButton") && !temp.name.Equals("custom"))
        {
            PushedButtonReset();
            temp.transform.GetChild(1).GetComponent<Image>().sprite =
                Resources.Load<Sprite>(string.Format("Sprites/Localize/btn_{0}_on(302x82)", temp.name));
            pushedBtn = temp;
        }

        if (temp.name.Equals("BGButton"))
        {
            temp.transform.parent.gameObject.SetActive(false);
        }
        else if (temp.name.Equals("custom"))
        {
            localizePhonicsPanel.SetActive(false);

            if (phonics.recordExist)
            {
                PushedButtonReset();
                temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Localize/btn_custom_on(632x82)");
                pushedBtn = temp;

                phonics.PlayRecPhonics();
                localizePhonicsImage.sprite = Resources.Load<Sprite>("Sprites/Localize/btn_language_custom(70x70)");
            }
            else
            {
                recordToastPanel.SetActive(true);
            }
        }
        else
        {
            LocalizationManager.CurrentLanguage = temp.name;

            if (isTitle) //UI_Localize 적용
            {
                SetLocalizeUIString();
                localizeUIPanel.SetActive(false);
            }
            else // Phonics_Localize 적용
            {
                StartCoroutine(phonics.ChangeText(false));
                localizePhonicsPanel.SetActive(false);
            }
            localizePhonicsImage.sprite = Resources.Load<Sprite>(string.Format("Sprites/Localize/btn_language_{0}(70x70)", LocalizationManager.CurrentLanguage));
        }
    }

    //UI Localization에 사용될 String값 저장
    private void SetLocalizeUIString()
    {
        ui_CurrentLang = LocalizationManager.CurrentLanguage;
        localizeUiImage.sprite = Resources.Load<Sprite>(string.Format("Sprites/Localize/btn_ui_{0}(70x70)", ui_CurrentLang));
        localizeFont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
        botFontSize = Convert.ToInt32(LocalizationManager.GetTermTranslation("UI_botFontSize"));

        //CanvasManager (개)
        scanString = LocalizationManager.GetTermTranslation("UI_scan");
        scanSingleString = LocalizationManager.GetTermTranslation("UI_scanSingle");
        scanMultiString = LocalizationManager.GetTermTranslation("UI_scanMulti");
        btnSingleString = LocalizationManager.GetTermTranslation("UI_single");
        btnMultiString = LocalizationManager.GetTermTranslation("UI_multi");

        //Phonics
        phoSentenceString = LocalizationManager.GetTermTranslation("UI_sentence");
        phoWordString = LocalizationManager.GetTermTranslation("UI_word");

        ChangeLocalizeUI();
        checkCode.SaveCurrentUiLanguage();
    }

    //UI Localization Setting
    private void ChangeLocalizeUI()
    {
        btnBotScanButton.font = localizeFont;
        btnBotScanButton.fontSize = botFontSize;
        scanTitle.GetComponent<Text>().font = localizeFont;

        btnMRGallery.text = LocalizationManager.GetTermTranslation("UI_MRGallery");
        btnMRGallery.font = localizeFont;

        btnStartScan.text = scanString;
        btnStartScan.font = localizeFont;

        btnHowtouse.text = LocalizationManager.GetTermTranslation("UI_HowtoUse");
        btnHowtouse.font = localizeFont;

        btnColoring.text = LocalizationManager.GetTermTranslation("UI_coloring");
        btnColoring.font = localizeFont;

        localUITitle.text = LocalizationManager.GetTermTranslation("UI_LocalUITitle");
        localUITitle.font = localizeFont;

        localUiToggle.text = LocalizationManager.GetTermTranslation("UI_LocalUIToggleText");
        localUiToggle.font = localizeFont;

        Text[] exitText = exitPanel.GetComponentsInChildren<Text>();
        for (int i = 0; i < exitText.Length; i++)
        {
            Text txt = exitText[i];
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = localizeFont;
        }

        Text[] recordToasText = recordToastPanel.GetComponentsInChildren<Text>();
        for (int i = 0; i < recordToasText.Length; i++)
        {
            Text txt = recordToasText[i];
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = localizeFont;
        }
    }
    #endregion //LOCALIZATION_METHOD


    //무료페이지가 아닌 타겟 인식하면 QR안내팝업
    internal void OnInfoSerial(bool isDown)
    {
        if (isNotToastAgain)
        {
            StopCoroutine(TextCleanWait());

            if (isDown)
                arPanel.transform.GetChild(2).GetComponent<Text>().text = LocalizationManager.GetTermTranslation("UI_needQR");
            else
                arPanel.transform.GetChild(2).GetComponent<Text>().text = LocalizationManager.GetTermTranslation("UI_needDown");

            arPanel.transform.GetChild(2).GetComponent<Text>().font = localizeFont;
            StartCoroutine(TextCleanWait());
        }
        else
        {
            arManager.ActivateDataSet(false);
            qrDownPanel.GetComponent<QRnDownPopUpController>().isQR = isDown;
            qrDownPanel.SetActive(true);
        }
    }

    private IEnumerator TextCleanWait()
    {
        yield return new WaitForSeconds(1);
        arPanel.transform.GetChild(2).GetComponent<Text>().text = string.Empty;

        yield break;
    }

    //현재 이용가능 콘텐츠 표시(하단 텍스트로)
    public void FreeContentNotice()
    {
        string bookList = string.Empty;

        for (int i = 0; i < checkCode.codeConfirms.Count; i++)
        {
            string codeNum = checkCode.codeConfirms[i].Remove(0, 2);

            bookList += string.Format(" {0}", codeNum);
        }

        if (bookList.Equals(string.Empty))
        {
            arPanel.transform.GetChild(2).GetComponent<Text>().text =
              LocalizationManager.GetTermTranslation("UI_usableFreeOnly");
        }
        else
        {
            arPanel.transform.GetChild(2).GetComponent<Text>().text =
                LocalizationManager.GetTermTranslation("UI_usableContent").Replace("*", bookList);
        }
        arPanel.transform.GetChild(2).GetComponent<Text>().font = localizeFont;
    }


    //Account, AboutUs, HowtoUse 패널 오픈셋팅
    public void BottomBarOpen()
    {
        bottomPanel.SetActive(true);
        bottomPanel.transform.GetChild(1).gameObject.SetActive(true);
        bottomPanel.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_scan(325x95)");
        btnBotScanButton.text = scanString;
    }


    //Home버튼, Scan버튼
    public void PanelManager(bool panel) //true: 스캔페이지로, false: 홈으로
    {
        arCamera.enabled = true;
        arManager.TurnOnAR(panel, false);
        if (isPhonics)
        {
            OnPhonicsPanel(false);
            LocalizationManager.CurrentLanguage = ui_CurrentLang;
        }

        if (panel)
        {
            isTitle = false;
            prefabLoader.isEndAR = false;
            ScanOn();
            arPanel.transform.GetChild(3).gameObject.SetActive(false);

            arManager.ActivateDataSet(true);
            arManager.hintState = ARManager.HintState.SINGLE;
            ChoiceControll();
            localizePhonicsImage.sprite = Resources.Load<Sprite>(string.Format("Sprites/Localize/btn_language_{0}(70x70)", ui_CurrentLang));

            if (!isNotCautionAgain)
            {
                Instantiate(Resources.Load<GameObject>("Prefabs/cautionPanel"), arPanel.transform, false);
                CautionActive(true);
            }

            FreeContentNotice();
        }
        else
        {
            arManager.setHintZero();
            DestroyAll();
            prefabLoader.isEndAR = true;
            LocalizationManager.CurrentLanguage = ui_CurrentLang;

            if (prefabLoader.isTargetoff)
                OnTargetOffObject(false);
            arPanel.SetActive(false);
            qrDownPanel.SetActive(false);
            accountManager.accountPanel.SetActive(false);

            //ImageTargetBehavior inactive
            //여기 4->5
            for (int i = 0; i < 5; i++)
            {
                aDSL.transform.GetChild(i).GetComponent<DataSetOnOff>().TrackingActiveOn(false);
            }

            isTitle = true;
            if (Manager.PrefabShelter.transform.parent.GetChild(1).GetChild(0).childCount > 2)
            {
                Destroy(Manager.PrefabShelter.transform.parent.GetChild(1).GetChild(0).GetChild(2).gameObject);
            }
        }

        bottomPanel.SetActive(panel);
        mainUI.SetActive(!panel);
        usePanel.SetActive(false);
        aboutUsPanel.SetActive(false);
        bookPanel.SetActive(false);
        swapCamButton.SetActive(panel); //Camera Swap Button


    }


    //Single or Multi or Zero
    public void ChoiceControll()
    {
        arPanel.SetActive(true);
        OnTrackingFound(true);
        if (prefabLoader.isTargetoff)
            OnTargetOffObject(false);

        if (arManager.hintState == ARManager.HintState.MULTI)
            isSingleTarget = true;
        else if (arManager.hintState == ARManager.HintState.SINGLE)
            isSingleTarget = false;
        else if (arManager.hintState == ARManager.HintState.ZERO)
            OnPhonicsPanel(false);

        OnSingleTarget(isSingleTarget);
    }

    //Scan Frame 변경     < True:Single, False:Multi >
    private void OnSingleTarget(bool on)
    {
        bottomPanel.transform.GetChild(1).gameObject.SetActive(true);

        if (on)
        {
            arManager.setHintSingle();
            scanTitle.GetComponent<Text>().text = scanSingleString;

            btnBotScanButton.text = btnMultiString;
            bottomPanel.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_multiple(325x95)");
        }
        else
        {
            arManager.setHintMulti();
            scanTitle.GetComponent<Text>().text = scanMultiString;

            btnBotScanButton.text = btnSingleString;
            bottomPanel.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_single(325x95)");
        }
    }

    public void OnPhonicsPanel(bool sender)
    {
        swapCamButton.SetActive(!sender); //Camera Swap Button
        if (sender)
        {
            bottomPanel.GetComponentInChildren<Text>().text = scanString;
            bottomPanel.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_scan(325x95)");
            //로컬버튼아이콘
            localizePhonicsImage.sprite = Resources.Load<Sprite>(string.Format("Sprites/Localize/btn_language_{0}(70x70)", ui_CurrentLang));
            isPhonics = true;
        }
        else
        {
            //Phonics Page (videoPanel, object, Prefab) 파괴, Camera → ARCamera, 타겟수 → single
            prefabLoader.DestroyPrefab();
            prefabLoader.isEndAR = false;
            arManager.ChangeCamera("ARCamera");
            OnSingleTarget(isSingleTarget);
            PushedButtonReset();

            if (isFrontCam)
            {
                arManager.UseFrontCamera(isFrontCam);
            }
            isPhonics = false;

            FreeContentNotice();
        }
        replayButton.SetActive(sender);   //Replay Button
        scanTitle.SetActive(!sender);     //스캔하기 타이틀 Text
        arPanel.transform.GetChild(1).gameObject.SetActive(!sender);   //스캔하기 프레임
        arPanel.transform.GetChild(3).gameObject.SetActive(false);    //타겟오프때 Close버튼
        localizePopupButton.SetActive(sender); //localization Button
        arPanel.transform.GetChild(1).gameObject.SetActive(!sender); //Scan View
    }

    private void DestroyAll()
    {
        StopAllCoroutines();
        if (GameObject.FindGameObjectsWithTag("augmentation") != null)
        {
            GameObject[] got = GameObject.FindGameObjectsWithTag("augmentation");
            for (int i = 0; i < got.Length; i++)
                Destroy(got[i]);

        }
        if (GameObject.FindGameObjectsWithTag("Phonics") != null)
        {
            GameObject[] phgo = GameObject.FindGameObjectsWithTag("Phonics");
            for (int i = 0; i < phgo.Length; i++)
                Destroy(phgo[i]);
        }
    }

    //QR등록된 타겟만 스캔ON
    public void ScanOn()
    {
        for (int j = 0; j < checkCode.codeConfirms.Count; j++)
        {
            int bNum = Convert.ToInt32(checkCode.codeConfirms[j].Substring(checkCode.codeConfirms[j].Length - 1));
            for (int i = (bNum - 1) * 100; i < bNum * 100; i++)
            {
                if (prefabShelter.tmModel[i] != null)
                    prefabShelter.tmModel[i].isConfirm = true;
            }

            //ImageTargetBehavior Active: QR등록된 타겟만
            aDSL.transform.GetChild(bNum - 1).GetComponent<DataSetOnOff>().TrackingActiveOn(true);
        }
    }


    public void OnTrackingFound(bool on)
    {
        scanTitle.SetActive(on);

        if (!on)
            arPanel.transform.GetChild(2).GetComponent<Text>().text = string.Empty;
    }

    //오브젝트 터치하면(true) AR카메라 Stop! CloseButton On!    Close버튼누르면(false) AR카메라 Start!
    public void OnTargetOffObject(bool on)
    {
        Light arLight = arCamera.GetComponentInChildren<Light>();
        arManager.ActivateDataSet(!on);

        arPanel.transform.GetChild(3).gameObject.SetActive(on);
        arPanel.transform.GetChild(1).gameObject.SetActive(!on);
        scanTitle.SetActive(!on);

        if (isCoverTarget && !on)
        {
            Destroy(coverVideo);
            coverVideo = null;
            arLight.intensity = 1;

            prefabLoader.isTargetoff = false;
            isCoverTarget = false;
        }
        else if (isCoverTarget && on)
        {

            if(!Manager.isMR)coverVideo = Instantiate(Resources.Load<GameObject>("prefabs/coverAR"), Camera.main.transform, false);
            arLight.intensity = 0.65f;
        }
        else if (!isCoverTarget && !on)
        {
            prefabLoader.DestroyObj();
        }
    }

    public void CautionActive(bool on)
    {
        arManager.ActivateDataSet(!on);

        arPanel.transform.GetChild(0).gameObject.SetActive(!on);
        arPanel.transform.GetChild(1).gameObject.SetActive(!on);
    }
}
