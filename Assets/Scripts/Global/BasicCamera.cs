using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCamera : MonoBehaviour
{
     public static BasicCamera Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private Transform player;
    public Vector3 offset;

    public float smoothSpeed = 0.125f;
    private Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    public void SetCamera(Transform player) { this.player = player; }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;
        Vector3 targetPosition = player.position + offset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothSpeed);
    }
}
