using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TagLib;
using UnityEditor;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.Audio;
using Mono.Data.Sqlite;
using System.Data;
using System.Net;
using System.Net.Http;
using SimpleFileBrowser;

public class Play : MonoBehaviour
{
    bool playing = false;
    bool clicked = false;
    bool ended = false;
    public AudioSource reproductor;
    public Slider slider;
    public Image imagePlay;
    public Sprite play;
    public Sprite pause;
    public Sprite list;
    public TMP_Text current;
    public TMP_Text maximum;
    public Button loop;
    public Button random;
    bool rand =false;
    public Button more;
    public TMP_Dropdown dropdown;
    public TMP_Dropdown orders;
    public TMP_Dropdown groups;
    public Image albumCover;
    public Text title;
    public Button forward;
    public Button back;
    public int index = 0;
    private List<Song> songs = new List<Song>();
    private List<Album> albums = new List<Album>();
    private List<Artist> artists = new List<Artist>();
    private List<Group> dates = new List<Group>();
    private List<Group> lists = new List<Group>();
    private List<Song> results;
    public GameObject itemList;
    public Transform content;
    const int contentHeight=160;
    const int groupHeight = 300;
    const int listHeight = 80;
    public Button hideList;
    public Image hide;
    Vector3 position;
    GameObject scrollView;
    public HorizontalLayoutGroup listaImagen;
    public Button search;
    public InputField searchField;
    public GameObject result;
    public GameObject popup;
    public Transform contentResult;
    public Button restore;
    public GameObject editPanel;
    public Button edit;
    public Image imageEdit;
    public InputField titleEdit;
    public InputField dateEdit;
    public InputField artistEdit;
    public InputField albumEdit;
    public GameObject downloadPanel;
    public InputField downloadField;
    public Button download;
    public Transform contentList;
    public Button createList;
    public Button deleteList;
    public GameObject listPanel;
    public Button listButton;
    public GameObject ListExistPanel;
    public GameObject createListPanel;
    public Button createListButton;
    public Transform contentListExist;
    public InputField createField;
    public GameObject listExistObject;
    public GameObject listDelete;
    public GameObject listDeletePanel;
    public Transform contentDelete;
    public Button configButton;
    public GameObject configPanel;
    public Button saveConfig;
    public Button defaultConfig;
    public GameObject mainPanel;
    public Image backgroundImage;
    public Image color1, color2, color3, color4;
    public Slider opacity;
    public Slider volume, pitch, reverb;
    public Button selectImage;
    string backgroundPath;
    public GameObject emptyPanel;
    public Button emptyAdd;
    public GameObject toast;
    public Text txt;
    Database db;



    //Dudas: 
    //-cambios visuales

    public AudioMixer mixer;

    // Start is called before the first frame update
    void Start()
    {
        //mixer.SetFloat("volume", 0);
        //mixer.GetFloat("volume");
        try
        {
            FileBrowser.RequestPermission();
            if (Application.platform == RuntimePlatform.Android)
            {
                Screen.SetResolution(1920, 1080, false);
            }
            db = new Database();
            createTables();
            scrollView = GameObject.FindGameObjectWithTag("Scrollview");
            position = hideList.transform.position;
            dropdown.transform.SetAsFirstSibling();
            slider.maxValue = reproductor.clip.length;
            stringCounts();
            startList();
            defaultList();
            slider.onValueChanged.AddListener(changeTime);
            loop.onClick.AddListener(onLoopClick);
            random.onClick.AddListener(onRandomClick);
            dropdown.onValueChanged.AddListener(menuValue);
            orders.onValueChanged.AddListener(ordersValue);
            groups.onValueChanged.AddListener(groupsValue);
            back.onClick.AddListener(onBackClick);
            forward.onClick.AddListener(onForwardClick);
            hideList.onClick.AddListener(onHideClick);
            more.onClick.AddListener(onMoreClick);
            search.onClick.AddListener(onSearchClick);
            download.onClick.AddListener(onDownloadClick);
            edit.onClick.AddListener(onEditClick);
            createList.onClick.AddListener(onCreateList);
            deleteList.onClick.AddListener(onDeleteList);
            listButton.onClick.AddListener(onListClick);
            createListButton.onClick.AddListener(onCreateCommit);
            saveConfig.onClick.AddListener(onSaveConfig);
            defaultConfig.onClick.AddListener(onDefaultConfig);
            selectImage.onClick.AddListener(onImageSelect);
            opacity.onValueChanged.AddListener(onOpacityChange);
            volume.onValueChanged.AddListener(onVolumeChange);
            pitch.onValueChanged.AddListener(onPitchChange);
            reverb.onValueChanged.AddListener(onReverbChange);
            color1.GetComponent<Button>().onClick.AddListener(onColor1);
            color2.GetComponent<Button>().onClick.AddListener(onColor2);
            color3.GetComponent<Button>().onClick.AddListener(onColor3);
            color4.GetComponent<Button>().onClick.AddListener(onColor4);
            emptyAdd.onClick.AddListener(addSongs);
            loadConfig();
        } catch(Exception e)
        {
            showToast(e.ToString(), 100);
        }
    }

    public void createTables()
    {
        db.execCreate("CREATE TABLE IF NOT EXISTS ARTISTA("+"ID_ARTISTA VARCHAR(50) PRIMARY KEY, Nome_Artista VARCHAR(150))");
        db.execCreate("CREATE TABLE IF NOT EXISTS ALBUM ( " + "ID_ALBUM VARCHAR(50) PRIMARY KEY, Nome_Album VARCHAR(150))");
        db.execCreate("CREATE TABLE IF NOT EXISTS CANCION ( " + "ID_CANCION VARCHAR(50) PRIMARY KEY, Titulo VARCHAR(150), Data VARCHAR(150), Duracion INTEGER(10), Ruta VARCHAR(150), " +
            "IdArtista VARCHAR(50) DEFAULT 'Desconocido', IdAlbum VARCHAR(50) DEFAULT 'Desconocido', FOREIGN KEY (IdArtista) REFERENCES ARTISTA(ID_ARTISTA) ON UPDATE CASCADE ON DELETE SET DEFAULT," +
            "FOREIGN KEY (IdAlbum) REFERENCES ALBUM(ID_ALBUM) ON UPDATE CASCADE ON DELETE SET DEFAULT)");
        db.execCreate("CREATE TABLE IF NOT EXISTS LISTA (" + "ID_LISTA VARCHAR(50) PRIMARY KEY, Nome_Lista VARCHAR(150))");
        db.execCreate("CREATE TABLE IF NOT EXISTS LISTACANCION (" + "IdLista VARCHAR(50), IdCancion VARCHAR(50), PRIMARY KEY(IdLista,IdCancion), FOREIGN KEY (IdLista) REFERENCES LISTA(ID_LISTA)" +
            "ON UPDATE CASCADE ON DELETE CASCADE, FOREIGN KEY (IdCancion) REFERENCES CANCION(ID_CANCION))");
    }

