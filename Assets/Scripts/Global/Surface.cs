using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Surface : MonoBehaviour
{
    public void SetSurface(int id, Vector3 center, Material[] mats, GameObject hexPrefab)
    {
        ID = id;
        initialMaterial = mats[0];
        ExpandMaterial = mats[1];
        HexPrefab = hexPrefab;
        Center = center;
        ShowInitialHexArea(Center);
    }

    private Vector3 Center;
    public int ID;
    private GameObject HexPrefab;
    private Material initialMaterial;
    private Material ExpandMaterial;
    private Vector3 lastSnappedPosition = Vector3.zero;
    public List<Vector3> initialHexes = new List<Vector3>();
    public List<Vector3> instantiatedHexes = new List<Vector3>();
    public List<GameObject> initialHexesObj = new List<GameObject>();
    public List<GameObject> instantiatedHexesObj = new List<GameObject>();
    public bool isOutsideInitialArea = false;
    private bool isDestroyed;

    public void DestroyAllHexs()
    {
        if (isDestroyed)
            return;
        StartCoroutine(DestroyAllHexesSmooth());
        isDestroyed = true;
    }

    private IEnumerator DestroyAllHexesSmooth()
    {
        print($"Player {ID} Got destroyed");
        instantiatedHexesObj.RemoveAll(hex => hex == null);
        initialHexesObj.RemoveAll(hex => hex == null);
        initialHexesObj = new List<GameObject>(initialHexesObj.OrderBy(hex => Vector3.Distance(hex.transform.position, instantiatedHexesObj.First().transform.position)));
        instantiatedHexesObj = instantiatedHexesObj.AsEnumerable().Reverse().ToList();
        List<GameObject> HexToDestroy = new List<GameObject>(instantiatedHexesObj);
        HexToDestroy.AddRange(initialHexesObj);
        float timePerHex = 0.5f / HexToDestroy.Count;
        foreach (GameObject hex in HexToDestroy)
        {
            if (hex == null)
                continue;
            Animator animator = hex.transform.Find("hex").GetComponent<Animator>();
            animator.SetTrigger("Destroy");
            Destroy(hex, 0.1f);
            yield return new WaitForSeconds(timePerHex);
        }
        instantiatedHexesObj.Clear();
        initialHexesObj.Clear();
        initialHexes.Clear();
        instantiatedHexes.Clear();
        yield return new WaitForSeconds(0.05f);
    }

    private void ShowInitialHexArea(Vector3 center)
    {
        ShowHexAtPosition(center, true);
        initialHexes.Add(center);
        foreach (Vector3 offset in SurfaceController.Instance.neighborOffsets)
        {
            ShowHexAtPosition(center + offset, true);
            initialHexes.Add(center + offset);
        }
        previousSnappedPosition = center;
    }

    private Vector3 previousSnappedPosition;

    public void SubmitStep(Vector3 snappedPosition)
    {
        if (isDestroyed)
            return;
        if (previousSnappedPosition != snappedPosition && !isOccupied(snappedPosition))
        {
            previousSnappedPosition = lastSnappedPosition;
            lastSnappedPosition = snappedPosition;
            StepEvent(snappedPosition);
        }
        else
            CheckForReconnection(snappedPosition);
    }

    public void StepEvent(Vector3 snappedPosition)
    {
        bool isOccupied = ShowHexAtPosition(snappedPosition, false);
    }

    private bool isOccupied(Vector3 snappedPosition)
    {
        float tolerance = 0.1f;
        foreach (Vector3 hexPosition in instantiatedHexes)
        {
            if (Vector3.Distance(hexPosition, snappedPosition) <= tolerance)
            {
                return true;
            }
        }
        foreach (Vector3 hexPosition in initialHexes)
        {
            if (Vector3.Distance(hexPosition, snappedPosition) <= tolerance)
            {
                return true;
            }
        }
        return false;
    }

    public bool isInsideInitialArea(Vector3 snappedPosition)
    {
        float tolerance = 0.1f;
        foreach (Vector3 hexPosition in initialHexes)
        {
            if (Vector3.Distance(hexPosition, snappedPosition) <= tolerance)
            {
                return true;
            }
        }
        return false;
    }

    public bool ShowHexAtPosition(Vector3 snappedPosition, bool isInitial = false)
    {
        if (isOccupied(snappedPosition))
            return true;
        InstantiateHex(snappedPosition, isInitial);
        instantiatedHexes.Add(snappedPosition);
        return false;
    }

    void InstantiateHex(Vector3 position, bool isInitial = false)
    {
        GameObject hex = Instantiate(HexPrefab, position, Quaternion.identity);
        hex.transform.localScale = Vector3.one * SurfaceController.Instance.HexSize;
        hex.transform.parent = this.transform;
        if (isInitial)
            initialHexesObj.Add(hex);
        else
            instantiatedHexesObj.Add(hex);
        SetHex(hex, isInitial);
    }

    public void RemoveHex(Vector3 position)
    {
        int idx = initialHexes.IndexOf(position);
        if (idx != -1)
        {
            initialHexes.RemoveAt(idx);
            Vector3 closestHexPosition = Vector3.zero;
            float minDistance = float.MaxValue;
            initialHexesObj.RemoveAll(hex => hex == null);
            foreach (GameObject hex in initialHexesObj)
            {
                float distance = Vector3.Distance(hex.transform.position, position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestHexPosition = hex.transform.position;
                }
            }
            GameObject closesthex = initialHexesObj.Find(hex => hex.transform.position == closestHexPosition);
            initialHexesObj.Remove(closesthex);
            Destroy(closesthex);
        }
    }

    public void SetHex(GameObject hex, bool isInitial)
    {
        Transform hexTransform = hex.transform.Find("hex");
        Animator animator = hexTransform.GetComponent<Animator>();
        hexTransform.localPosition = Vector3.zero + new Vector3(0, isInitial ? 0 : -0.15f, 0);
        hexTransform.Find("surface").gameObject.SetActive(!isInitial);
        MeshRenderer hexRenderer = hexTransform.Find("mesh").GetComponent<MeshRenderer>();
        hexRenderer.sharedMaterial = isInitial ? initialMaterial : ExpandMaterial;
        animator.SetTrigger("Pop");
    }

    private void CheckForReconnection(Vector3 hexPosition)
    {
        if (instantiatedHexes.Count > 0)
        {
            if (initialHexes.Contains(hexPosition))
            {
                if (isOutsideInitialArea)
                {
                    MergeHexAreas();
                    isOutsideInitialArea = false;
                }
            }
            else
            {
                isOutsideInitialArea = true;
            }
        }
    }

    private void MergeHexAreas()
    {
        foreach (Vector3 hex in instantiatedHexes)
        {
            initialHexes.Add(hex);
        }
        CoverFreeArea();
        foreach (GameObject hex in instantiatedHexesObj)
        {
            initialHexesObj.Add(hex);
            SetHex(hex, true);
        }
        instantiatedHexesObj.Clear();
    }

    private void CoverFreeArea()
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
        int coverErrorMarging = 10;
        foreach (Vector3 hex in instantiatedHexes)
        {
            minX = Mathf.Min(minX, hex.x);
            maxX = Mathf.Max(maxX, hex.x);
            minZ = Mathf.Min(minZ, hex.z);
            maxZ = Mathf.Max(maxZ, hex.z);
        }
        List<Vector3> checkedHexes = new List<Vector3>();
        for (float x = minX; x <= maxX; x += SurfaceController.Instance.hexWidth)
        {
            for (float z = maxZ; z >= minZ; z -= SurfaceController.Instance.hexHeight)
            {
                Vector3 position = SurfaceController.Instance.SnapToHexGrid(new Vector3(x, 0, z));
                if (isOccupied(position))
                    continue;
                bool isInside = true;
                foreach (Vector3 offset in SurfaceController.Instance.neighborOffsets)
                {
                    Vector3 currentHexPosition = position;
                    int maxIterations = 10;
                    for (int i = 1; i <= maxIterations; i++)
                    {
                        currentHexPosition += offset;
                        if (checkedHexes.Contains(currentHexPosition))
                            break;
                        if (isOccupied(currentHexPosition))
                        {
                            for (int u = 1; u < i; u++)
                            {
                                Vector3 offsitPosition = position + offset * u;
                                checkedHexes.Add(offsitPosition);
                                ShowHexAtPosition(offsitPosition, true);
                            }
                            break;
                        }
                        if (currentHexPosition.x < minX || currentHexPosition.x > maxX || currentHexPosition.z < minZ || currentHexPosition.z > maxZ)
                        {
                            isInside = false;
                            break;
                        }
                        if (i == maxIterations)
                        {
                            isInside = false;
                        }
                    }
                    if (!isInside)
                        break;
                }
                if (isInside)
                    ShowHexAtPosition(position, true);
            }
        }
        instantiatedHexes.Clear();
    }



    // IEnumerator CoverFreeAreaDebug()
    // {
    //     float minX = float.MaxValue;
    //     float maxX = float.MinValue;
    //     float minZ = float.MaxValue;
    //     float maxZ = float.MinValue;

    //     int coverErrorMarging = 10;
    //     // Determine the min/max bounds of the grid
    //     foreach (Vector3 hex in instantiatedHexes)
    //     {
    //         minX = Mathf.Min(minX, hex.x);
    //         maxX = Mathf.Max(maxX, hex.x);
    //         minZ = Mathf.Min(minZ, hex.z);
    //         maxZ = Mathf.Max(maxZ, hex.z);
    //     }

    //     List<Vector3> checkedHexes = new List<Vector3>();

    //     // Iterate through each hex in the grid
    //     for (float x = minX; x <= maxX; x += SurfaceController.Instance.hexWidth)
    //     {
    //         for (float z = maxZ; z >= minZ; z -= SurfaceController.Instance.hexHeight)
    //         {

    //             Vector3 position = SurfaceController.Instance.SnapToHexGrid(new Vector3(x, 0, z));

    //             if (isOccupied(position))
    //                 continue;

    //             DebugHex(position, true);
    //             bool isInside = true;

    //             // Check each of the 6 directions
    //             foreach (Vector3 offset in SurfaceController.Instance.neighborOffsets)
    //             {
    //                 Vector3 currentHexPosition = position;
    //                 int maxIterations = 10;  // Fixed max iteration limit

    //                 // Check in the current direction for up to maxIterations hexes
    //                 for (int i = 1; i <= maxIterations; i++)
    //                 {
    //                     currentHexPosition += offset;
    //                     //Vector3 snappedOffsetPosition = SurfaceController.Instance.SnapToHexGrid(currentHexPosition);

    //                     // Skip if already checked
    //                     if (checkedHexes.Contains(currentHexPosition))
    //                         break;

    //                     if (isOccupied(currentHexPosition))
    //                     {
    //                         for (int u = 1; u < i; u++)
    //                         {
    //                             Vector3 offsitPosition = position + offset * u;
    //                             checkedHexes.Add(offsitPosition);
    //                             ShowHexAtPosition(offsitPosition, true);
    //                         }
    //                         // If we find an occupied hex in this direction, move to the next direction
    //                         break;
    //                     }

    //                     yield return new WaitForSeconds(.1f);

    //                     DebugHex(currentHexPosition, false);
    //                     // Mark as checked

    //                     // Check if the current position is out of bounds
    //                     if (currentHexPosition.x < minX || currentHexPosition.x > maxX || currentHexPosition.z < minZ || currentHexPosition.z > maxZ)
    //                     {
    //                         isInside = false;
    //                         break;
    //                     }

    //                     // If no hexes are occupied after checking maxIterations, it's not inside bounds
    //                     if (i == maxIterations)
    //                     {
    //                         isInside = false;
    //                     }
    //                 }

    //                 if (!isInside)
    //                     break;
    //             }

    //             // If the position is still considered inside after checking all directions
    //             if (isInside)
    //                 ShowHexAtPosition(position, true);

    //             ResetDebug();
    //         }
    //     }

    //     yield return new WaitForSeconds(0.01f); // to avoid crashing the coroutine
    //     instantiatedHexes.Clear();
    // }

    // private void DebugHex(Vector3 position, bool isInitial = false)
    // {
    //     InstantiateHexDebug(position, isInitial);
    // }

    // void InstantiateHexDebug(Vector3 position, bool isInitial = false)
    // {
    //     GameObject hex = Instantiate(HexPrefab, position, Quaternion.identity);
    //     hex.transform.localScale = Vector3.one * SurfaceController.Instance.HexSize;
    //     hex.transform.parent = this.transform; // Set the parent to keep hierarchy clean
    //     debugHex.Add(hex);
    //     hex.transform.Find("hex/mesh").GetComponent<MeshRenderer>().material = isInitial ? debugmaterail1 : debugmaterail2;

    // }
    // public Material debugmaterail1;
    // public Material debugmaterail2;
    // private void ResetDebug()
    // {
    //     foreach (GameObject hex in debugHex)
    //     {
    //         Destroy(hex);
    //     }
    //     debugHex.Clear();
    // }
    // private List<GameObject> debugHex = new List<GameObject>();
}