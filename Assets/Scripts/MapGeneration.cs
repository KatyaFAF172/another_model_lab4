﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class MapGeneration : NetworkBehaviour
{
    // Start is called before the first frame update
    public Material[] materials;
    public GameObject prefab;
    public Vector3 mapSize;

    void Start()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int z = 0; z < mapSize.z; z++)
            {
                float noice = Mathf.PerlinNoise(x / mapSize.x, z / mapSize.z);
                int y = Mathf.RoundToInt(noice * mapSize.y);

                GameObject obj = Instantiate(prefab);
                obj.transform.position = new Vector3(x, y - 20, z);
                obj.GetComponent<Renderer>().material = y < 5 ? materials[0] :
                                                        y < 8 ? materials[1] :
                                                        y < 11 ? materials[2] :
                                                                 materials[3];
                NetworkServer.Spawn(obj);

            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
