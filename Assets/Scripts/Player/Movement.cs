using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Player player;
    private float startX;
    private float startY;
    private bool isPressing = false;
    
#if UNITY_EDITOR
    private bool isMobile = false;
#else
    private bool isMobile = false;
#endif

    private void Awake()
    {
        player = GetComponent<Player>();
    }
    // Update is called once per frame
    void Update()
    {
        if(player.inGame == false)
            return;
        MovementControll();
        RotationControll();

    }

    private void MovementControll()
    {
        player.SetRunAnimation(isPressing);

        if (!isPressing)
            return;

        transform.position += transform.forward * player.runSpeed * Time.deltaTime;
    }
    private void RotationControll()
    {
        if (!isMobile)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startX = Input.mousePosition.x;
                startY = Input.mousePosition.y;
                isPressing = true;
            }
            else if (Input.GetMouseButton(0))
            {
                if (isPressing)
                {
                    float angle = Mathf.Atan2(Input.mousePosition.x - startX, Input.mousePosition.y - startY) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), player.rotationSpeed * Time.deltaTime);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isPressing = false;
            }
        }

    }
}
