using UnityEngine;

public class IgnorePlayerPrincessCollision : MonoBehaviour
{
    void Start()
    {
        // 이 스크립트는 플레이어나 공주에 붙여서 사용합니다.
        // 현재 오브젝트의 태그에 따라 다른 오브젝트를 찾아 충돌 무시 처리합니다.
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
            return;
        
        if (gameObject.CompareTag("Player"))
        {
            GameObject princessObj = GameObject.FindGameObjectWithTag("Princess");
            if (princessObj != null)
            {
                Collider2D princessCollider = princessObj.GetComponent<Collider2D>();
                if (princessCollider != null)
                {
                    Physics2D.IgnoreCollision(myCollider, princessCollider, true);
                }
            }
        }
        else if (gameObject.CompareTag("Princess"))
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
                if (playerCollider != null)
                {
                    Physics2D.IgnoreCollision(myCollider, playerCollider, true);
                }
            }
        }
    }
}
