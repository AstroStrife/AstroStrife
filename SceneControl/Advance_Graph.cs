using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Advance_Graph : MonoBehaviour
{
    public static Advance_Graph Instance { get; private set; }
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private TMP_Dropdown dropdown;
    private RectTransform graphContainer;
    private RectTransform labelX;
    private RectTransform labelY;
    private List<GameObject> gameObjectList;

    public List<LogEntry> onSelectPlayer = new List<LogEntry>();
    public List<LogEntry> onShowGraphPlayerbyGold = new List<LogEntry>();
    public List<int> onGetGoldPerMin = new List<int>();

    private void Awake() {
        Instance = this;

        graphContainer = transform.Find("Graph").GetComponent<RectTransform>();
        labelX = graphContainer.Find("LabelX").GetComponent<RectTransform>();
        labelY = graphContainer.Find("LabelY").GetComponent<RectTransform>();

        gameObjectList = new List<GameObject>();

        //List<int> valuelist = new List<int>() {5 , 10 ,15 ,12 ,14};

        //ShowGraph(valuelist, -1, (int _i) => "" +(_i+1) , (float _f) => "" + Mathf.RoundToInt(_f));
        
    }
    public void SetUsername(string name, bool actionShowPlayerName){
        if(actionShowPlayerName){
            dropdown.GetComponent<DropdownSelection>().ShowplayerName(name);
        }
        ShowGraph(GetGoldPerMin(ShowGraphPlayerbyGold(SelectPlayer(name))), -1, (int _i) => "" +(_i+1) , (float _f) => "" + Mathf.RoundToInt(_f));
    }
    
    public List<LogEntry> SelectPlayer(string name){
        List<LogEntry> logEntries = GameLogger.Instance.GetLog();
        List<LogEntry> selectplayerlog = new List<LogEntry>();
        foreach(LogEntry logentry in logEntries){
            if(logentry.subject == name){
                selectplayerlog.Add(logentry);
            }
        }
        return selectplayerlog;
    }
    public List<LogEntry> ShowGraphPlayerbyGold(List<LogEntry> logEntries){
        List<LogEntry> selectplayerlog = new List<LogEntry>();
        foreach(LogEntry logentry in logEntries){
            if(logentry.action == " Report Gold Per min : "){
                selectplayerlog.Add(logentry);
            }
        }
        return selectplayerlog;
    }

    private List<int> GetGoldPerMin(List<LogEntry> logEntries){
        List<int> goldPerMin = new List<int>();
        foreach(LogEntry logentry in logEntries){
            goldPerMin.Add(int.Parse(logentry.target));
        }
        return goldPerMin;
    }

    private GameObject CreateCircle(Vector2 vector2)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = vector2;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    public void ShowGraph(List<int> valuelist, int maxVisibleValueAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        if(getAxisLabelX == null){
            getAxisLabelX = delegate (int _i) {return _i.ToString();} ;
        }
        if(getAxisLabelY == null){
            getAxisLabelY = delegate (float _f) {return Mathf.RoundToInt(_f).ToString();} ;
        }

        if (maxVisibleValueAmount <= 0) {
            maxVisibleValueAmount = valuelist.Count;
        }

        foreach (GameObject gameObject in gameObjectList) {
            Destroy(gameObject);
        }
        gameObjectList.Clear();
        

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        float yMaximum = valuelist[0];
        float yMinimum = valuelist[0];

        for (int i = Mathf.Max(valuelist.Count - maxVisibleValueAmount, 0); i < valuelist.Count; i++) {
            int value = valuelist[i];
            if (value > yMaximum) {
                yMaximum = value;
            }
            if (value < yMinimum) {
                yMinimum = value;
            }
        }

        float yDifference = yMaximum - yMinimum;
        if (yDifference <= 0) {
            yDifference = 5f;
        }
        yMaximum = yMaximum + (yDifference * 0.2f);
        yMinimum = yMinimum - (yDifference * 0.2f);

        yMinimum = 0f; // Start the graph at zero

        float xSize = graphWidth / (maxVisibleValueAmount + 1);

        int xIndex = 0;

        GameObject lastCircleGameObject = null;

        for (int i = Mathf.Max(valuelist.Count - maxVisibleValueAmount, 0); i < valuelist.Count; i++) {
            float xPosition = xSize + xIndex * xSize;
            float yPosition = ((valuelist[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;
            GameObject circleObject = CreateCircle(new Vector2(xPosition,yPosition));
            gameObjectList.Add(circleObject);
            if(lastCircleGameObject != null){
                GameObject ConnectionGameObject = CreateConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add(ConnectionGameObject);
            }
            lastCircleGameObject = circleObject;

            RectTransform label_X = Instantiate(labelX);
            label_X.SetParent(graphContainer, false);
            label_X.gameObject.SetActive(true);
            label_X.anchoredPosition = new Vector2(xPosition, -7f);
            label_X.GetComponent<Text>().text = getAxisLabelX(i);
            gameObjectList.Add(label_X.gameObject);

            xIndex++;
        }

        int separatorCount = 10;
        for(int i = 0 ; i < separatorCount; i++){
            RectTransform label_Y = Instantiate(labelY);
            label_Y.SetParent(graphContainer, false);
            label_Y.gameObject.SetActive(true);
            float normalizedValue = i * 1f / separatorCount;
            label_Y.anchoredPosition = new Vector2(-7f, normalizedValue * graphHeight);
            label_Y.GetComponent<Text>().text = getAxisLabelY( yMinimum + ( normalizedValue * ( yMaximum - yMinimum )));
            gameObjectList.Add(label_Y.gameObject);
        }



    }

    private GameObject CreateConnection(Vector2 PositionA , Vector2 PositionB){
        GameObject gameObject = new GameObject("dotConnection" , typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1,1,1, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (PositionB - PositionA).normalized;
        float distance = Vector2.Distance(PositionA, PositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = PositionA + dir * distance * .5f;

        rectTransform.localEulerAngles = new Vector3(0,0, GetAngleFromVectorFloat(dir));
        return gameObject;
    }
    public static float GetAngleFromVectorFloat(Vector3 dir) {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;

            return n;
    }
}
