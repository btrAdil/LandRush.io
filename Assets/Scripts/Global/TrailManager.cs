using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailManager : MonoBehaviour
{
    public GameObject stepPrefab;
    public float distanceBetweenSteps = 0.5f;

    public float stepSize=1f;

    private Vector3 LastStepPosition;
    private void Update()
    {
        if (Vector3.Distance(transform.position, LastStepPosition) > distanceBetweenSteps)
        {
            CreateStep();
        }
    }

    private void CreateStep()
    {
        GameObject newStep = Instantiate(stepPrefab, transform.position, transform.rotation);
        newStep.transform.localScale = Vector3.one * stepSize;
        LastStepPosition=transform.position;
        GameManager.Instance.MoveToTrash(newStep.transform);
        
    }

}