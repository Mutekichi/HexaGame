using UnityEngine;
using UnityEngine.SceneManagement;

public class ColliderButtonManager : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    private PolygonCollider2D polygonCollider;

    private void Start()
    {
        // アタッチされているPolygonCollider2Dを取得
        polygonCollider = GetComponent<PolygonCollider2D>();

        if (polygonCollider == null)
        {
            Debug.LogError("PolygonCollider2D が見つかりません: " + gameObject.name);
        }
    }

    private void OnMouseDown()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("遷移先のシーン名が設定されていません: " + gameObject.name);
        }
    }
}