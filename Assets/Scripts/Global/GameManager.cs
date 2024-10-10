using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
    public Color GetColor(int id){
        return Colors[id];
    }
    public Color[] Colors;
    private Transform Trash;
    public void MoveToTrash(Transform t){
        t.SetParent(Trash);
    }
    // Start is called before the first frame update
    void Start()
    {
        Trash = new GameObject("Trash").transform;
    }

    
}
