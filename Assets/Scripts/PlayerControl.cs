using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TO DO:
//(hint - ctrl+F and search for TODO)
//-Change activeUnits to List<DiceUnit>
//-in StartingSpace.cs, change myUnit to type DiceUnit
//-update unitPrefabs in the inspector to contain actual dice units (make sure order is the same as the UnitID enum)

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
    public UnitController UnitController;
    
    [Header("UI")]
    public Transform unitRow;
    public Transform diceRow;
    public GameObject uiModel;
    public GameObject uiDiePrefab;
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
    private List<DiceUnit> activeUnits;    //Units on the battlefield  TODO
    private UnitID lastPlacedUnit;
    private List<int> unitInventory;        //Integers corresponding to available units not yet on battlefield
    private List<Transform> diceInventory;
    private List<Transform> dice;
    private Vector3 elementScalar;
    private int selectedElement;
    private Camera cam;
    private float pointerAnim;
    private PlaceMode placementMode;

    public enum PlaceMode
    {
        PLACE_UNIT,
        PLACE_DIE
    }
    
    //Ensure this is in the same order as the unitPrefabs list in Player Perspective Prefab
    public enum UnitID
    {
        SQUIRREL,
        BIRD,
        NONE
    }

    void Start()
    {
        cam = Camera.main;
        pointerAnim = 0;
        uiUnits = new List<Transform>();
        activeUnits = new List<DiceUnit>();
        unitInventory = new List<int>();
        diceInventory = new List<Transform>();
        dice = new List<Transform>();
        elementScalar = new Vector3( elementHoverScale, elementHoverScale, elementHoverScale );
        selectedElement = -1;
        selectBox.SetActive(false);
        placementMode = PlaceMode.PLACE_UNIT;
        lastPlacedUnit = UnitID.NONE;
        
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~FILLING INVENTORY TEST~~~~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        AddToUnitInventory( UnitID.SQUIRREL );
        AddToUnitInventory( UnitID.BIRD );
        AddToUnitInventory( UnitID.SQUIRREL );
        AddToUnitInventory( UnitID.BIRD );
        AddToUnitInventory( UnitID.SQUIRREL );
        AddToUnitInventory( UnitID.BIRD );
        AddToUnitInventory( UnitID.SQUIRREL );
        AddToUnitInventory( UnitID.BIRD );

        var doublesDice = new Dice(new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.DoubleAttack, DiceSides.DoubleMove, DiceSides.DoubleMove, DiceSides.Lose1Hp, DiceSides.Lose2Hp });
        var prettyGoodDice = new Dice(new List<DiceSides>() { DiceSides.Attack, DiceSides.Move, DiceSides.Defend, DiceSides.DoubleAttack, DiceSides.DoubleMove, DiceSides.Nothing });
        var scout = new Dice(new List<DiceSides>() { DiceSides.Move, DiceSides.Move, DiceSides.Move, DiceSides.Move, DiceSides.Attack, DiceSides.Attack });

        AddToDiceInventory( doublesDice );
        AddToDiceInventory( doublesDice );
        AddToDiceInventory( doublesDice );
        AddToDiceInventory( prettyGoodDice );
        AddToDiceInventory( prettyGoodDice );
        AddToDiceInventory( prettyGoodDice );
        AddToDiceInventory( scout );
        AddToDiceInventory( scout );
    }

    void Update()
    {
        UpdateGameSpace();
        UpdateUI();
    }

    public void UnitWasFullyDestroyed(DiceUnit unit)
    {
        if (unit == null || activeUnits == null) return;

        if (activeUnits.Contains(unit))
        {
            activeUnits.Remove(unit);
        }
    }

    //Updates in actual gamespace
    void UpdateGameSpace()
    {
        //Add check for "if in placing phase" and "if it's my turn" (multiplayer)
        if(playerID == 2) //FOR TESTING PURPOSES, change to if(currentPlayerID != playerID) when a global current player id is implemented
        {
            return;
        }

        //MOUSE CONTROL
        Ray ray = cam.ScreenPointToRay( Input.mousePosition );
        int layerMask = 1 << SELECTABLE_LAYER;
        
        if( placementMode == PlaceMode.PLACE_UNIT )
        {
            UnitPlacingMode( ray, layerMask );
        }
        else if( placementMode == PlaceMode.PLACE_DIE )
        {
            DiePlacingMode( ray, layerMask );
        }
    }

    void UnitPlacingMode( Ray ray, int layerMask )
    {
        unitRow.gameObject.SetActive(true);
        diceRow.gameObject.SetActive(false);
        RaycastHit hit;
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
                        DiceUnit newUnit = Instantiate( unitPrefabs[unitInventory[selectedElement]].GetComponent<DiceUnit>() );
                        lastPlacedUnit = (UnitID)unitInventory[selectedElement];
                        Bounds bounds = newUnit.GetComponent<Collider>().bounds;
                        newUnit.transform.position = hit.transform.position + Vector3.up * bounds.size.y / 2 - (bounds.center - newUnit.transform.position);
                        newUnit.transform.eulerAngles = new Vector3( newUnit.transform.eulerAngles.x, transform.eulerAngles.y, newUnit.transform.eulerAngles.z);
                        activeUnits.Add( newUnit );
                        spaceInfo.AssignUnit( newUnit, unitInventory[selectedElement] );
                        RemoveFromUnitInventory( selectedElement );
                        selectedElement = -1;
                        selectBox.SetActive(false);
                        SwitchPlacementMode();
                    }
                    //REMOVED A UNIT
                    else if( spaceInfo.HasUnit() )
                    {
                        spaceInfo.GetUnit().DoDestroy();
                        //DoDestroy already does this remove so we dont have to
                        // activeUnits.Remove( spaceInfo.GetUnit() );
                        AddToUnitInventory( (UnitID)spaceInfo.unitType );
                        //TODO: Add spaceInfo.GetUnit()'s Dice back to inventory using AddToDiceInventory
                        spaceInfo.RemoveUnit();
                    }
                    pointer.SetActive(false);
                    pointerGhost.SetActive(false);
                }

                //Move pointer to selected space and animate, if the space is empty
                else if( !spaceInfo.HasUnit() && selectedElement != -1 )
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

    void DiePlacingMode( Ray ray, int layerMask )
    {
        unitRow.gameObject.SetActive(false);
        diceRow.gameObject.SetActive(true);
    }

    //Updates on UI layer
    void UpdateUI()
    {
        if( placementMode == PlaceMode.PLACE_UNIT )
        {
            UpdateUnitUI();
        }
        else if( placementMode == PlaceMode.PLACE_DIE )
        {
            UpdateDiceUI();
        }
    }

    //CALLED WHEN SELECTING UNITS
    void UpdateUnitUI()
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
            }
        }

        if( selectedElement != -1 )
        {
            selectBox.transform.position = uiUnits[selectedElement].position + Vector3.forward;
        }
    }

    //CALLED WHEN SELECTING DIE
    void UpdateDiceUI()
    {
        //PLACE UI ELEMENTS
        for( int i = 0; i < diceInventory.Count; i++ )
        {
            float offset = i + (i * elementSpacing ) - ( ( diceInventory.Count - 1 + (diceInventory.Count - 1) * elementSpacing ) / 2 );
            diceInventory[i].localPosition = new Vector3(offset,0,0);
            diceInventory[i].localScale = Vector3.one;
            diceInventory[i].Rotate( new Vector3( elementRotateSpeed, elementRotateSpeed / 2, elementRotateSpeed / 3 ) );
        }

        if( Input.GetMouseButtonDown(1) )
        {
            activeUnits[activeUnits.Count - 1].DoDestroy();
            // We don't need to modify activeUnits after DoDestroy, DoDestroy already did it
            // activeUnits.RemoveAt( activeUnits.Count - 1 );
            AddToUnitInventory( lastPlacedUnit );
            lastPlacedUnit = UnitID.NONE;
            SwitchPlacementMode();
            return;
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
                selectedElement = diceInventory.IndexOf( hit.transform );
                Dice dieToAssign = diceInventory[selectedElement].GetComponent<UIDieDisplay>().GetDie();
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //TODO: ASSIGN DIE TO UNIT HERE
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                RemoveFromDiceInventory( diceInventory[selectedElement] );
                SwitchPlacementMode();
            }
        }

        if( selectedElement != -1 )
        {
            selectBox.transform.position = diceInventory[selectedElement].position + Vector3.forward;
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

    public void AddToDiceInventory( Dice d )
    {
        Transform newUID = Instantiate( uiDiePrefab, diceRow, false ).transform;
        newUID.GetComponent<UIDieDisplay>().SetDie( d );
        diceInventory.Add( newUID );
    }

    public void RemoveFromDiceInventory( Transform d )
    {
        diceInventory.Remove( d );
        Destroy( d.gameObject );
    }

    private void SwitchPlacementMode()
    {
        selectedElement = -1;
        if(placementMode == PlaceMode.PLACE_UNIT)
        {
            placementMode = PlaceMode.PLACE_DIE;
        }
        else
        {
            placementMode = PlaceMode.PLACE_UNIT;
        }
    }
}
