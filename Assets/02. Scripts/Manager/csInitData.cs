using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using LitJson;

// 플레이어 데이터 정보
public class PlayerData
{
    public string name = "";
    public int wood = 0;
    public int dmg = 0;

    public PlayerData(PlayerData _data)
    {
        name = _data.name;
        wood = _data.wood;
        dmg = _data.dmg;
    }

    public PlayerData()
    {
        name = "";
        wood = 0;
        dmg = 0;
    }

    // 플레이어 데이터 초기화
    public PlayerData(string _name, int _wood, int _dmg)
    {
        name = _name;
        wood = _wood;
        dmg = _dmg;
    }
}

public class csInitData : MonoBehaviour
{
    public static csInitData instance;

    [HideInInspector]
    public GameObject grass1;
    [HideInInspector]
    public GameObject grass2;

    [HideInInspector]
    public GameObject water;

    [HideInInspector]
    public GameObject map;

    [HideInInspector]
    public GameObject target;

    [HideInInspector]
    public GameObject[] tree;

    [HideInInspector]
    public Sprite btntrue;

    [HideInInspector]
    public Sprite btnfalse;

    [HideInInspector]
    public GameObject wood;

    [HideInInspector]
    public GameObject hpbar;

    [HideInInspector]
    public string use_path = "";
    private const string data_Route = "/Resources/Player_Data.text";

    [HideInInspector]
    public PlayerData myData;

    private List<PlayerData> myDataList = new List<PlayerData>();

    private bool datacheck = false;

    private void Awake()
    {
        instance = this;
        // 에디터 버전 지정된 경로
#if UNITY_EDITOR
        use_path = Application.dataPath + data_Route;
        // 핸드폰 지정 경로
#elif UNITY_ANDROID
        use_path = Application.persistentDataPath + "/Data.text";
#endif
        
        // 처음 플레이 할때
        if (!File.Exists(use_path))
        {
            Debug.Log("Json새로생성");

            FileStream tempfile = File.Create(use_path);
            datacheck = true;
        }
        // 플레이어 정보가 있을때
        else
        {
            LoadJson();
        }

        tree = new GameObject[9];

        map = GameObject.FindGameObjectWithTag("Map");

        //리소스폴더에 저장된 정보 가져오기
        grass1 = Resources.Load<GameObject>("grass1");
        grass2 = Resources.Load<GameObject>("grass2");
        water = Resources.Load<GameObject>("water");
        target = Resources.Load<GameObject>("SetTarget");

        btntrue = Resources.Load<Sprite>("IMG/button");
        btnfalse = Resources.Load<Sprite>("IMG/button2");

        wood = Resources.Load<GameObject>("woodCut");

        hpbar = Resources.Load<GameObject>("Hp_Bar");

        string temp = "";

        for (int i = 0; i < tree.Length; i++)
        {
            temp = "Tree/B/TreeB" + (i + 1);
            tree[i] = Resources.Load<GameObject>(temp);
        }

        Debug.Log("===== FINSH INIT DATA =====");
    }

    private void Start()
    {
        GGCheck("test_id");
    }

    public void GGCheck(string _id)
    {
        PlayerData tempData = new PlayerData(_id, 0, 1);

        if (datacheck)
        {
            SavePlayerInfo(tempData);
            LoadPlayerInfo(myData.name);
        }
        else
        {
            LoadPlayerInfo(_id);
        }
    }

    // 경로 파일에서 Json파일 불러오기(플레이어 정보 불러오기)
    private void LoadJson()
    {
        string jsonStr = File.ReadAllText(use_path);

        JsonData playerData = JsonMapper.ToObject(jsonStr);

        for (int i = 0; i < playerData.Count; i++)
        {
            PlayerData loadData = new PlayerData();

            loadData.name = playerData[i]["name"].ToString();

            string toint = playerData[i]["wood"].ToString();
            loadData.wood = Convert.ToInt32(toint);

            toint = playerData[i]["dmg"].ToString();
            loadData.dmg = Convert.ToInt32(toint);

            myDataList.Add(loadData);
        }
    }

    //플레이어 정보 가져오기
    public bool LoadPlayerInfo(string _name)
    {
        {
            bool find_data = false;

            string jsonStr = File.ReadAllText(use_path);

            JsonData playerData = JsonMapper.ToObject(jsonStr);

            for (int i = 0; i < playerData.Count; i++)
            {
                PlayerData loadData = new PlayerData();

                loadData.name = playerData[i]["name"].ToString();

                if (loadData.name != _name)
                {
                    continue;
                }

                find_data = true;

                string toint = playerData[i]["wood"].ToString();
                loadData.wood = Convert.ToInt32(toint);

                toint = playerData[i]["dmg"].ToString();
                loadData.dmg = Convert.ToInt32(toint);

                //myDataList.Add(loadData);
                myData = new PlayerData(loadData);
            }

            if (!find_data)
            {
                return false;
            }
        }
        return true;
    }


    // 플레이어 정보 저장
    public void SavePlayerInfo(PlayerData data)
    {
        if (File.Exists(use_path))
        {
            string jsonStr = File.ReadAllText(use_path);

            JsonData playerData = JsonMapper.ToObject(jsonStr);

            if (!datacheck)
            {
                for (int i = 0; i < playerData.Count; i++)
                {
                    if (playerData[i]["name"].ToString().Equals(data.name))
                    {
                        myDataList.RemoveAt(i);
                    }
                }
            }
            else if (datacheck)
            {
                datacheck = false;
            }

            myDataList.Add(data);

            WriteJsonToText();
        }
    }

    // 플레이어 데이터 저장
    public void SavePlayerData(int _wood)
    {
        myData.wood = _wood;

        SavePlayerInfo(myData);
        LoadPlayerInfo(myData.name);
    }

    // 플레이어 데미지 상승
    public void UpgradePlayerDmg()
    {
        myData.wood -= myData.dmg * 10;
        myData.dmg += 1;

        SavePlayerInfo(myData);
        LoadPlayerInfo(myData.name);
    }

    // 제이슨 파일 정보를 덮어씌우기
    void WriteJsonToText()
    {
        JsonData infoJson = JsonMapper.ToJson(myDataList);

        File.WriteAllText(use_path, infoJson.ToString());
    }
}
