using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    static int SELECTABLE_LAYER = 8;
    
    private Camera cam;
    public int playerID = 0;
    public GameObject pointer;
    public float pointerRotateSpeed = 0.25f;
    public float pointerBounceSpeed = 0.1f;
    public float pointerBounceHeight = 0.1f;
    private float pointerAnim;

    void Start()
    {
        cam = Camera.main;
        pointerAnim = 0;
    }

    void Update()
    {
        //Add check for "if in placing phase" and "if it's my turn" (multiplayer)
        if(playerID == 2) //FOR TESTING PURPOSES, DELETE LATER
        {
            return;
        }
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay( Input.mousePosition );
        int layerMask = 1 << SELECTABLE_LAYER;
        
        if( Physics.Raycast( ray, out hit, float.PositiveInfinity, layerMask ) )
        {
            //Check if we're selecting our own space
            if( hit.transform.parent.parent.GetComponent<PlayerControl>().playerID == playerID )
            {
                if( Input.GetMouseButtonDown(0) )
                {
                    //CLICKED A VALID SPACE
                    Debug.Log("Clicked " + hit.transform.name + "!");
                }
                //Move pointer to selected space and animate
                pointer.SetActive(true);
                pointerAnim += (Time.deltaTime * pointerBounceSpeed) % Mathf.PI;
                float currentBounceHeight = Mathf.Abs( Mathf.Sin( pointerAnim ) ) * pointerBounceHeight;
                pointer.transform.position = hit.transform.position + Vector3.up * currentBounceHeight;;
                pointer.transform.Rotate( 0, 0, pointerRotateSpeed );
            }
        }
        else
        {
            pointer.SetActive(false);
        }
    }
}
