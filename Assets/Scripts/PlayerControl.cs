using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    static int SELECTABLE_LAYER = 8;
    
    [Header("GameSpace")]
    public int playerID = 0;
    public GameObject pointer;
    public GameObject pointerGhost;
    public float pointerRotateSpeed = 0.25f;
    public float pointerBounceSpeed = 0.1f;
    public float pointerBounceHeight = 0.1f;
    public List<GameObject> unitPrefabs;
    
    [Header("UI")]
    public Transform unitRow;
    public Transform diceRow;
    public GameObject uiModel;
    public GameObject selectBox;
    public Camera uiCam;
    public float elementSpacing = 1f;
    public float elementHoverScale = 1.25f;
    public float elementRotateSpeed = 0.1f;
    public Mesh pointerMesh;
    public Mesh cancelMesh;
    public Material pointerMaterial;
    public Material cancelMaterial;

    private List<Transform> uiUnits;        //Units displayed in the UI
    private List<Transform> activeUnits;    //Units on the battlefield
    private List<int> unitInventory;        //Integers corresponding to available units not yet on battlefield
    private List<Transform> dice;
    private Vector3 elementScalar;
    private int selectedElement;
    private Camera cam;
    private float pointerAnim;
    
    //Ensure this is in the same order as the unitPrefabs list in Player Perspective Prefab
    public enum UnitID
    {
        SQUIRREL,
        BIRD
    }

    void Start()
    {
        cam = Camera.main;
        pointerAnim = 0;
        uiUnits = new List<Transform>();
        activeUnits = new List<Transform>();
        unitInventory = new List<int>();
        dice = new List<Transform>();
        elementScalar = new Vector3( elementHoverScale, elementHoverScale, elementHoverScale );
        selectedElement = -1;
        selectBox.SetActive(false);
        //TEST
        AddToUnitInventory( UnitID.SQUIRREL );
        AddToUnitInventory( UnitID.BIRD );
        AddToUnitInventory( UnitID.SQUIRREL );
        AddToUnitInventory( UnitID.BIRD );
        AddToUnitInventory( UnitID.SQUIRREL );
        AddToUnitInventory( UnitID.BIRD );
        AddToUnitInventory( UnitID.SQUIRREL );
        AddToUnitInventory( UnitID.BIRD );
    }

    void Update()
    {
        UpdateGameSpace();
        UpdateUI();
    }

    //Updates in actual gamespace
    void UpdateGameSpace()
    {
        //Add check for "if in placing phase" and "if it's my turn" (multiplayer)
        if(playerID == 2) //FOR TESTING PURPOSES, DELETE LATER
        {
            return;
        }

        //MOUSE CONTROL
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay( Input.mousePosition );
        int layerMask = 1 << SELECTABLE_LAYER;
        
        if( Physics.Raycast( ray, out hit, float.PositiveInfinity, layerMask ) )
        {
            //Check if we're selecting our own space
            StartingSpace spaceInfo = hit.transform.GetComponent<StartingSpace>();
            if( spaceInfo.parentPlayerID == playerID )
            {
                if( Input.GetMouseButtonUp(0) )
                {
                    //CLICKED A VALID SPACE, AND...
                    //PLACED A UNIT
                    if( !spaceInfo.HasUnit() && selectedElement != -1 )
                    {
                        Transform newUnit = Instantiate( unitPrefabs[unitInventory[selectedElement]] ).transform;
                        Bounds bounds = newUnit.GetComponent<Collider>().bounds;
                        newUnit.position = hit.transform.position + Vector3.up * bounds.size.y / 2 - (bounds.center - newUnit.position);
                        newUnit.eulerAngles = new Vector3( newUnit.eulerAngles.x, transform.eulerAngles.y, newUnit.eulerAngles.z);
                        activeUnits.Add( newUnit );
                        spaceInfo.AssignUnit( newUnit, unitInventory[selectedElement] );
                        RemoveFromUnitInventory( selectedElement );
                        selectedElement = -1;
                        selectBox.SetActive(false);
                    }
                    //REMOVED A UNIT
                    else if( spaceInfo.HasUnit() )
                    {
                        activeUnits.Remove( spaceInfo.GetUnit() );
                        AddToUnitInventory( (UnitID)spaceInfo.unitType );
                        spaceInfo.RemoveUnit();
                    }
                    pointer.SetActive(false);
                    pointerGhost.SetActive(false);
                }

                //Move pointer to selected space and animate, if the space is empty
                if( !spaceInfo.HasUnit() && selectedElement != -1 )
                {
                    pointer.SetActive(true);
                    pointer.GetComponent<Renderer>().material = pointerMaterial;
                    pointer.GetComponent<MeshFilter>().mesh = pointerMesh;
                    pointerAnim += (Time.deltaTime * pointerBounceSpeed) % Mathf.PI;
                    float currentBounceHeight = Mathf.Abs( Mathf.Sin( pointerAnim ) ) * pointerBounceHeight;
                    pointer.transform.position = hit.transform.position + Vector3.up * currentBounceHeight;;
                    pointer.transform.Rotate( 0, 0, pointerRotateSpeed );

                    pointerGhost.SetActive(true);
                    pointerGhost.transform.position = hit.transform.position + Vector3.up * ( pointerBounceHeight + 0.5f );
                }
                //if hovering over existing element, show X
                else if( spaceInfo.HasUnit() )
                {
                    pointer.SetActive(true);
                    pointer.GetComponent<Renderer>().material = cancelMaterial;
                    pointer.GetComponent<MeshFilter>().mesh = cancelMesh;
                    pointerAnim += (Time.deltaTime * pointerBounceSpeed) % Mathf.PI;
                    float currentBounceHeight = Mathf.Abs( Mathf.Sin( pointerAnim ) ) * pointerBounceHeight;
                    Vector3 addHeight = Vector3.up * spaceInfo.GetUnit().GetComponent<Collider>().bounds.size.y;
                    pointer.transform.position = hit.transform.position + addHeight + Vector3.up * currentBounceHeight;;
                    pointer.transform.Rotate( 0, 0, pointerRotateSpeed );

                    pointerGhost.SetActive(false);
                }
            }
        }
        else
        {
            pointer.SetActive(false);
            pointerGhost.SetActive(false);
        }
    }

    //Updates on UI layer
    void UpdateUI()
    {
        //PLACE UI ELEMENTS
        for( int i = 0; i < uiUnits.Count; i++ )
        {
            float offset = i + (i * elementSpacing ) - ( ( uiUnits.Count - 1 + (uiUnits.Count - 1) * elementSpacing ) / 2 );
            uiUnits[i].localPosition = new Vector3(offset,0,0);
            uiUnits[i].localScale = Vector3.one;
            uiUnits[i].Rotate( Vector3.up * elementRotateSpeed );
        }

        //CHECK FOR CLICKING ON UI
        RaycastHit hit;
        Ray ray = uiCam.ScreenPointToRay( Input.mousePosition );
        int layerMask = 1 << 6;
        
        if( Physics.Raycast( ray, out hit, float.PositiveInfinity, layerMask ) )
        {
            hit.transform.localScale = elementScalar;
            if( Input.GetMouseButtonDown(0) )
            {
                selectedElement = uiUnits.IndexOf( hit.transform );
                selectBox.SetActive( true );
                pointerGhost.GetComponent<MeshFilter>().mesh = hit.transform.GetComponent<MeshFilter>().mesh;
                pointerGhost.GetComponent<Renderer>().material = hit.transform.GetComponent<Renderer>().material;
                Debug.Log("Clicked " + hit.transform.name + "!");
            }
        }

        if( selectedElement != -1 )
        {
            selectBox.transform.position = uiUnits[selectedElement].position + Vector3.forward;
        }
    }

    public void AddToUnitInventory( UnitID unitID )
    {
        int id = (int)unitID;
        Transform newUiUnit = Instantiate( uiModel, unitRow, false ).transform;
        newUiUnit.GetComponent<MeshFilter>().mesh = unitPrefabs[id].GetComponent<MeshFilter>().sharedMesh;
        newUiUnit.GetComponent<Renderer>().material = unitPrefabs[id].GetComponent<Renderer>().sharedMaterial;
        uiUnits.Add( newUiUnit );
        unitInventory.Add( id );
    }

    public void RemoveFromUnitInventory( int index )
    {
        Transform toDestroy = uiUnits[index];
        uiUnits.Remove( toDestroy );
        unitInventory.RemoveAt( index );
        Destroy( toDestroy.gameObject );
    }
}
