using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    float correctionSize = 0.5f;
    [SerializeField]
    int size;
    [SerializeField]
    GameObject barH, barH2, barV, barV2, cat;
    [SerializeField]
    Text text, turn;
    int turnCount;
    int phaseNo;//操作フェイズ
    enum PhaseName
    {
        初期化 = 0, センサー設置, パネル除去, センサー検出, 終了判定, 猫移動,ゲーム終了
    }
    int counter;
    bool mouseStay;//ボタンを押しっぱなしにしているか
    bool win;//猫発見できたかどうか
    GameObject[] panels;
    bool[] panelInfo, panelInfoSub;//パネルの色情報、白でなければtrue subは前の状態
    bool start;
    Vector2 mousePosSub;//マウスクリック座標(正規化後)

    // Use this for initialization
    void Start()
    {
        correctionSize = size % 2 == 0 ? 0 : 0.5f;
        phaseNo = (int)PhaseName.猫移動;
        mouseStay = false;
        float catCorrect = 0.5f;
        cat.transform.position = new Vector2(Random.Range(0, size) - size * 0.5f + catCorrect,
            Random.Range(0, size) - size * 0.5f + catCorrect);
        start = true;
        turnCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (phaseNo)
        {
            case 0://初期化
                /*if (Input.GetMouseButtonDown(0))
                {
                    /*foreach (GameObject panel in GameObject.FindGameObjectsWithTag("Panel"))
                    {
                        panel.GetComponent<SpriteRenderer>().color = Color.white;
                    }*/
                    barH.SetActive(false);
                    barH2.SetActive(false);
                    barV.SetActive(false);
                    barV2.SetActive(false);
                    phaseNo = 1;
                //}
                text.text += "\n消すパネルを選んでください。";
                turnCount++;
                turn.text = "ターン : "+turnCount.ToString();
                break;
            case 1:
                #region センサー設置
                if (Input.GetMouseButtonDown(0))//ボタン押下、一本目のバーを設置
                {
                    Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);//タッチ座標
                    float sizeHalf = size * 0.5f;
                    float barCorrection = 0.5f;
                    mousePosSub = Vector2.zero;
                    if (pos.x < -sizeHalf && Mathf.Abs(pos.y) < sizeHalf - barCorrection)//水平バー設置
                    {
                        barH.SetActive(true);
                        barH2.SetActive(true);
                        barH.transform.position
                            = new Vector2(barH.transform.position.x, Mathf.Round(pos.y - correctionSize) + correctionSize);
                        barV.SetActive(false);
                        barV2.SetActive(false);
                        mouseStay = true;
                    }
                    else if (pos.y < -sizeHalf && Mathf.Abs(pos.x) < sizeHalf - barCorrection)//垂直バー設置
                    {
                        barV.SetActive(true);
                        barV2.SetActive(true);
                        barV.transform.position
                            = new Vector2(Mathf.Round(pos.x - correctionSize) + correctionSize, barV.transform.position.y);
                        barH.SetActive(false);
                        barH2.SetActive(false);
                        mouseStay = true;
                    }
                    else if ((Mathf.Abs(pos.x) < sizeHalf && (Mathf.Abs(pos.y) < sizeHalf)))
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
                        if (barH2.activeSelf)//水平バー設置
                        {
                            if (Mathf.Abs(pos.y) > sizeHalf)
                            {
                                pos = new Vector2(pos.x, pos.y / Mathf.Abs(pos.y) * sizeHalf);
                            }
                            barH2.transform.position
                                = new Vector2(barH.transform.position.x, Mathf.Round(pos.y - correctionSize) + correctionSize);
                            if (barH.transform.position.y > barH2.transform.position.y)
                            {
                                Vector2 posSub = barH.transform.position;
                                barH.transform.position = barH2.transform.position;
                                barH2.transform.position = posSub;
                            }
                            else if (barH.transform.position == barH2.transform.position)
                            {
                                barH.SetActive(false);
                                barH2.SetActive(false);
                                phaseNo = (int)PhaseName.センサー設置;
                            }
                            mouseStay = false;
                        }
                        else if (barV2.activeSelf)//垂直バー設置
                        {
                            if (Mathf.Abs(pos.x) > sizeHalf)
                            {
                                pos = new Vector2(pos.x / Mathf.Abs(pos.x) * sizeHalf, pos.y);
                            }
                            barV2.transform.position
                                = new Vector2(Mathf.Round(pos.x - correctionSize) + correctionSize, barV.transform.position.y);
                            if (barV2.transform.position.x < barV.transform.position.x)
                            {
                                Vector2 posSub = barV.transform.position;
                                barV.transform.position = barV2.transform.position;
                                barV2.transform.position = posSub;
                            }
                            else if (barV.transform.position == barV2.transform.position)
                            {
                                barV.SetActive(false);
                                barV2.SetActive(false);
                                phaseNo = (int)PhaseName.センサー設置;
                            }
                            mouseStay = false;
                        }
                        else if (mousePosSub != Vector2.zero)
                        {
                            Vector2 posSub = new Vector2(Mathf.Round(pos.x - correctionSize - 0.5f) + 0.5f + correctionSize,
                                Mathf.Round(pos.y - correctionSize - 0.5f) + 0.5f + correctionSize);
                            if (mousePosSub != posSub)
                            {
                                phaseNo = (int)PhaseName.センサー設置;
                            }
                        }
                    }
                    else//ボタン押し中、2本目バーの座標をマウスに追随させる
                    {
                        if (barH2.activeSelf)
                        {
                            if (Mathf.Abs(pos.y) > sizeHalf)
                            {
                                pos = new Vector2(pos.x, pos.y / Mathf.Abs(pos.y) * sizeHalf);
                            }
                            barH2.transform.position = new Vector2(barH2.transform.position.x, pos.y);
                        }
                        else if (barV2.activeSelf)
                        {
                            if (Mathf.Abs(pos.x) > sizeHalf)
                            {
                                pos = new Vector2(pos.x / Mathf.Abs(pos.x) * sizeHalf, pos.y);
                            }
                            barV2.transform.position = new Vector2(pos.x, barV2.transform.position.y);
                        }
                    }
                }
                #endregion
                break;
            case 2://パネル除去
                #region パネル除去
                bool horizontal = barH.activeSelf;
                bool one = !barV.activeSelf;
                Vector2 catPos = cat.transform.position;
                Vector2 barHPos = barH.transform.position;
                Vector2 barH2Pos = barH2.transform.position;
                Vector2 barVPos = barV.transform.position;
                Vector2 barV2Pos = barV2.transform.position;
                int countLimit = 50;
                if (counter == 0)
                {
                    win = true;
                }
                foreach (GameObject panel in GameObject.FindGameObjectsWithTag("Panel"))
                {
                    Vector2 panelPos = panel.transform.position;
                    if ((!horizontal && one && mousePosSub.x == panelPos.x && mousePosSub.y == panelPos.y)
                            || (horizontal && (barHPos.y < panelPos.y && panelPos.y < barH2Pos.y))
                    || (!one && (barVPos.x < panelPos.x && panelPos.x < barV2Pos.x)))
                    {
                        if (counter == 0)
                        {
                            panel.GetComponent<SpriteRenderer>().color = Color.red;
                            if (panelPos.x == catPos.x && panelPos.y == catPos.y)//猫が露見したら
                            {
                                win = false;
                                cat.GetComponent<SpriteRenderer>().color = Color.red;
                            }
                        }
                        else if (counter < countLimit)
                        {
                            panel.transform.localScale *= 0.9f;
                        }
                        else
                        {
                            Destroy(panel);
                        }
                    }
                }
                counter++;
                if (counter > countLimit)
                {
                    counter = 0;
                    phaseNo = win ? (int)PhaseName.初期化 : (int)PhaseName.ゲーム終了;
                }
                Debug.Log(counter);
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
            case 4://終了判定
                break;
            case 5://猫移動
                panels = GameObject.FindGameObjectsWithTag("Panel");
                int catCount = panels.Length / 3;//猫候補表示数
                int panelNo;
                SpriteRenderer sr;
                counter = 0;
                win = panels.Length == 1;//パネル数が1つなら、勝利
                panelInfo = new bool[panels.Length];
                panelInfoSub = new bool[panels.Length];
                if (!start&&!win)//猫移動
                {
                    CatMove();
                }
                else
                {
                    start = false;
                }
                for (int i = 0; i < panels.Length; i++)//猫のいるパネルとダミーパネル取得、変色
                {
                    sr = panels[i].GetComponent<SpriteRenderer>();
                    panelInfoSub[i] = sr.color == Color.blue || sr.color == Color.magenta;
                    if ((cat.transform.position.x == panels[i].transform.position.x
                        && cat.transform.position.y == panels[i].transform.position.y)
                        || Random.Range(0, 2) == 2)
                    {
                        panelInfo[i] = true;
                        counter++;
                    }
                    else
                    {
                        panelInfo[i] = false;
                    }
                    /*if (sr.color == Color.blue || sr.color == Color.magenta)//前段階で青、マゼンタの時
                    {
                        sr.color = panelInfo[i] ? Color.magenta : Color.cyan;
                    }
                    else
                    {
                        sr.color = panelInfo[i] ? Color.blue : Color.white;
                    }*/
                    if(panelInfo[i])
                    {
                        sr.color = Color.blue;
                    }
                    else if(panelInfoSub[i])
                    {
                        sr.color = Color.cyan; 
                    }
                    else
                    {
                        sr.color = Color.white;
                    }
                }
                while (counter < catCount)
                {
                    panelNo = Random.Range(0, panels.Length);
                    sr = panels[panelNo].GetComponent<SpriteRenderer>();
                    if (sr.color != Color.blue&&sr.color!=Color.magenta)
                    {
                        panelInfo[panelNo] = true;
                        //sr.color = sr.color == Color.cyan ? Color.magenta : Color.blue;
                        sr.color = Color.blue;
                        counter++;
                    }
                }
                counter = 0;
                phaseNo = win ? (int)PhaseName.ゲーム終了 : (int)PhaseName.初期化;
                break;
            case 6://ゲーム終了
                #region ゲーム終了
                Debug.Log("last");
                if (counter == 0)
                {
                    Debug.Log("setMessage");
                    string message;
                    if (win)
                    {
                        Destroy(GameObject.FindGameObjectWithTag("Panel"));
                        message= "You Win !";
                    }
                    else
                    {
                        message = "You Lose...";
                    }
                    text.text = message;
                    counter++;
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    //phaseNo = (int)PhaseName.初期化;
                    SceneManager.LoadScene(0);
                    counter = 0;
                }
                #endregion
                break;
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);//タイトルへ戻る
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

    public void DisplayLastPanel()
    {
        if (panels != null && panelInfoSub != null)
        {
            for (int i = 0; i < panelInfoSub.Length; i++)
            {
                Color c = panelInfoSub[i] ? Color.cyan : Color.white;
                panels[i].GetComponent<SpriteRenderer>().color = c;
            }
        }
    }

    public void CloseLastPanel()
    {
        if (panels != null && panelInfo != null)
        {
            for (int i = 0; i < panelInfo.Length; i++)
            {
                Color c = panelInfo[i] ? Color.blue : Color.white;
                panels[i].GetComponent<SpriteRenderer>().color = c;
            }
        }
    }
}
