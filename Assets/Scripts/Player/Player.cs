using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform playerTransform;
    public Animator playerAnimator;
    public SkinnedMeshRenderer playerSkinnedMeshRenderer;
    public int playerID;
    public float runSpeed = 4.5f;
    public float rotationSpeed = 5f;

    public bool inGame = false;
    private bool IsLost;
    public void SetRunAnimation(bool isRunning)
    {
        if (playerAnimator.GetBool("Run") != isRunning)
            playerAnimator.SetBool("Run", isRunning);
    }
    public void SetLoseAnimation()
    {
        playerAnimator.SetTrigger("Fall");
    }

    void Update()
    {
        if (!inGame || IsLost)
            return;
        SurfaceController.Instance.SubmitStep(playerTransform.position, playerID);
    }

    public void StartGame()
    {
        inGame = true;
    }
    public void LoseGame()
    {
        if(IsLost)
        return;
        SetLoseAnimation();
        Destroy(gameObject, 3f);
        IsLost = true;
        inGame = false;
    }
    public void Set(int id, bool isAi = false)
    {
        playerID = id;
        Color playerColor = GameManager.Instance.GetColor(playerID);
        Material playerMaterial = new Material(playerSkinnedMeshRenderer.material);
        playerMaterial.color = playerColor;
        playerSkinnedMeshRenderer.material = playerMaterial;

        if (isAi)
            gameObject.AddComponent<AIController>();
        else
            gameObject.AddComponent<Movement>();

    }
}
