using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceController : MonoBehaviour
{
    public static SurfaceController Instance { get; private set; }

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
        SetupHexGrid();

    }
    public float HexSize = 1f;
    public GameObject hexPrefab;
    public List<Material> materials = new List<Material>();
    private List<Surface> surfaces = new List<Surface>();

    public List<Vector3> preDefinedPlayerPositions = new List<Vector3>();

    public Vector3 GridCenter;
    public Vector3[] neighborOffsets;
    public float hexWidth;
    public float hexHeight;
    public float playerPositionSpreadDistace=14;

    public List<Vector3> GetTerritoryHexes(int playerId)
    {
        return surfaces[playerId].initialHexes;
    }
    public bool isOutsideArea(int playerId)
    {
        return surfaces[playerId].isOutsideInitialArea;
    }
    public bool isPositionInsideInitialArea(Vector3 position, int playerId)
    {
        return surfaces[playerId].isInsideInitialArea(position);

    }
    public void InitializeSurface(int playerId)
    {

        Vector3 center = preDefinedPlayerPositions[playerId];
        Surface surface = gameObject.AddComponent<Surface>();

        Material defMat = new Material(materials[0]);
        defMat.color = GameManager.Instance.GetColor(playerId);

        Material expMat = new Material(materials[1]);
        expMat.color = GameManager.Instance.GetColor(playerId);

        Material[] ColoredMats = new Material[] { defMat, expMat };
        surface.SetSurface(playerId, center, ColoredMats, hexPrefab);
        surfaces.Add(surface);
    }
    private void SetupHexGrid()
    {
        hexWidth = HexSize;  
        hexHeight = Mathf.Sqrt(3) * HexSize * 0.5f;  

        neighborOffsets = new Vector3[]
       {
            new Vector3(hexWidth, 0, 0),                      // Right
            new Vector3(-hexWidth, 0, 0),                     // Left
            new Vector3(hexWidth * 0.5f, 0, hexHeight),       // Top-right
            new Vector3(-hexWidth * 0.5f, 0, hexHeight),      // Top-left
            new Vector3(hexWidth * 0.5f, 0, -hexHeight),      // Bottom-right
            new Vector3(-hexWidth * 0.5f, 0, -hexHeight)      // Bottom-left
       };


        GridCenter = Vector3.zero;
        preDefinedPlayerPositions.Add(GridCenter);
        for (int i = 2; i < neighborOffsets.Length; i++)
        {
            preDefinedPlayerPositions.Add(SnapToHexGrid(neighborOffsets[i] * playerPositionSpreadDistace));
        }

    }
    public void SubmitStep(Vector3 playerPosition, int playerId)
    {
        Vector3 snappedPosition = SnapToHexGrid(new Vector3(playerPosition.x, 0, playerPosition.z));
        SetStepOutcome(snappedPosition, playerId);

        surfaces[playerId].SubmitStep(snappedPosition);
    }
    private void SetStepOutcome(Vector3 snappedPosition, int playerId)
    {
        foreach (Surface surface in surfaces)
        {
            if (surface.ID == playerId)
                continue;


            if (surface.instantiatedHexes.Contains(snappedPosition))
            {
              
                surface.DestroyAllHexs();
                LevelManager.Instance.LosePlayer(surface.ID);
             
            }
            if (surface.initialHexes.Contains(snappedPosition))
            {
              
              surface.RemoveHex(snappedPosition);
            }
        }

    }

    public Vector3 SnapToHexGrid(Vector3 charPosition)
    {
        float snappedX = Mathf.Round((charPosition.x - GridCenter.x) / hexWidth) * hexWidth + GridCenter.x;
        float snappedZ = Mathf.Round((charPosition.z - GridCenter.z) / hexHeight) * hexHeight + GridCenter.z;

        int row = Mathf.RoundToInt((charPosition.z - GridCenter.z) / hexHeight);

        if (Mathf.Abs(row) % 2 == 1)
        {
            snappedX += 0.5f * hexWidth;
        }

        return new Vector3(
        Mathf.Round(snappedX * 100f) / 100f,
        0,
        Mathf.Round(snappedZ * 100f) / 100f);
    }


}
