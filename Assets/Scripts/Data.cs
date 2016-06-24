using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class Data : MonoBehaviour
{ }

    [Serializable]
public class DataController
{
    //[NonSerialized]
    public StageData[] stageData;
    private static readonly string savePath = Application.dataPath + "/save.bytes";
    static DataController instance;
    public static DataController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DataController();
                instance.stageData = new StageData[3] { new StageData(), new StageData(),new StageData() };
            }
            return instance;
        }
        set { instance = value; }
    }

    private DataController() { }

    public bool Save()
    {
        //stg1 = stageData[0];
        //stg2 = stageData[1];
        MemoryStream memoryStream = new MemoryStream();
#if UNITY_IPHONE || UNITY_IOS
		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(memoryStream, instance);

        string tmp = System.Convert.ToBase64String(memoryStream.ToArray());
        try
        {
            PlayerPrefs.SetString(savePath, tmp);
        }
        catch (PlayerPrefsException)
        {
            return false;
        }
        PlayerPrefs.Save();
        return true;
    }

    public DataController Load()
    {
        if (!PlayerPrefs.HasKey(savePath)) return default(DataController);
#if UNITY_IPHONE || UNITY_IOS
		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        BinaryFormatter bf = new BinaryFormatter();
        string serializedData = PlayerPrefs.GetString(savePath);

        MemoryStream dataStream = new MemoryStream(System.Convert.FromBase64String(serializedData));
        instance = (DataController)bf.Deserialize(dataStream);
        return instance;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

[Serializable]
public class StageData
{
    public int bestScore;//最速クリアスコア
    public int playCount;//挑戦回数
    public int winCount;//勝利数
    public StageData()
    {
        bestScore = -1;
        playCount = 0;
        winCount = 0;
    }
}
