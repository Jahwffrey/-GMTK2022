using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGen : MonoBehaviour
{
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject puddlePrefab;
    public GameObject grassPrefab;

    public int treeCost = 2;
    public int rockCost = 4;
    public int puddleCost = 3;
    public int budget = 17;
    public int grassCount = 20;

    private List<GameObject> obstacles;
    private Bounds bounds;
    private int treeCount = 0;
    private int rockCount = 0;
    private int puddleCount = 0;
    
    void Start()
    {
        obstacles = new List<GameObject>();
        bounds = transform.GetComponent<Collider>().bounds;

        PlaceObstacles();
    }

    public void PlaceObstacles()
    {//Decide how many of each obstacle to spawn
        //This assumes that trees cost the least
        int currentBudget = budget;
        while (currentBudget >= treeCost)
        {
            int choice = Random.Range(0, 3);
            switch (choice)
            {
                case 0:
                    treeCount += 1;
                    currentBudget -= treeCost;
                    break;
                case 1:
                    if (currentBudget >= rockCost)
                    {
                        rockCount += 1;
                        currentBudget -= rockCost;
                    }
                    break;
                case 2:
                    if (currentBudget >= puddleCost)
                    {
                        puddleCount += 1;
                        currentBudget -= puddleCost;
                    }
                    break;
                default:
                    Debug.Log("Error: tried to spawn invalid obstacle type!");
                    break;
            }
        }

        Spawn( treePrefab, treeCount );
        Spawn( rockPrefab, rockCount );
        Spawn( puddlePrefab, puddleCount );
        Spawn( grassPrefab, grassCount );
    }

    //Spawn a given prefab a given amount of times
    //Accounts for overlapping and board edge
    void Spawn( GameObject prefab, int count )
    {
        int layerMask = 1 << 7;
        Bounds objBounds = prefab.transform.GetChild(0).GetComponent<Renderer>().bounds;
        float halfX = objBounds.extents.x;
        float halfZ = objBounds.extents.z;
        for( int i = 0; i < count; ++i )
        {
            float randomRotation = Random.Range( 0.0f, 360.0f );
            float randomX = Random.Range( bounds.center.x - bounds.extents.x + halfX, bounds.center.x + bounds.extents.x - halfX );
            float randomZ = Random.Range( bounds.center.z - bounds.extents.z + halfZ, bounds.center.z + bounds.extents.z - halfZ );
            int safety = 10000;
            while (safety > 0 && Physics.CheckBox( new Vector3(randomX, objBounds.center.y, randomZ), objBounds.extents*2f, Quaternion.identity, layerMask, QueryTriggerInteraction.Collide ) )
            {
                safety -= 1;
                randomX = Random.Range( bounds.center.x - bounds.extents.x + halfX, bounds.center.x + bounds.extents.x - halfX );
                randomZ = Random.Range( bounds.center.z - bounds.extents.z + halfZ, bounds.center.z + bounds.extents.z - halfZ );
            }
            GameObject newObject = Instantiate( prefab, new Vector3( randomX, 0, randomZ ), Quaternion.identity );
            newObject.transform.Rotate( Vector3.up * randomRotation );
            obstacles.Add( newObject );
        }
    }

    //Clear the board for the next game
    public void Cleanup()
    {
        foreach( GameObject o in obstacles )
        {
            Destroy( o );
        }
        obstacles.Clear();
        treeCount = 0;
        rockCount = 0;
        puddleCount = 0;
    }

}