    public void startList()
    {
        emptyPanel.SetActive(true);
        songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION");
        albums = db.readAlbums("SELECT ID_ALBUM,Nome_Album FROM ALBUM");
        artists = db.readArtists("SELECT ID_ARTISTA,Nome_Artista FROM ARTISTA");
        if(songs.Count != 0)
        {
            emptyPanel.SetActive(false);
            playFirst();
            print(songs.Count);
            showToast(songs.Count+" songs in Database",3);
            fillList();
        }
    }

    public void playFirst()
    {
        index = 0;
        title.text = getArtist(index) + " - " + songs[index].title;
        albumCover.sprite = getPic(songs[index].path);
        _ = updateAsync();
    }

    public void defaultList()
    {
        string name_list = "Default";
        string listid = randomID();

        if (!db.readName("SELECT ID_LISTA FROM LISTA WHERE Nome_Lista='" + name_list + "'"))
            db.execInsert("INSERT INTO LISTA (ID_LISTA,Nome_Lista) VALUES ('" + listid + "','" + name_list + "')");
        else
            listid = db.readField("SELECT ID_LISTA FROM LISTA WHERE Nome_Lista='" + name_list + "'");

        foreach(Song song in songs)
        {
            db.execInsert("INSERT OR IGNORE INTO LISTACANCION (IdLista,IdCancion) VALUES ('" + listid + "','" + song.id_song + "')");
        }
    }

    //Update is called once per frame
    void Update()
    {

        if (reproductor.time >= reproductor.clip.length)
        {
            ended = true;
        }
        
    }

    IEnumerator updateSong()
    {
        yield return new WaitForSeconds(0.5f);
        while (reproductor.isPlaying || playing)
        {
            yield return new WaitForSeconds(0.25f);
            slider.value = reproductor.time;
            if (clicked)
            {
                clicked = false;
            }
            else
            {
                yield return new WaitForSeconds(0.01f);
                if (ended && index < songs.Count - 1)
                {
                    yield return new WaitForSeconds(0.01f);
                    index++;
                    title.text = getArtist(index) + " - " + songs[index].title;
                    albumCover.sprite = getPic(songs[index].path);
                    _ = updateAsync();
                }
                else if(ended && index >= songs.Count - 1)
                {
                    reproductor.Stop();
                    playing = false;
                    imagePlay.overrideSprite = play;
                }
            }
        }
    }

    public void firstImage()
    {
        #if UNITY_EDITOR
        string assetPath = AssetDatabase.GetAssetPath(reproductor.clip.GetInstanceID());
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        filePath = filePath.Replace("/", "\\");
        print(filePath);
        var tfile = TagLib.File.Create(filePath);
        title.text = tfile.Tag.Performers[0] + " - " + tfile.Tag.Title;

        TagLib.IPicture pic = tfile.Tag.Pictures[0];
        MemoryStream ms = new MemoryStream(pic.Data.Data);
        ms.Seek(0, SeekOrigin.Begin);

        //Create texture2d with MemoryStream 
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(ms.ToArray());

        Sprite imagen = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        /// assign to a RawImage
        albumCover.sprite = imagen;
        #endif
    }

    public void isPlaying()
    {
        if (reproductor.isPlaying)
        {
            reproductor.Pause();
            playing = false;
            imagePlay.overrideSprite = play;
        }
        else
        {
            reproductor.Play();
            playing = true;
            ended = false;
            imagePlay.overrideSprite = pause;
            StartCoroutine(updateSong());
        }
    }

    public void stringCounts()
    {
        string minutes = "" + (int)reproductor.clip.length / 60;
        string seconds = "" + (int)reproductor.clip.length % 60;
        if (minutes.Length < 2)
            minutes = "0" + minutes;
        if (seconds.Length < 2)
            seconds = "0" + seconds;
        maximum.text = "" + minutes + ":" + seconds;
        current.text = "00:00";
    }
    public void changeTime(float time)
    {
        reproductor.time = time;
        string minutes = "" + (int)time / 60;
        string seconds = "" + (int)time % 60;
        if (minutes.Length < 2)
            minutes = "0" + minutes;
        if (seconds.Length < 2)
            seconds = "0" + seconds;
        current.text = "" + minutes + ":" + seconds;
    }

    void showToast(string text,
    int duration)
    {
        toast.SetActive(true);
        StartCoroutine(showToastCOR(text, duration));
    }

    private IEnumerator showToastCOR(string text,
    int duration)
    {
        Color orginalColor = txt.color;

        txt.text = text;
        txt.enabled = true;

        //Fade in
        yield return fadeInAndOut(txt, true, 0.5f);

        //Wait for the duration
        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            yield return null;
        }

        //Fade out
        yield return fadeInAndOut(txt, false, 0.5f);

