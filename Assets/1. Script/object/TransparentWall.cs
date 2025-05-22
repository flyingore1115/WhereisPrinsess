using UnityEngine;

public class TransparentWall : MonoBehaviour
{
    private Collider2D wallCollider;

    void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
        if (wallCollider == null)
        {
            Debug.LogError("[TransparentWall] Collider2D 없음!");
        }
    }

    void Update()
    {
        if (StorySceneManager.Instance != null && StorySceneManager.Instance.isAttackTutorialComplete)
        {
            Destroy(gameObject);
        }
    }
}
