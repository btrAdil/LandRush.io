using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private Player player;

    private Vector3 desiredPosition = Vector3.zero;
    private bool isReturningToSafeZone = false;

    private float boundarySize = 10;
    private float minMoveDistance = 5f;
    private float DistThreshold = 0.5f;

    private float minDistance = 2f;
    private float maxDistance = 8f;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void SetNewDesiredPosition()
    {
        if (isReturningToSafeZone)
        {
            desiredPosition = FindClosestHexInArea(transform.position);
        }
        else
        {
            desiredPosition = FindNewTarget();
        }

        player.SetRunAnimation(true);
    }

    private void MoveToDesiredPosition()
    {
        if (!player.inGame)
            return;

        if (desiredPosition == Vector3.zero || Vector3.Distance(transform.position, desiredPosition) < minMoveDistance)
        {
            SetNewDesiredPosition();
        }

        Vector3 direction = desiredPosition - transform.position;
        direction.y = 0; // Stay grounded
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), player.rotationSpeed * Time.deltaTime);
        transform.position += transform.forward * player.runSpeed * Time.deltaTime;

        if (direction.magnitude < DistThreshold)
        {
            if (SurfaceController.Instance.isOutsideArea(player.playerID))
            {
                isReturningToSafeZone = true;
                SetNewDesiredPosition();
            }
            else
            {
                isReturningToSafeZone = false;
                SetNewDesiredPosition();
            }
        }
    }

    private Vector3 FindNewTarget()
    {
        Vector3 expansionTarget = Vector3.zero;
       

        for (int attempt = 0; attempt < 5; attempt++)
        {
            float randomDistance = Random.Range(minDistance, maxDistance);
            float randomAngle = Random.Range(0, 360);
            Vector3 randomOffset = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)) * randomDistance;

            Vector3 candidatePosition = FindFarestHexInTerritory(transform.position) + randomOffset;

            if (!IsOutOfBounds(candidatePosition))
            {
                return candidatePosition;
            }
        }

        return SnapToBoundaryEdge();
    }

    private Vector3 FindClosestHexInArea(Vector3 currentPosition)
    {
        Vector3 closestHex = Vector3.zero;
        float minDistance = float.MaxValue;

        foreach (Vector3 hex in SurfaceController.Instance.GetTerritoryHexes(player.playerID))
        {
            float distance = Vector3.Distance(currentPosition, hex);
            if (distance < minDistance)
            {
                closestHex = hex;
                minDistance = distance;
            }
        }

        return closestHex;
    }
    private Vector3 FindFarestHexInTerritory(Vector3 currentPosition)
    {
        Vector3 farestHex = Vector3.zero;
        float maxDistance = float.MinValue;

        foreach (Vector3 hex in SurfaceController.Instance.GetTerritoryHexes(player.playerID))
        {
            float distance = Vector3.Distance(currentPosition, hex);
            if (distance > maxDistance)
            {
                farestHex = hex;
                maxDistance = distance;
            }
        }

        return farestHex;
    }

    private bool IsOutOfBounds(Vector3 position)
    {
        return position.x < -boundarySize / 2 || position.x > boundarySize / 2 ||
               position.z < -boundarySize / 2 || position.z > boundarySize / 2;
    }

    private Vector3 SnapToBoundaryEdge()
    {
        Vector3 position = transform.position;
        float clampedX = Mathf.Clamp(position.x, -boundarySize / 2, boundarySize / 2);
        float clampedZ = Mathf.Clamp(position.z, -boundarySize / 2, boundarySize / 2);
        return new Vector3(clampedX, position.y, clampedZ);
    }

    private void Update()
    {
        MoveToDesiredPosition();
    }
}
