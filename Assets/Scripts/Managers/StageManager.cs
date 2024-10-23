using UnityEditor.Rendering;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public int stageNumber;
    public GameObject AnswerBoardPrefab;
    public GameObject PlayerBoardPrefab;
    public GameObject TriangleTilePrefab;
    public GameObject PlayerBoardInstance;
    void Start()
    {
        
        // PlacePlayerBoard();
        FindPlayerBoard();
        PlaceTriangleTile(new Vector3(0, 0, 0));
        PlaceTriangleTile(new Vector3(0, 1, 0), 0.5f, false, false);
        StageLogic.Test();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlacePlayerBoard()
    {
        GameObject playerBoard = GameObject.Find("PlayerBoardInstance");
        Debug.Log(playerBoard ? "PlayerBoardInstance found" : "PlayerBoardInstance not found");

        if (playerBoard != null)
        {
            playerBoard = Instantiate(PlayerBoardPrefab) as GameObject;
            playerBoard.name = "PlayerBoardInstance";
        }
    }

    private void FindPlayerBoard()
    {
        GameObject playerBoard = GameObject.Find("PlayerBoardInstance");
        Debug.Log(playerBoard ? "PlayerBoardInstance found" : "PlayerBoardInstance not found");
    }

    private void PlaceTriangleTile(Vector3 position, float scale = 1f, bool isUpward = true, bool isFront = true)
{
   GameObject triangleTile = Instantiate(TriangleTilePrefab, PlayerBoardInstance.transform);
   triangleTile.transform.localPosition = position;
   triangleTile.transform.localScale = new Vector3(scale, scale, 1);

   TriangleTileBehaviour tileBehaviour = triangleTile.GetComponent<TriangleTileBehaviour>();
   if (tileBehaviour != null)
   {
       tileBehaviour.isUpward = isUpward;
       tileBehaviour.isFront = isFront;
   }
}
}