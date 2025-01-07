using System.Collections;
using UnityEngine;

public class PlayerOver : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public Camera mainCamera;
    public Transform princess;

    private bool isDisabled = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage()
    {
        if (isDisabled) return;

        currentHealth--;
        if (currentHealth <= 0)
        {
            DisablePlayer();
        }
    }

    private void DisablePlayer()
    {
        isDisabled = true;
        Debug.Log("Player is disabled!");

        // 카메라 공주로 이동
        StartCoroutine(MoveCameraToPrincess());
    }

    private IEnumerator MoveCameraToPrincess()
    {
        while (Vector3.Distance(mainCamera.transform.position, princess.position + new Vector3(0, 0, -10)) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, princess.position + new Vector3(0, 0, -10), Time.deltaTime);
            yield return null;
        }
    }
}
