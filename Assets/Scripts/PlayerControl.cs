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
    public UnitController UnitController;

    [Header("UI")]
    public GameObject ReadyButton;
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
    private List<DiceUnit> activeUnits;    //Units on the battlefield 
    private UnitID lastPlacedUnit;
    private List<int> unitInventory;        //Integers corresponding to available units not yet on battlefield
    private List<Transform> diceInventory;
    private List<Transform> dice;
    private Vector3 elementScalar;
    private int selectedElement;
    private Camera cam;
    private float pointerAnim;
    private PlaceMode placementMode;

    private StartingSpace spaceInfo;

    public enum PlaceMode
    {
        PLACE_UNIT,
        PLACE_DIE,
        GAMEPLAY,
        WAIT_FOR_OTHER_PLAYER
    }
    
    //Ensure this is in the same order as the unitPrefabs list in Player Perspective Prefab
    public enum UnitID
    {
        SQUIRREL,
        BIRD,
        NONE
    }
    
    public void BeginGameplay()
    {
        placementMode = PlaceMode.GAMEPLAY;
    }
    
    protected void HideAllUI()
    {
        unitRow.gameObject.SetActive(false);
        diceRow.gameObject.SetActive(false);
    }

    public void PregameSetup()
    {
        if (playerID == 0)
        {
            placementMode = PlaceMode.PLACE_UNIT;
        }
        else
        {
            placementMode = PlaceMode.WAIT_FOR_OTHER_PLAYER;
        }
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
        elementScalar = new Vector3(elementHoverScale, elementHoverScale, elementHoverScale);
        selectedElement = -1;
        selectBox.SetActive(false);
        placementMode = PlaceMode.PLACE_UNIT;
        lastPlacedUnit = UnitID.NONE;
        spaceInfo = null;
    }

    public void ClearBothInventories()
    {
        while(uiUnits.Count > 0)
        {
            RemoveFromUnitInventory(0);
        }
        while(diceInventory.Count > 0)
        {
            RemoveFromDiceInventory(diceInventory[0]);
        }
    }

    public void PostgameCleanup()
    {
        ClearBothInventories();
    }

    public void SetInventories(List<UnitID> units, List<Dice> dice)
    {
        ClearBothInventories();

        foreach(var unit in units)
        {
            AddToUnitInventory(unit);
        }


        foreach (var die in dice)
        {
            AddToDiceInventory(die);
        }
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
        if(playerID != UnitController.GetCurrentPlayerId)
        {
            HideAllUI();
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
        else
        {
            HideAllUI();
        }
    }

    public GameObject GetUnitPrefab(UnitID id)
    {
        return unitPrefabs[(int)id];
    }

    void UnitPlacingMode( Ray ray, int layerMask )
    {
        unitRow.gameObject.SetActive(true);
        diceRow.gameObject.SetActive(false);
        RaycastHit hit;
        if( Physics.Raycast( ray, out hit, float.PositiveInfinity, layerMask ) )
        {
            //Check if we're selecting our own space
            spaceInfo = hit.transform.GetComponent<StartingSpace>();
            if( spaceInfo.parentPlayerID == playerID )
            {
                if( Input.GetMouseButtonUp(0) )
                {
                    //CLICKED A VALID SPACE, AND...
                    //PLACED A UNIT
                    if( !spaceInfo.HasUnit() && selectedElement != -1 )
                    {
                        DiceUnit newUnit = Instantiate( unitPrefabs[unitInventory[selectedElement]].GetComponent<DiceUnit>() );
                        newUnit.SetPlayer(playerID);
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
                        AddToDiceInventory(spaceInfo.GetUnit().GetDice());
                        spaceInfo.GetUnit().DoDestroy();
                        //DoDestroy already does this remove so we dont have to
                        // activeUnits.Remove( spaceInfo.GetUnit() );
                        AddToUnitInventory( (UnitID)spaceInfo.unitType );
                        spaceInfo.RemoveUnit();
                    }
                    pointer.SetActive(false);
                    pointerGhost.SetActive(false);
                }

                //Move pointer to selected space and animate, if the space is empty
                else if( !spaceInfo.HasUnit() && selectedElement != -1 )
                {
                    AnimatePointer( false, false, true );
                }
                //if hovering over existing element, show X
                else if( spaceInfo.HasUnit() )
                {
                    AnimatePointer( true, true, false);
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
        UpdateCanvasUI();
        if( placementMode == PlaceMode.PLACE_UNIT)
        {
            UpdateUnitUI();
        }
        else if( placementMode == PlaceMode.PLACE_DIE )
        {
            UpdateDiceUI();
        }
    }

    protected void UpdateCanvasUI()
    {
        if (placementMode == PlaceMode.PLACE_UNIT)
        {
            ReadyButton.SetActive(true);
        }
        else
        {
            ReadyButton.SetActive(false);
        }
    }

    public void PlayerOneReady()
    {
        if (playerID == 0)
        {
            placementMode = PlaceMode.WAIT_FOR_OTHER_PLAYER;
        }
        else
        {
            placementMode = PlaceMode.PLACE_UNIT;
        }
    }
    public void PlayerTwoReady()
    {
        placementMode = PlaceMode.WAIT_FOR_OTHER_PLAYER;
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
                if (selectedElement != -1)
                {
                    selectBox.SetActive(true);
                    pointerGhost.GetComponent<MeshFilter>().mesh = hit.transform.GetComponent<MeshFilter>().mesh;
                    pointerGhost.GetComponent<Renderer>().material = hit.transform.GetComponent<Renderer>().material;
                }
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
        AnimatePointer( true, false, false);
        var currentUnit = activeUnits[activeUnits.Count - 1];

        //PLACE UI ELEMENTS
        for ( int i = 0; i < diceInventory.Count; i++ )
        {
            float offset = i + (i * elementSpacing ) - ( ( diceInventory.Count - 1 + (diceInventory.Count - 1) * elementSpacing ) / 2 );
            diceInventory[i].localPosition = new Vector3(offset,0,0);
            diceInventory[i].localScale = Vector3.one;
            diceInventory[i].Rotate( new Vector3( elementRotateSpeed, elementRotateSpeed / 2, elementRotateSpeed / 3 ) );
            diceInventory[i].GetComponent<UIDieDisplay>().HideToolTip();
        }

        if( Input.GetMouseButtonDown(1) )
        {
            currentUnit.DoDestroy();
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
            hit.transform.GetComponent<UIDieDisplay>().ShowToolTip();
            if( Input.GetMouseButtonDown(0) )
            {
                selectedElement = diceInventory.IndexOf(hit.transform);
                if (selectedElement != -1)
                {
                    currentUnit.SetDice(diceInventory[selectedElement].GetComponent<UIDieDisplay>().GetDie());
                    RemoveFromDiceInventory(diceInventory[selectedElement]);
                    spaceInfo.PlayDieEffect();
                    SwitchPlacementMode();
                }
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

    void AnimatePointer( bool spaceFilled = false, bool cancel = false, bool showPointerGhost = true )
    {
        pointer.SetActive(true);
        pointer.GetComponent<Renderer>().material = cancel ? cancelMaterial : pointerMaterial;
        pointer.GetComponent<MeshFilter>().mesh = cancel ? cancelMesh : pointerMesh;
        pointerAnim += (Time.deltaTime * pointerBounceSpeed) % Mathf.PI;
        float currentBounceHeight = Mathf.Abs( Mathf.Sin( pointerAnim ) ) * pointerBounceHeight;
        Vector3 addHeight = spaceFilled ? Vector3.up * spaceInfo.GetUnit().GetComponent<Collider>().bounds.size.y : Vector3.zero;
        pointer.transform.position = spaceInfo.transform.position + addHeight + Vector3.up * currentBounceHeight;;
        pointer.transform.Rotate( 0, 0, pointerRotateSpeed );

        if(showPointerGhost)
        {
            pointerGhost.SetActive(true);
            pointerGhost.transform.position = spaceInfo.transform.position + Vector3.up * ( pointerBounceHeight + 0.5f );
        }
        else
        {
            pointerGhost.SetActive(false);
        }
    }
}
