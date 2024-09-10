using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject SnowmanPrefab;

    private int _idSnowmanIdCounter = 1;

    // Start is called before the first frame update
    void Start()
    {
        SpawnSnowman(new Vector3(0, 1, 1), false);
        SpawnSnowman(new Vector3(0, 1, 4), true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnSnowman(Vector3 spawnPosition, bool isNpc)
    {
        GameObject snowman = Instantiate(SnowmanPrefab, spawnPosition, Quaternion.identity);
        var newSnowman = snowman.GetComponent<SnowmanCombat>();
        newSnowman.snowmanId = _idSnowmanIdCounter++;
        newSnowman.isNpc = isNpc;
    }
}

public static class Constants
{

}
