using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    float correctionSize = 0.5f;
    [SerializeField]
    int size, stageNo;
    [SerializeField]
    GameObject cat, bucket, result;
    [SerializeField]
    Text text, turn, time;
    int turnCount;
    int phaseNo;//操作フェイズ
    enum PhaseName
    {
        初期化 = 0, センサー設置, パネル除去, センサー検出, パネル点滅, パネル初期化, ゲーム終了, リザルト
    }
    int counter;
    bool mouseStay;//ボタンを押しっぱなしにしているか
    bool win;//猫発見できたかどうか
    GameObject[] panels;
    GameObject[] buckets;
    Vector2 mousePosSub;//マウスクリック座標(正規化後)
    Color[] signal;
    Color gold = new Color(0.9f, 0.6f, 0.3f);
    string[] signalName;
    int removeCount;//消したパネルの個数
    int skillCount;//スキル発動までに必要なパネル数
    int skillTimer;
    int catCount;
    float startTime, nowTime;

    // Use this for initialization
    void Start()
    {
        correctionSize = size % 2 == 0 ? 0 : 0.5f;
        phaseNo = (int)PhaseName.パネル初期化;
        mouseStay = false;
        float catCorrect = 0.5f;
        cat.transform.position = new Vector2(Random.Range(0, size) - size * 0.5f + catCorrect,
            Random.Range(0, size) - size * 0.5f + catCorrect);
        turnCount = 0;
        signal = new Color[3] { Color.red, Color.blue, Color.green };
        signalName = new string[3] { "赤","青","緑" };
        removeCount = 0;
        skillTimer = 3;
        skillCount = skillTimer;
        startTime = Time.time;
        nowTime = 0;
        buckets = new GameObject[skillTimer];
        for(int i=0;i<buckets.Length;i++)//バケツ初期化
        {
            buckets[i] = Instantiate(bucket);
            buckets[i].SetActive(true);
            buckets[i].transform.SetParent(gameObject.transform);
            buckets[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-200 + 70 * i, -300);
            buckets[i].GetComponent<RectTransform>().localScale = Vector3.one;
            buckets[i].GetComponent<Image>().color = Color.yellow;
        }
        text.text = "猫以外のパネルをすべて消してください。\nタップしてパネルを消す。";
    }

    // Update is called once per frame
    void Update()
    {
        #region ゲーム基本処理
        switch (phaseNo)
        {
            case 0://初期化
                phaseNo = 1;
                //}
                //text.text += "\n消すパネルを選んでください。";
                if (skillCount == skillTimer)
                {
                    foreach (GameObject panel in GameObject.FindGameObjectsWithTag("Panel"))
                    {
                        if (panel.GetComponent<SpriteRenderer>().color != Color.white)
                        {
                            panel.transform.FindChild("garbage").GetComponent<Animator>().SetBool("Skill", true);
                        }
                    }
                }
                else if (skillCount == skillTimer + 1)
                {
                    foreach (GameObject panel in GameObject.FindGameObjectsWithTag("Panel"))
                    {
                        if (panel.GetComponent<SpriteRenderer>().color != Color.white)
                        {
                            panel.transform.FindChild("garbage").GetComponent<Animator>().SetBool("Skill", false);
                        }
                    }
                    for(int i=1;i<skillTimer;i++)
                    {
                        buckets[i].GetComponent<Image>().color = Color.white;
                    }
                    skillCount = 1;
                }
                if (removeCount == catCount - 1)
                {
                    foreach (GameObject panel in GameObject.FindGameObjectsWithTag("Panel"))
                    {
                        if (panel.GetComponent<SpriteRenderer>().color == Color.white)
                        {
                            panel.GetComponent<SpriteRenderer>().color = gold;
                            Debug.Log("でいてはいる");
                        }
                        Debug.Log("おかしいぞ");
                    }
                }
                turnCount++;
                turn.text = "残りパネル : " + (catCount - removeCount).ToString();
                break;
            case 1:
                #region センサー設置
                if (Input.GetMouseButtonDown(0))//ボタン押下、一本目のバーを設置
                {
                    Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);//タッチ座標
                    float sizeHalf = size * 0.5f;
                    float barCorrection = 0.5f;
                    mousePosSub = Vector2.zero;
                    if ((Mathf.Abs(pos.x) < sizeHalf && (Mathf.Abs(pos.y) < sizeHalf)))
                    {
                        mousePosSub = new Vector2(Mathf.Round(pos.x - correctionSize - 0.5f) + 0.5f + correctionSize,
                            Mathf.Round(pos.y - correctionSize - 0.5f) + 0.5f + correctionSize);
                        mouseStay = true;
                    }
                }
                else if (mouseStay)//ボタン押中
                {
                    Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    float sizeHalf = size * 0.5f;
                    if (Input.GetMouseButtonUp(0))//ボタン押上時、2本目のバーを設置
                    {
                        phaseNo = (int)PhaseName.パネル除去;
                        if (mousePosSub != Vector2.zero)
                        {
                            Vector2 posSub = new Vector2(Mathf.Round(pos.x - correctionSize - 0.5f) + 0.5f + correctionSize,
                                Mathf.Round(pos.y - correctionSize - 0.5f) + 0.5f + correctionSize);
                            if (mousePosSub != posSub)
                            {
                                phaseNo = (int)PhaseName.センサー設置;
                            }
                        }
                    }
                }
                #endregion
                break;
            case 2://パネル除去
                #region パネル除去
                Vector2 catPos = cat.transform.position;
                int countLimit = 50;
                win = true;
                foreach (GameObject panel in GameObject.FindGameObjectsWithTag("Panel"))
                {
                    Vector2 panelPos = panel.transform.position;
                    bool destroying = panel.GetComponent<PanelController>().isDestroying;
                    if (mousePosSub.x == panelPos.x && mousePosSub.y == panelPos.y)
                    {
                        SpriteRenderer srn = panel.GetComponent<SpriteRenderer>();
                        if (!destroying)
                        {
                            if (srn.color == Color.white || srn.color == gold)
                            {
                                removeCount++;
                                if (skillCount < skillTimer)
                                {
                                    buckets[skillCount].GetComponent<Image>().color = Color.yellow;
                                }
                                skillCount++;
                                panel.GetComponent<Animator>().SetTrigger("Delete");
                                panel.GetComponent<PanelController>().isDestroying = true;
                                win = (Vector2)panel.transform.position != catPos && removeCount < catCount;
                                break;
                            }
                            else if (skillCount == skillTimer)
                            {
                                int searchNo = 0;
                                for (int i = 0; i < signal.Length; i++)
                                {
                                    if (signal[i] == panel.GetComponent<SpriteRenderer>().color)
                                    {
                                        searchNo = i;
                                        break;
                                    }
                                }
                                RemovePanel(panel);
                                SearchPanel(searchNo);
                                skillCount = 0;
                                for(int i=0;i<buckets.Length;i++)
                                {
                                    buckets[i].GetComponent<Image>().color = Color.white;
                                }
                                break;
                            }
                        }
                    }
                }
                phaseNo = win ? (int)PhaseName.初期化 : (int)PhaseName.ゲーム終了;
                win = removeCount == catCount;
                # endregion
                break;
            case 3://センサー検出
                /*bool horizontal = barH.activeSelf;
                Vector2 catPos = cat.transform.position;
                Vector2 barHPos = barH.transform.position;
                Vector2 barH2Pos = barH2.transform.position;
                Vector2 barVPos = barV.transform.position;
                Vector2 barV2Pos = barV2.transform.position;
                foreach (GameObject panel in GameObject.FindGameObjectsWithTag("Panel"))
                {
                    Vector2 panelPos = panel.transform.position;
                    if ((horizontal && (barHPos.y < catPos.y && catPos.y < barH2Pos.y) && (barHPos.y < panelPos.y && panelPos.y < barH2Pos.y))
                        || (!horizontal && (barVPos.x < catPos.x && catPos.x < barV2Pos.x) && (barVPos.x < panelPos.x && panelPos.x < barV2Pos.y)))
                    {
                        panel.GetComponent<SpriteRenderer>().color = Color.red;
                    }
                }
                phaseNo = 0;*/
                break;
            case 4://パネル点滅
                break;
            case 5://パネル初期化
                panels = GameObject.FindGameObjectsWithTag("Panel");
                catCount = panels.Length / 3;//猫候補表示数
                int panelNo;
                SpriteRenderer sr;
                counter = 0;
                for (int i = 0; i < panels.Length; i++)//猫のいるパネルとダミーパネル取得、変色
                {
                    sr = panels[i].GetComponent<SpriteRenderer>();
                    if ((cat.transform.position.x == panels[i].transform.position.x
                        && cat.transform.position.y == panels[i].transform.position.y)
                        || Random.Range(0, 2) == 2)
                    {
                        sr.color = Color.white;
                        counter++;
                    }
                    else
                    {
                        sr.color = signal[Random.Range(0, signal.Length)];
                    }
                    /*if (sr.color == Color.blue || sr.color == Color.magenta)//前段階で青、マゼンタの時
                    {
                        sr.color = panelInfo[i] ? Color.magenta : Color.cyan;
                    }
                    else
                    {
                        sr.color = panelInfo[i] ? Color.blue : Color.white;
                    }*/
                }
                while (counter < catCount)
                {
                    panelNo = Random.Range(0, panels.Length);
                    sr = panels[panelNo].GetComponent<SpriteRenderer>();
                    if (sr.color != Color.white)
                    {
                        //sr.color = sr.color == Color.cyan ? Color.magenta : Color.blue;
                        sr.color = Color.white;
                        counter++;
                    }
                }
                //SearchPanel(Random.Range(0, signal.Length));
                counter = 0;
                phaseNo = win ? (int)PhaseName.ゲーム終了 : (int)PhaseName.初期化;
                break;
            case 6://ゲーム終了
                #region ゲーム終了
                if (counter == 0)
                {
                    Debug.Log("setMessage");
                    string message;
                    if (win)
                    {
                        Destroy(GameObject.FindGameObjectWithTag("Panel"));
                        message = "You Win !";
                    }
                    else
                    {
                        message = "You Lose...";
                    }
                    text.text = message;
                    counter++;
                }
                else if (counter>10&&Input.GetMouseButtonDown(0))
                {
                    phaseNo = (int)PhaseName.リザルト;
                    counter = -1;
                }
                counter++;
                #endregion
                break;
            case 7://リザルト
                if (counter == 0)
                {
                    StageData stgData = DataController.Instance.stageData[stageNo];
                    stgData.playCount++;
                    if (win)
                    {
                        stgData.winCount++;
                        if (nowTime < stgData.bestScore || stgData.bestScore == -1)
                        {
                            stgData.bestScore = (int)nowTime;
                        }
                    }
                    string bestScoreText = stgData.bestScore == -1 ? "--" : stgData.bestScore.ToString();
                    float winPer = (float)stgData.winCount / stgData.playCount;
                    result.SetActive(true);
                    result.transform.FindChild("Win").GetComponent<Text>().text = win ? "You Win!" : "You Lose;";
                    result.transform.FindChild("Play Count").GetComponent<Text>().text = "プレイ数          "
                        + stgData.playCount.ToString() + "回";
                    result.transform.FindChild("Win Count").GetComponent<Text>().text = "勝利数              "
                        + stgData.winCount.ToString() + "回";
                    result.transform.FindChild("Win Percentage").GetComponent<Text>().text = "勝率                "
                        + winPer.ToString("0.0%");
                    result.transform.FindChild("Time").GetComponent<Text>().text = "タイム             "
                        + ((int)nowTime).ToString() + "s";
                    result.transform.FindChild("Best Time").GetComponent<Text>().text = "ベストタイム  "
                        + bestScoreText + "s";
                    counter++;
                }
                else if (counter>10&&Input.GetMouseButtonDown(0))
                {
                    counter = 0;
                    DataController.Instance.Save();
                    SceneManager.LoadScene(0);
                }
                counter++;
                break;
        }
        #endregion
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);//タイトルへ戻る
        }
        if (!win && phaseNo != (int)PhaseName.リザルト)
        {
            //nowTime = Time.time - startTime;
            nowTime += Time.deltaTime;
            time.text = /*"タイム: " + */((int)nowTime).ToString() + "s";
        }
    }

    void CatMove()
    {
        bool go = false;
        List<Vector2> directions
            = new List<Vector2> { Vector2.zero, Vector2.up, Vector2.right, Vector2.down, Vector2.left };
        List<string> comments
            = new List<string> { "その場にとどまった。", "上に動いた！", "右に動いた！", "下に動いた！", "左に動いた！" };
        while (!go && directions.Count > 0)
        {
            int rand = Random.Range(0, directions.Count);
            Vector2 pos = (Vector2)cat.transform.position + directions[rand];
            for (int i = 0; i < panels.Length; i++)
            {
                go |= (Vector2)cat.transform.position + directions[rand] == (Vector2)panels[i].transform.position;
            }
            if (go)
            {
                cat.transform.position = pos;
                text.text = "猫は" + comments[rand].Clone();
            }
            else
            {
                directions.RemoveAt(rand);
                comments.RemoveAt(rand);
            }
        }
    }

    void SearchPanel(int flagColorNo)
    {
        int count = 0;
        Vector2 catPos = cat.transform.position;
        foreach (GameObject panel in panels)
        {
            if (panel != null && (catPos - (Vector2)panel.transform.position).magnitude < 2
                && panel.GetComponent<SpriteRenderer>().color == signal[flagColorNo]
                &&!panel.GetComponent<PanelController>().isDestroying)
            {
                count++;
            }
        }
        string numText=count>0 ? /*Random.Range(1,count+1)*/count.ToString()+"枚":"0枚";
        text.text = signalName[flagColorNo] + "パネルは" + numText;
    }

    /// <summary>
    /// 色付きパネルを削除する再帰関数
    /// </summary>
    void RemovePanel(GameObject panel)
    {
        panel.GetComponent<Animator>().SetTrigger("Delete");
        panel.GetComponent<PanelController>().isDestroying = true;
        Color c = panel.GetComponent<SpriteRenderer>().color;
        foreach (GameObject p in panels)
        {
            if (panel != null && p != null)
            {
                if (p.GetComponent<SpriteRenderer>().color != Color.white)
                {
                    p.transform.FindChild("garbage").GetComponent<Animator>().SetBool("Skill", false);
                }
                if ((p.transform.position - panel.transform.position).magnitude <= 1
                    && p.GetComponent<SpriteRenderer>().color == c && !p.GetComponent<PanelController>().isDestroying)
                {
                    RemovePanel(p);
                }
            }
        }
    }
}
