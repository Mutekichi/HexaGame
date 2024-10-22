using UnityEditor.Rendering;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public int stageNumber;
    public Object AnswerBoardPrefab;
    public Object PlayerBoardPrefab;
    void Start()
    {
        
        PlacePlayerBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlacePlayerBoard()
    {
        GameObject playerBoard = GameObject.Find("PlayerBoardInstance");
        Debug.Log(playerBoard ? "PlayerBoardInstance found" : "PlayerBoardInstance not found");

        if (playerBoard == null)
        {
            playerBoard = Instantiate(PlayerBoardPrefab) as GameObject;
            playerBoard.name = "PlayerBoardInstance";
        }
        
    }
}