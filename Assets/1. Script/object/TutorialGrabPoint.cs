using UnityEngine;

public class TutorialGrabPoint : MonoBehaviour
{
    void OnDrawGizmos() 
        => Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);

    void OnTriggerEnter2D(Collider2D col)
{
    if (col.CompareTag("Princess"))
    {
        StorySceneManager.Instance.SetInsideGrabPoint(true);
        StorySceneManager.Instance.ShowTutorialMessage("스페이스바를 눌러 시간을 해제하세요!");
    }
}

void OnTriggerExit2D(Collider2D col)
{
    if (col.CompareTag("Princess"))
    {
        StorySceneManager.Instance.SetInsideGrabPoint(false);
        StorySceneManager.Instance.ShowTutorialMessage("여기가 아니에요!");
    }
}

}
