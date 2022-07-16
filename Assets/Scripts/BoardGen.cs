using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGen : MonoBehaviour
{
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject puddlePrefab;

    private List<GameObject> obstacles;
    private Bounds bounds;
    private Bounds treeBounds;
    private Bounds rockBounds;
    private Bounds puddleBounds;
    
    // Start is called before the first frame update
    void Start()
    {
        obstacles = new List<GameObject>();
        bounds = transform.GetComponent<Collider>().bounds;
        treeBounds = treePrefab.GetComponent<Collider>().bounds;
        rockBounds = rockPrefab.GetComponent<Collider>().bounds;
        puddleBounds = puddlePrefab.GetComponent<Collider>().bounds;
    }

    void PlaceObstacles( int treeCount = 3, int rockCount = 5, int puddleCount = 2 )
    {
        
        int layerMask = 1 << 7;
        for( int i = 0; i < treeCount; ++i )
        {
            float randomRotation = Random.Range( 0.0f, 360.0f );
            GameObject newTree = Instantiate( treePrefab );
            newTree.transform.Rotate( Vector3.up * randomRotation );
            float randomX = Random.Range( bounds.center.x - bounds.extents.x + 0.5f, bounds.center.x + bounds.extents.x - 0.5f );
            float randomZ = Random.Range( bounds.center.z - bounds.extents.z + 0.5f, bounds.center.z + bounds.extents.z - 0.5f );
            while( Physics.CheckBox( new Vector3(randomX, treeBounds.center.y, randomZ), treeBounds.extents*1.2f, Quaternion.identity, layerMask ) )
            {
                randomX = Random.Range( bounds.center.x - bounds.extents.x + 0.5f, bounds.center.x + bounds.extents.x - 0.5f );
                randomZ = Random.Range( bounds.center.z - bounds.extents.z + 0.5f, bounds.center.z + bounds.extents.z - 0.5f );
            }
            newTree.transform.position = new Vector3( randomX, 0, randomZ );
            obstacles.Add( newTree );
        }
    }
}