        txt.enabled = false;
        toast.SetActive(false);
        txt.color = orginalColor;
    }

    IEnumerator fadeInAndOut(Text targetText, bool fadeIn, float duration)
    {
        //Set Values depending on if fadeIn or fadeOut
        float a, b;
        if (fadeIn)
        {
            a = 0f;
            b = 1f;
        }
        else
        {
            a = 1f;
            b = 0f;
        }

        Color currentColor = Color.clear;
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(a, b, counter / duration);

            targetText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            toast.GetComponent<Image>().color = new Color(toast.GetComponent<Image>().color.r, toast.GetComponent<Image>().color.g, toast.GetComponent<Image>().color.b, alpha);
            yield return null;
        }
    }

    public void onMoreClick()
    {
        dropdown.Show();
    }

    public void onSearchClick()
    {
        if(!searchField.gameObject.activeSelf)
            searchField.gameObject.SetActive(true);
        else
            searchField.gameObject.SetActive(false);
    }

    public void searchSong()
    {
        string query = searchField.text;
        songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION WHERE Titulo LIKE '%" + query + "%' OR Data LIKE '%" + query + "%' OR" +
                " (SELECT Nome_Artista FROM ARTISTA WHERE ID_ARTISTA=IdArtista) LIKE '%" + query + "%' OR (SELECT Nome_Album FROM ALBUM WHERE ID_ALBUM=IdAlbum) LIKE '%" + query + "%'");
        emptyList("ListItem");
        if (songs.Count != 0)
        {
            fillList();
            playFirst();
        }
    }

    public void onSaveConfig()
    {
        PlayerPrefs.SetInt("saved", 1);
        PlayerPrefs.SetFloat("opacity", opacity.value);
        PlayerPrefs.SetFloat("volumen", volume.value);
        PlayerPrefs.SetFloat("pitch", pitch.value);
        PlayerPrefs.SetFloat("reverb", reverb.value);
        PlayerPrefs.SetFloat("r", mainPanel.GetComponent<Image>().color.r);
        PlayerPrefs.SetFloat("g", mainPanel.GetComponent<Image>().color.g);
        PlayerPrefs.SetFloat("b", mainPanel.GetComponent<Image>().color.b);
        PlayerPrefs.SetFloat("a", mainPanel.GetComponent<Image>().color.a);
        PlayerPrefs.SetString("path", backgroundPath);
        configPanel.SetActive(false);
    }

    public void loadConfig()
    {
        if (PlayerPrefs.GetInt("saved") == 1)
        {
            opacity.value = PlayerPrefs.GetFloat("opacity");
            volume.value = PlayerPrefs.GetFloat("volumen");
            pitch.value = PlayerPrefs.GetFloat("pitch");
            reverb.value = PlayerPrefs.GetFloat("reverb");
            Color temp = mainPanel.GetComponent<Image>().color;
            temp.r = PlayerPrefs.GetFloat("r");
            temp.g = PlayerPrefs.GetFloat("g");
            temp.b = PlayerPrefs.GetFloat("b");
            temp.a = PlayerPrefs.GetFloat("a");
            mainPanel.GetComponent<Image>().color = temp;
            backgroundImage.sprite = getBackground(PlayerPrefs.GetString("path"));
        }
    }

    public void onDefaultConfig()
    {
        opacity.value = 0.3921569f;
        Color temp = Color.black;
        temp.a = opacity.value;
        mainPanel.GetComponent<Image>().color = temp;
        backgroundImage.sprite = null;
        volume.value = 1;
        pitch.value = 1;
        reverb.value = 1;
    }

    public void onImageSelect()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png",".jpeg"));
        FileBrowser.SetDefaultFilter(".jpg");
        FileBrowser.ShowLoadDialog((paths) => { backgroundPath = paths[0]; backgroundImage.sprite = getBackground(backgroundPath); }, () => { print("Canceled"); },
                                   FileBrowser.PickMode.Files, false, null, null, "Select Picture", "Select");
        
    }

    public void onOpacityChange(float value)
    {
        Color temp = mainPanel.GetComponent<Image>().color;
        temp.a = value;
        mainPanel.GetComponent<Image>().color = temp;
    }

    public void onVolumeChange(float value)
    {
        reproductor.volume = value;
    }

    public void onPitchChange(float value)
    {
        reproductor.pitch = value;
    }

    public void onReverbChange(float value)
    {
        reproductor.reverbZoneMix = value;
    }

    public void onColor1()
    {
        Color temp = color1.color;
        temp.a = opacity.value;
        mainPanel.GetComponent<Image>().color = temp;
    }

    public void onColor2()
    {
        Color temp = color2.color;
        temp.a = opacity.value;
        mainPanel.GetComponent<Image>().color = temp;
    }

    public void onColor3()
    {
        Color temp = color3.color;
        temp.a = opacity.value;
        mainPanel.GetComponent<Image>().color = temp;
    }

    public void onColor4()
    {
        Color temp = color4.color;
        temp.a = opacity.value;
        mainPanel.GetComponent<Image>().color = temp;
    }

    public void onListClick()
    {
        listPanel.SetActive(true);
        emptyList("SearchItem");
        listPanel.SetActive(false);
        fillGroupsList();
    }

    public void onCreateList()
    {
        createListPanel.SetActive(true);
    }

    public void onCreateCommit()
    {
        if (!db.readName("SELECT ID_LISTA FROM LISTA WHERE Nome_Lista='" + createField.text + "'"))
        {
            db.execInsert("INSERT INTO LISTA (ID_LISTA,Nome_Lista) VALUES ('" + randomID() + "','" + createField.text + "')");
            emptyList("SearchItem");
            fillGroupsList();
        }
    }

    public void onDeleteList()
    {
        listDeletePanel.SetActive(true);
        emptyList("ListDelete");
        listDeletePanel.SetActive(false);
        lists = db.readList("SELECT ID_LISTA, Nome_Lista FROM LISTA");
        for (int i = 0; i < lists.Count; i++)
        {
            listDeletePanel.SetActive(true);
            GameObject cloneItem = Instantiate(listDelete);
            cloneItem.GetComponent<Button>().onClick.AddListener(delegate { onItemClickListDelete(cloneItem); });
            cloneItem.GetComponent<Text>().text = " " + i;
            cloneItem.transform.GetChild(0).GetComponent<Text>().text = lists[i].count;
            cloneItem.transform.SetParent(contentDelete);
            cloneItem.transform.localScale = new Vector3(1, 1, 1);
            contentList.GetComponent<RectTransform>().sizeDelta = new Vector2(contentList.GetComponent<RectTransform>().sizeDelta.x, listHeight * lists.Count);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentList.GetComponent<RectTransform>());
        }
    }

    public void addSongtoList()
    {
        lists = db.readList("SELECT ID_LISTA, Nome_Lista FROM LISTA");
        for (int i = 0; i < lists.Count; i++)
        {
            ListExistPanel.SetActive(true);
            GameObject cloneItem = Instantiate(listExistObject);
            cloneItem.GetComponent<Text>().text = " " + i;
            Toggle toggleList = cloneItem.transform.GetChild(0).GetComponent<Toggle>();
            toggleList.isOn = false;
            if (db.readName("SELECT IdCancion FROM LISTACANCION WHERE IdLista='"+lists[i].group+"' AND IdCancion='"+songs[index].id_song+"'"))
                toggleList.isOn = true; 
            toggleList.onValueChanged.AddListener(delegate { onItemClickListSong(cloneItem); });
            toggleList.transform.GetChild(1).GetComponent<Text>().text = lists[i].count;
            cloneItem.transform.SetParent(contentListExist);
            cloneItem.transform.localScale = new Vector3(1, 1, 1);
            contentList.GetComponent<RectTransform>().sizeDelta = new Vector2(contentList.GetComponent<RectTransform>().sizeDelta.x, listHeight * lists.Count);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentList.GetComponent<RectTransform>());
        }
    }

    public void fillGroupsList()
    {
        lists = db.readList("SELECT ID_LISTA, Nome_Lista FROM LISTA");
        if (lists != null)
        {
            for (int j = 0; j < lists.Count; j++)
            {
                if (db.readName("SELECT IdCancion FROM LISTACANCION WHERE IdLista='" + lists[j].group + "'"))
                {
                    dates = db.readGroup("SELECT IdCancion,COUNT(*) FROM CANCION INNER JOIN LISTACANCION ON IdCancion=ID_CANCION WHERE IdLista='" + lists[j].group + "'");

                    if (dates.Count != 0)
                    {
                        for (int i = 0; i < dates.Count; i++)
                        {
                            listPanel.SetActive(true);
                            GameObject cloneItem = Instantiate(result);
                            cloneItem.GetComponent<Button>().onClick.AddListener(delegate { onItemClickList(cloneItem); });
                            cloneItem.GetComponent<Text>().text = " " + j;
                            cloneItem.transform.GetChild(0).GetComponent<Image>().sprite = getPic(db.readField("SELECT Ruta FROM CANCION WHERE ID_CANCION='" + dates[i].group + "'"));
                            cloneItem.transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
                            cloneItem.transform.GetChild(1).GetComponent<Text>().text = "" + dates[i].count;
                            cloneItem.transform.GetChild(2).GetComponent<Text>().text = "" + lists[j].count;
                            cloneItem.transform.SetParent(contentList);
                            cloneItem.transform.localScale = new Vector3(1, 1, 1);
                            int size = 80;
                            if (dates.Count % 5 != 0)
                                size = 300;
                            contentList.GetComponent<RectTransform>().sizeDelta = new Vector2(contentList.GetComponent<RectTransform>().sizeDelta.x, size + groupHeight * dates.Count / 5);
                            LayoutRebuilder.ForceRebuildLayoutImmediate(contentList.GetComponent<RectTransform>());
                        }
                    }
                }
            }
        }
        print(dates.Count);
    }

    public void fillGroupsArtist()
    {
        dates = db.readGroup("SELECT (Select Nome_Artista FROM ARTISTA WHERE ID_ARTISTA=IdArtista), COUNT(*), IdArtista FROM CANCION GROUP BY IdArtista");

        if (dates.Count != 0)
        {
            for (int i = 0; i < dates.Count; i++)
            {
                popup.SetActive(true);
                restore.gameObject.SetActive(true);
                GameObject cloneItem = Instantiate(result);
                cloneItem.GetComponent<Button>().onClick.AddListener(delegate { onItemClickArtist(cloneItem); });
                cloneItem.GetComponent<Text>().text = " "+i;
                cloneItem.transform.GetChild(0).GetComponent<Image>().sprite = getPic(db.readField("SELECT Ruta FROM CANCION WHERE IdArtista='"+db.readField("SELECT ID_ARTISTA FROM ARTISTA WHERE Nome_Artista='"+dates[i].group+"'")+"'"));
                cloneItem.transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
                cloneItem.transform.GetChild(1).GetComponent<Text>().text = ""+dates[i].count;
                cloneItem.transform.GetChild(2).GetComponent<Text>().text = ""+dates[i].group;
                cloneItem.transform.SetParent(contentResult);
                cloneItem.transform.localScale = new Vector3(1, 1, 1);
                int size = 30;
                if (dates.Count % 5 != 0)
                    size = 250;
                contentResult.GetComponent<RectTransform>().sizeDelta = new Vector2(contentResult.GetComponent<RectTransform>().sizeDelta.x, size + groupHeight * dates.Count / 5);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentResult.GetComponent<RectTransform>());
            }
        }
        print(dates.Count);
    }

    public void fillGroupsAlbum()
    {
        dates = db.readGroup("SELECT (Select Nome_Album FROM ALBUM WHERE ID_ALBUM=IdAlbum), COUNT(*), IdAlbum FROM CANCION GROUP BY IdAlbum");

        if (dates.Count != 0)
        {
            for (int i = 0; i < dates.Count; i++)
            {
                popup.SetActive(true);
                restore.gameObject.SetActive(true);
                GameObject cloneItem = Instantiate(result);
                cloneItem.GetComponent<Button>().onClick.AddListener(delegate { onItemClickAlbum(cloneItem); });
                cloneItem.GetComponent<Text>().text = " " + i;
                cloneItem.transform.GetChild(0).GetComponent<Image>().sprite = getPic(db.readField("SELECT Ruta FROM CANCION WHERE IdAlbum='" + db.readField("SELECT ID_ALBUM FROM ALBUM WHERE Nome_Album='" + dates[i].group + "'") + "'"));
                cloneItem.transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
                cloneItem.transform.GetChild(1).GetComponent<Text>().text = "" + dates[i].count;
                cloneItem.transform.GetChild(2).GetComponent<Text>().text = "" + dates[i].group;
                cloneItem.transform.SetParent(contentResult);
                cloneItem.transform.localScale = new Vector3(1, 1, 1);
                int size = 30;
                if (dates.Count % 5 != 0)
                    size = 250;
                contentResult.GetComponent<RectTransform>().sizeDelta = new Vector2(contentResult.GetComponent<RectTransform>().sizeDelta.x, size + groupHeight * dates.Count / 5);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentResult.GetComponent<RectTransform>());
            }
        }
        print(dates.Count);
    }

    public void fillGroupsDate()
    {
        dates = db.readGroup("SELECT Data, COUNT(*) FROM CANCION GROUP BY Data");

        if (dates.Count != 0)
        {
            for (int i = 0; i < dates.Count; i++)
            {
                popup.SetActive(true);
                restore.gameObject.SetActive(true);
                GameObject cloneItem = Instantiate(result);
                cloneItem.GetComponent<Button>().onClick.AddListener(delegate { onItemClickDate(cloneItem); });
                cloneItem.GetComponent<Text>().text = " " + i;
                cloneItem.transform.GetChild(0).GetComponent<Image>().sprite = getPic(db.readField("SELECT Ruta FROM CANCION WHERE Data='" + dates[i].group + "'"));
                cloneItem.transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
                cloneItem.transform.GetChild(1).GetComponent<Text>().text = "" + dates[i].count;
                cloneItem.transform.GetChild(2).GetComponent<Text>().text = "" + dates[i].group;
                cloneItem.transform.SetParent(contentResult);
                cloneItem.transform.localScale = new Vector3(1, 1, 1);
                int size = 30;
                if (dates.Count % 5 != 0)
                    size = 250;
                contentResult.GetComponent<RectTransform>().sizeDelta = new Vector2(contentResult.GetComponent<RectTransform>().sizeDelta.x, size + groupHeight * dates.Count / 5);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentResult.GetComponent<RectTransform>());
            }
        }
        print(dates.Count);
    }

    public void onHideClick()
    {
        if (scrollView.activeSelf)
        {
            scrollView.SetActive(false);
            hide.transform.localRotation = Quaternion.Euler(0, 180, 0);
            hideList.transform.position = new Vector3(20, hideList.transform.position.y, hideList.transform.position.z);
            listaImagen.childAlignment = TextAnchor.MiddleCenter;
        }
        else
        {
            scrollView.SetActive(true);
            hide.transform.localRotation = Quaternion.Euler(0, 0, 0);
            listaImagen.childAlignment = TextAnchor.MiddleLeft;
            hideList.transform.position = position;
        }
    }

    public void onBackClick()
    {
        if (songs.Count != 0)
        {
            if (rand)
            {
                index = new System.Random().Next(songs.Count);
                title.text = getArtist(index) + " - " + songs[index].title;
                albumCover.sprite = getPic(songs[index].path);
                _ = updateAsync();
            }
            else if (index > 0)
            {
                index--;
                title.text = getArtist(index) + " - " + songs[index].title;
                albumCover.sprite = getPic(songs[index].path);
                _ = updateAsync();
            }
        }
    }

    public void onForwardClick()
    {
        if (songs.Count != 0)
        {
            if (rand)
            {
                index = new System.Random().Next(songs.Count);
                title.text = getArtist(index) + " - " + songs[index].title;
                albumCover.sprite = getPic(songs[index].path);
                _ = updateAsync();
            }
            else if (index < songs.Count - 1)
            {
                index++;
                title.text = getArtist(index) + " - " + songs[index].title;
                albumCover.sprite = getPic(songs[index].path);
                _ = updateAsync();
            }
        }
    }

    public async Task updateAsync()
    {
        try
        {
            StopCoroutine(updateSong());
            reproductor.Stop();
            playing = false;
            string path = "" + songs[index].path;
            if (Application.platform == RuntimePlatform.Android)
            {
                path = GetUri(path);
            }
            reproductor.clip = await LoadClip(path);
            reproductor.time = 0;
            stringCounts();
            slider.maxValue = reproductor.clip.length;
            reproductor.Play();
            playing = true;
            ended = false;
            imagePlay.overrideSprite = pause;
            StartCoroutine(updateSong());
        } catch(Exception e)
        {
            showToast(e.ToString(), 10);
        }
    }

    private string GetUri(string uri)
    {
        if (uri.Contains("://") || uri.Contains(":///"))
            return uri;

        return "file://" + uri;
    }

    async Task<AudioClip> LoadClip(string path)
    {
        AudioClip clip = null;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            uwr.SendWebRequest();
            print(uwr.url);
            // wrap tasks in try/catch, otherwise it'll fail silently
            try
            {
                while (!uwr.isDone) await Task.Delay(5);

                if (uwr.result==UnityWebRequest.Result.ConnectionError || uwr.result==UnityWebRequest.Result.ProtocolError) Debug.Log($"{uwr.error}");
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            catch (Exception err)
            {
                Debug.Log($"{err.Message}, {err.StackTrace}");
            }
        }

        return clip;
    }

    public void onLoopClick()
    {
        if (reproductor.loop)
        {
            reproductor.loop = false;
            loop.image.color = new Color(1, 1, 1);
        }
        else
        {
            reproductor.loop = true;
            loop.image.color = new Color(0.58f, 0.38f, 0.84f);

        }
    }

    public void onRandomClick()
    {
        if (rand)
        {
            rand = false;
            random.image.color = new Color(1, 1, 1);
        }
        else
        {
            rand = true;
            index = new System.Random().Next(songs.Count);
            random.image.color = new Color(0.58f, 0.38f, 0.84f);
        }
    }

    public void onEditClick()
    {
        string artistid = randomID();
        string albumid = randomID();
        if (!db.readName("SELECT ID_ARTISTA FROM ARTISTA WHERE Nome_Artista='" + artistEdit.text + "'"))
            db.execInsert("INSERT INTO ARTISTA(ID_ARTISTA, Nome_Artista) VALUES('" + artistid + "','" + artistEdit.text + "')");
        else if (db.readName("SELECT ID_ARTISTA FROM ARTISTA WHERE Nome_Artista='" + artistEdit.text + "'"))
            artistid = db.readField("SELECT ID_ARTISTA FROM ARTISTA WHERE Nome_Artista='" + artistEdit.text + "'");
        else
            artistid = songs[index].id_artist;
        if (!db.readName("SELECT ID_ALBUM FROM ALBUM WHERE Nome_Album='" + albumEdit.text + "'"))
            db.execInsert("INSERT INTO ALBUM(ID_ALBUM, Nome_Album) VALUES('" + albumid + "','" + albumEdit.text + "')");
        else if (db.readName("SELECT ID_ALBUM FROM ALBUM WHERE Nome_Album='" + albumEdit.text + "'"))
            albumid = db.readField("SELECT ID_ALBUM FROM ALBUM WHERE Nome_Album='" + albumEdit.text + "'");
        else
            albumid = songs[index].id_album;
        db.execInsert("UPDATE CANCION SET Titulo='" + titleEdit.text + "', Data='" + dateEdit.text + "', IdAlbum='" + albumid + "'," +
                "IdArtista='" + artistid + "' WHERE ID_CANCION='" + songs[index].id_song + "'");
        songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION");
        GameObject[] clones = GameObject.FindGameObjectsWithTag("ListItem");
        GameObject texts = clones[index].transform.GetChild(1).gameObject;
        texts.transform.GetChild(0).GetComponent<Text>().text = getArtist(index);
        texts.transform.GetChild(1).GetComponent<Text>().text = "" + songs[index].title;
        editPanel.SetActive(false);
    }

    public void onDownloadClick()
    {
        if (!downloadField.text.Equals(""))
        {
            Uri.IsWellFormedUriString(downloadField.text, UriKind.Absolute);
            if (downloadField.text.Substring(downloadField.text.Length - 3).Equals("mp3"))
                _ = DownloadAsync();
            else
                print("Not valid");
        } 
    }

    public async Task DownloadAsync()
    {
        try
        {
            string path = Application.dataPath + "/Downloads";
            Directory.CreateDirectory(path);
            string[] arr = downloadField.text.Split("/");
            string name = arr[arr.Length - 1];
            print(path);
            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(downloadField.text);
            using var fs = new FileStream(path + "/" + name, FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);
            fs.Close();
            s.Close();
            client.Dispose();
            print(path + "/" + name);
            getMetadata(path + "/" + name);
            songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION");
            string name_list = "Default";
            string listid = randomID();

            if (!db.readName("SELECT ID_LISTA FROM LISTA WHERE Nome_Lista='" + name_list + "'"))
                db.execInsert("INSERT INTO LISTA (ID_LISTA,Nome_Lista) VALUES ('" + listid + "','" + name_list + "')");
            else
                listid = db.readField("SELECT ID_LISTA FROM LISTA WHERE Nome_Lista='" + name_list + "'");

            db.execInsert("INSERT OR IGNORE INTO LISTACANCION (IdLista,IdCancion) VALUES ('" + listid + "','" + songs[songs.Count-1].id_song + "')");
            addtoLast();
        }
        catch(Exception e){
            print(e.StackTrace+" "+e.InnerException+" "+e.Message);
        }
    }

    public void getMetadata(string filePath)
    {

        string title="";
        string date="";
        int duration=0;
        string path="";
        string name_artist="";
        string name_album="";
        try
        {
            var tfile = TagLib.File.Create(filePath);
            path=filePath;
            if (tfile.Tag.Title != null)
                title=tfile.Tag.Title;
            else
                title=Path.GetFileNameWithoutExtension(tfile.Name);
            if (tfile.Tag.Performers.Length != 0)
                name_artist = tfile.Tag.Performers[0];
            else
                name_artist = "Desconocido";
            if (tfile.Tag.Album != null)
                name_album = tfile.Tag.Album;
            else
                name_album = "Desconocido";
            if (tfile.Tag.Year != 0)
                date = "" + tfile.Tag.Year;
            else
                date = "Desconocido";
            if (tfile.Properties.Duration != null)
                duration = (int)tfile.Properties.Duration.TotalMilliseconds;
        

            string id_song = randomID();
            string id_artist = randomID();
            string id_album = randomID();
            string uartid = "";
            string ualbid = "";
            name_artist = name_artist.Replace("'", "''");
            name_album = name_album.Replace("'", "''");
            path = path.Replace("'", "''");
            title = title.Replace("'", "''");

            if (!db.readName("SELECT Nome_Artista FROM ARTISTA WHERE Nome_Artista='" + name_artist + "'"))
            {
                uartid = id_artist;
                db.execInsert("INSERT INTO ARTISTA(ID_ARTISTA, Nome_Artista) VALUES('" + uartid + "','" + name_artist + "')");
            }
            else
            {
                uartid = db.readField("SELECT ID_ARTISTA FROM ARTISTA WHERE Nome_Artista='" + name_artist + "'");
            }
            if (!db.readName("SELECT Nome_Album FROM ALBUM WHERE Nome_Album='" + name_album + "'"))
            {
                ualbid = id_album;
                db.execInsert("INSERT INTO ALBUM(ID_ALBUM, Nome_Album) VALUES('" + ualbid + "','" + name_album + "')");
            }
            else
            {
                ualbid = db.readField("SELECT ID_ALBUM FROM ALBUM WHERE Nome_Album='" + name_album + "'");
            }
            if (!db.readName("SELECT ID_CANCION FROM CANCION WHERE Ruta='" + path + "'"))
                db.execInsert("INSERT INTO CANCION(ID_CANCION, Titulo, Data, Duracion, Ruta, IdArtista, IdAlbum) VALUES " +
                                                        "('" + id_song + "','" + title + "','" + date + "','" + duration + "','" + path + "','" + uartid + "','" + ualbid + "')");

        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }

    public string randomID()
    {
        return Guid.NewGuid().ToString().Substring(24);
    }

    public Sprite getPic(string path)
    {
        var tfile = TagLib.File.Create(path);
        if (tfile.Tag.Pictures.Length != 0)
        {
            TagLib.IPicture pic = tfile.Tag.Pictures[0];

            MemoryStream ms = new MemoryStream(pic.Data.Data);
            ms.Seek(0, SeekOrigin.Begin);

            //Create texture2d with MemoryStream 
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(ms.ToArray());

            return (Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
        }
        else
            return (Resources.Load<Sprite>("default_img"));
    }

    public Sprite getBackground(string path)
    {
        
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite;
        Texture2D SpriteTexture = LoadTexture(path);
        NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0));

        return NewSprite;
        
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (System.IO.File.Exists(FilePath))
        {
            FileData = System.IO.File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public string getArtist(int i)
    {
        return db.readField("SELECT Nome_Artista FROM ARTISTA WHERE ID_ARTISTA='" + songs[i].id_artist + "'");
    }

    public void menuValue(int value)
    {
        switch (value)
        {
            case 0:
                print(dropdown.options[dropdown.value].text);
                ListExistPanel.SetActive(true);
                emptyList("ExistList");
                ListExistPanel.SetActive(false);
                addSongtoList();
                break;
            case 1:
                print(dropdown.options[dropdown.value].text);
                editPanel.SetActive(true);
                imageEdit.sprite = albumCover.sprite;
                titleEdit.text = songs[index].title;
                dateEdit.text = songs[index].date;
                artistEdit.text = db.readField("SELECT Nome_Artista FROM ARTISTA WHERE ID_ARTISTA='" + songs[index].id_artist + "'");
                albumEdit.text = db.readField("SELECT Nome_Album FROM ALBUM WHERE ID_ALBUM='" + songs[index].id_album + "'");
                break;
            case 2:
                print(dropdown.options[dropdown.value].text);
                downloadPanel.SetActive(true);
                break;
            case 3:
                print(dropdown.options[dropdown.value].text);

                addSongs();

                break;
            case 4:
                break;
            default:
                break;
        }
        dropdown.value = 4;
    }

    public void addSongs()
    {
        FileBrowser.SetDefaultFilter(".*");
        FileBrowser.ShowLoadDialog((paths) => { addSongsSuccess(paths[0]); }, () => { print("Canceled"); },
        						   FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select" );
    }

    public void addSongsSuccess(string path)
    {
        print(path);
        string[] files = Directory.GetFiles(path);
        showToast("Reading " + files.Length + " files", 5);
        int i = 0;
        foreach (string file in files)
        {
            if (file != null)
                if (file.Substring(file.Length - 3).Equals("mp3"))
                {
                    i++;
                    print(file);
                    getMetadata(file);
                    txt.text = "Reading " + i + " files";
                }
        }

        songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION");

        if (songs.Count != 0)
        {
            defaultList();
            playFirst();
            emptyPanel.SetActive(true);
            emptyPanel.SetActive(false);
        }
        print(songs.Count);
        emptyList("ListItem");
        fillList();
    }

    public void emptyList(string item)
    {
        GameObject[] clones = GameObject.FindGameObjectsWithTag(item);
        for(int i = clones.Length - 1; i >= 0; i--)
        {
            Destroy(clones[i]);
        }
    }

    public void fillList()
    {
        for (int i = 0; i < songs.Count; i++)
        {
            GameObject cloneItem = Instantiate(itemList);
            cloneItem.GetComponent<Button>().onClick.AddListener(delegate { onItemClick(cloneItem); });
            cloneItem.GetComponent<Text>().text = " " + i;
            cloneItem.transform.GetChild(0).GetComponent<Image>().sprite = getPic(songs[i].path);
            cloneItem.transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
            GameObject texts = cloneItem.transform.GetChild(1).gameObject;
            texts.transform.GetChild(0).GetComponent<Text>().text = getArtist(i);
            texts.transform.GetChild(1).GetComponent<Text>().text = "" + songs[i].title;
            cloneItem.transform.SetParent(content);
            cloneItem.transform.localScale = new Vector3(1, 1, 1);
            content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, contentHeight * songs.Count);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        }
    }

    public void addtoLast()
    {
        GameObject cloneItem = Instantiate(itemList);
        cloneItem.GetComponent<Button>().onClick.AddListener(delegate { onItemClick(cloneItem); });
        cloneItem.GetComponent<Text>().text = " " + (songs.Count-1);
        cloneItem.transform.GetChild(0).GetComponent<Image>().sprite = getPic(songs[(songs.Count - 1)].path);
        cloneItem.transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
        GameObject texts = cloneItem.transform.GetChild(1).gameObject;
        texts.transform.GetChild(0).GetComponent<Text>().text = getArtist((songs.Count - 1));
        texts.transform.GetChild(1).GetComponent<Text>().text = "" + songs[(songs.Count - 1)].title;
        cloneItem.transform.SetParent(content);
        cloneItem.transform.localScale = new Vector3(1, 1, 1);
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, contentHeight * songs.Count);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    public void onItemClick(GameObject clone)
    {
        GameObject parent = clone;
        index = Int32.Parse(parent.GetComponent<Text>().text);
        clicked = true;
        title.text = getArtist(index) + " - " + songs[index].title;
        albumCover.sprite = getPic(songs[index].path);
        _ = updateAsync();
    }

    public void onItemClickList(GameObject clone)
    {
        GameObject parent = clone;
        index = Int32.Parse(parent.GetComponent<Text>().text);
        songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION  INNER JOIN LISTACANCION ON IdCancion=ID_CANCION WHERE IdLista='" + lists[index].group + "'");
        clicked = true;
        emptyList("ListItem");
        fillList();
        playFirst();
    }

    public void onItemClickListDelete(GameObject clone)
    {
        GameObject parent = clone;
        index = Int32.Parse(parent.GetComponent<Text>().text);

        db.execInsert("DELETE FROM LISTA WHERE ID_LISTA = '" + lists[index].group + "'");
        emptyList("ListDelete");
        onDeleteList();
        emptyList("SearchItem");
        fillGroupsList();
        
    }

    public void onItemClickListSong(GameObject clone)
    {
        GameObject parent = clone;
        int i = Int32.Parse(parent.GetComponent<Text>().text);
        Toggle toggleList = parent.transform.GetChild(0).GetComponent<Toggle>();
        if (toggleList.isOn)
            db.execInsert("INSERT INTO LISTACANCION (IdLista,IdCancion) VALUES ('" + lists[i].group + "','" + songs[index].id_song + "')");
        else
            db.execInsert("DELETE FROM LISTACANCION WHERE IdCancion = '" + songs[index].id_song + "' AND IdLista = '" + lists[i].group + "'");

    }

    public void onItemClickArtist(GameObject clone)
    {
        GameObject parent = clone;
        index = Int32.Parse(parent.GetComponent<Text>().text);
        songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION WHERE IdArtista='" + db.readField("SELECT ID_ARTISTA FROM ARTISTA WHERE Nome_Artista='" + dates[index].group + "'") + "'");
        clicked = true;
        emptyList("ListItem");
        fillList();
        playFirst();
    }

    public void onItemClickAlbum(GameObject clone)
    {
        GameObject parent = clone;
        index = Int32.Parse(parent.GetComponent<Text>().text);
        songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION WHERE IdAlbum='" + db.readField("SELECT ID_ALBUM FROM ALBUM WHERE Nome_Album='" + dates[index].group + "'") + "'");
        clicked = true;
        emptyList("ListItem");
        fillList();
        playFirst();
    }

    public void onItemClickDate(GameObject clone)
    {
        GameObject parent = clone;
        index = Int32.Parse(parent.GetComponent<Text>().text);
        songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION WHERE Data='" + dates[index].group + "'");
        clicked = true;
        emptyList("ListItem");
        fillList();
        playFirst();
    }

    public void ordersValue(int value)
    {
        switch (value)
        {
            case 0:
                songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION ORDER BY Titulo ASC");
                emptyList("ListItem");
                fillList();
                playFirst();
                break;
            case 1:
                songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION ORDER BY Data ASC");
                emptyList("ListItem");
                fillList();
                playFirst();
                break;
            case 2:
                songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION INNER JOIN ARTISTA ON ID_ARTISTA=IdArtista ORDER BY Nome_Artista ASC");
                emptyList("ListItem");
                fillList();
                playFirst();
                break;
            case 3:
                songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION INNER JOIN ALBUM ON ID_ALBUM = IdAlbum ORDER BY Nome_Album ASC");
                emptyList("ListItem");
                fillList();
                playFirst();
                break;
            case 4:
                songs = db.readSongs("SELECT ID_CANCION,Titulo,Data,Duracion,Ruta,IdArtista,IdAlbum FROM CANCION INNER JOIN ALBUM ON ID_ALBUM = IdAlbum ORDER BY Duracion ASC");
                emptyList("ListItem");
                fillList();
                playFirst();
                break;
            default:
                break;
        }
    }

    public void groupsValue(int value)
    {
        switch (value)
        {
            case 0:
                popup.SetActive(true);
                emptyList("SearchItem");
                popup.SetActive(false);
                fillGroupsArtist();
                break;
            case 1:
                popup.SetActive(true);
                emptyList("SearchItem");
                popup.SetActive(false);
                fillGroupsAlbum();
                break;
            case 2:
                popup.SetActive(true);
                emptyList("SearchItem");
                popup.SetActive(false);
                fillGroupsDate();
                break;
            default:
                break;
        }
    }
}

public class Database
{
    IDbConnection dbConnection;
    IDataReader dataReader;

    private IDbConnection CreateAndOpenDatabase()
    {
        // Open a connection to the database.
        string dbUri = "Reproductor.sqlite";
        if(Application.platform == RuntimePlatform.Android)
        {
            dbUri = Application.persistentDataPath + "/Reproductor.db";
            if (!System.IO.File.Exists(dbUri))
            {
                using (UnityWebRequest uwr = UnityWebRequest.Get("jar:file://" + Application.dataPath + "!/assets/" + "Reproductor.db"))
                {
                    uwr.SendWebRequest();
                    // wrap tasks in try/catch, otherwise it'll fail silently

                    while (!uwr.isDone) { }

                        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError) Debug.Log($"{uwr.error}");

                        else
                            System.IO.File.WriteAllBytes(dbUri, uwr.downloadHandler.data);
                }
            }
        }
        IDbConnection dbConnection = new SqliteConnection("URI=file:"+dbUri);
        dbConnection.Open();

        return dbConnection;
    }

    public void execCreate(string command)
    {
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandCreateTable = dbConnection.CreateCommand();
        dbCommandCreateTable.CommandText = command;
        dbCommandCreateTable.ExecuteReader();
        dbConnection.Close();
    }

    public void execInsert(string command)
    {
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandInsertValue = dbConnection.CreateCommand();
        dbCommandInsertValue.CommandText = command;
        dbCommandInsertValue.ExecuteNonQuery();
        dbConnection.Close();
    }

    public bool readName(string command)
    {
        bool result = false;
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = command;
        dataReader = dbCommandReadValues.ExecuteReader();
        result = dataReader.Read();
        dataReader.Close();
        dbConnection.Close();
        return result;
    }

    public List<Song> readSongs(string command)
    {
        List<Song> canciones = new List<Song>();
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = command;
        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        while (dataReader.Read())
        {
            string id = dataReader.GetString(0);
            string title = dataReader.GetString(1);
            string date = dataReader.GetString(2);
            int duration = dataReader.GetInt32(3);
            string path = dataReader.GetString(4);
            string idart = dataReader.GetString(5);
            string idalb = dataReader.GetString(6);
            canciones.Add(new Song(id, title, date, duration, path, idart, idalb));
        }
        dataReader.Close();
        dbConnection.Close();
        return canciones;
    }

    public List<Album> readAlbums(string command)
    {
        List<Album> album = new List<Album>();
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = command;
        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        while (dataReader.Read())
        {
            string id=dataReader.GetString(0);
            string name=dataReader.GetString(1);
            album.Add(new Album(id, name));
        }
        dataReader.Close();
        dbConnection.Close();
        return album;
    }

    public List<Artist> readArtists(string command)
    {
        List<Artist> artist = new List<Artist>();
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = command;
        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        while (dataReader.Read())
        {
            string id = dataReader.GetString(0);
            string name = dataReader.GetString(1);
            artist.Add(new Artist(id, name));
        }
        dataReader.Close();
        dbConnection.Close();
        return artist;
    }

    public string readField(string command)
    {
        string artist = "";
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = command;
        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        while (dataReader.Read())
        {
            artist = dataReader.GetString(0);
        }
        dataReader.Close();
        dbConnection.Close();
        return artist;
    }

    public List<Group> readGroup(string command)
    {
        List<Group> group = new List<Group>();
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = command;
        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        while (dataReader.Read())
        {
            group.Add(new Group(dataReader.GetString(0), "" + dataReader.GetInt32(1)));
        }
        dataReader.Close();
        dbConnection.Close();
        return group;
    }

    public List<Group> readList(string command)
    {
        List<Group> group = new List<Group>();
        dbConnection = CreateAndOpenDatabase();
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = command;
        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        while (dataReader.Read())
        {
            group.Add(new Group(dataReader.GetString(0), "" + dataReader.GetString(1)));
        }
        dataReader.Close();
        dbConnection.Close();
        return group;
    }

    public void close()
    {
        dataReader.Close();
        dbConnection.Close();
    }
}

public class Song
{
    public string id_song { get; set; }
    public string title { get; set; }
    public string date { get; set; }
    public int duration { get; set; }
    public string path { get; set; }
    public string id_album { get; set; }
    public string id_artist { get; set; }

    public Song(string id_song, string title, string date, int duration, string path, string id_artist, string id_album)
    {
        this.id_song = id_song;
        this.title = title;
        this.date = date;
        this.duration = duration;
        this.path = path;
        this.id_album = id_album;
        this.id_artist = id_artist;
    }
}

public class Album
{
    public string id_album { get; set; }
    public string name_album { get; set; }

    public Album(string id_album, string name_album)
    {
        this.id_album = id_album;
        this.name_album = name_album;
    }
}

public class Artist
{
    public string id_artist { get; set; }
    public string name_artist { get; set; }

    public Artist(string id_artist, string name_artist)
    {
        this.id_artist = id_artist;
        this.name_artist = name_artist;
    }
}

public class Group
{
    public string group { get; set; }
    public string count { get; set; }

    public Group(string group, string count)
    {
        this.group = group;
        this.count = count;
    }
}
