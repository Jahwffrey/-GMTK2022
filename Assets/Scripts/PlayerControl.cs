﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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
    public bool FreePlayMode = false;

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

    public GameObject textBox;
    public GameObject infoText;
    public GameObject diceKey;
    public Canvas infoCanvas;

    public MusicController musicMaster;

    private StartingSpace spaceInfo;

    public enum PlaceMode
    {
        PLACE_UNIT,
        PLACE_DIE,
        GAMEPLAY,
        WAIT_FOR_OTHER_PLAYER,
        SELECT_NEW_UNIT,
        SELECT_NEW_DIE
    }
    
    //Ensure this is in the same order as the unitPrefabs list in Player Perspective Prefab
    public enum UnitID
    {
        WOLF = 0,
        HARE = 1,
        SNAKE = 2,
        DUCK = 3,
        DEER = 4,
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
        foreach(var space in GetComponentsInChildren<StartingSpace>())
        {
            space.ResetForGame();
        }
        if (playerID == 0)
        {
            placementMode = PlaceMode.PLACE_UNIT;
            SetBothInventoriesForFreePlayMode();
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
        SetBothInventoriesForFreePlayMode();
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

    public void FreePlayModeHack()
    {
        SetBothInventoriesForFreePlayMode();
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
        
        if( placementMode == PlaceMode.PLACE_UNIT || placementMode == PlaceMode.SELECT_NEW_UNIT )
        {
            UnitPlacingMode( ray, layerMask );
        }
        else if( placementMode == PlaceMode.PLACE_DIE  || placementMode == PlaceMode.SELECT_NEW_DIE)
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
        if(placementMode == PlaceMode.PLACE_UNIT && Physics.Raycast( ray, out hit, float.PositiveInfinity, layerMask ) )
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
                        musicMaster.PlayPlacement();
                    }
                    //REMOVED A UNIT
                    else if( spaceInfo.HasUnit() )
                    {
                        if( !FreePlayMode )
                        {
                            AddToDiceInventory(spaceInfo.GetUnit().GetDice());
                            AddToUnitInventory( (UnitID)spaceInfo.unitType );
                        }
                        spaceInfo.GetUnit().DoDestroy();
                        //DoDestroy already does this remove so we dont have to
                        // activeUnits.Remove( spaceInfo.GetUnit() );
                        spaceInfo.RemoveUnit();
                        musicMaster.PlayBlip();
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
        if( placementMode == PlaceMode.PLACE_UNIT || placementMode == PlaceMode.SELECT_NEW_UNIT)
        {
            UpdateUnitUI();
        }
        else if( placementMode == PlaceMode.PLACE_DIE || placementMode == PlaceMode.SELECT_NEW_DIE )
        {
            UpdateDiceUI();
        }
    }

    protected void UpdateCanvasUI()
    {
        if (placementMode == PlaceMode.PLACE_UNIT && !UnitController.AnnouncementShowing())
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
            SetBothInventoriesForFreePlayMode();
        }
    }
    public void PlayerTwoReady()
    {
        placementMode = PlaceMode.WAIT_FOR_OTHER_PLAYER;
    }

    protected void SetBothInventoriesForFreePlayMode()
    {
        if (FreePlayMode)
        {
            var allUnits = System.Enum.GetValues(typeof(PlayerControl.UnitID)).Cast<PlayerControl.UnitID>().ToList();
            allUnits.RemoveAt(allUnits.Count - 1); // Remove the NONE unit
            SetInventories(allUnits, UnitController.GetAllDice());
        }
    }

    private int UnitsPerUiRow = 8;
    //CALLED WHEN SELECTING UNITS
    void UpdateUnitUI()
    {
        //PLACE UI ELEMENTS
        for( int i = 0; i < uiUnits.Count; i++ )
        {
            int xPos = i % UnitsPerUiRow;
            int yPos = i / UnitsPerUiRow;
            int yRows = uiUnits.Count / UnitsPerUiRow;
            int countOnThisRow = (yPos == yRows) ? uiUnits.Count - yRows * UnitsPerUiRow : UnitsPerUiRow;
            float offset = xPos + (xPos * elementSpacing ) - ( (countOnThisRow - 1 + (countOnThisRow - 1) * elementSpacing ) / 2 );
            uiUnits[i].localPosition = new Vector3(offset,-2.5f - 2f * yPos,0);
            uiUnits[i].localScale = Vector3.one;
            uiUnits[i].Rotate( Vector3.up * elementRotateSpeed * Time.deltaTime );
        }

        textBox.gameObject.SetActive(false);

        //CHECK FOR CLICKING ON UI
        RaycastHit hit;
        Ray ray = uiCam.ScreenPointToRay( Input.mousePosition );
        int layerMask = 1 << 6;
        
        if( Physics.Raycast( ray, out hit, float.PositiveInfinity, layerMask ) )
        {
            hit.transform.localScale = elementScalar;
            textBox.gameObject.SetActive(true);

            try
            {
                int hitElement2 = uiUnits.IndexOf(hit.transform);
                infoText.GetComponent<TextMeshProUGUI>().text = GetUnitPrefab((UnitID)unitInventory[hitElement2]).GetComponent<DiceUnit>().GetInfoText();
            } catch
            {
                // Ignore
            }

            if( Input.GetMouseButtonDown(0) )
            {
                int hitElement = uiUnits.IndexOf(hit.transform);
                if (placementMode == PlaceMode.SELECT_NEW_UNIT)
                {
                    UnitController.NewUnitSelected((UnitID) unitInventory[hitElement]);
                }
                else
                {
                    selectedElement = hitElement;
                    if (selectedElement != -1)
                    {
                        selectBox.SetActive(true);
                        pointerGhost.GetComponent<MeshFilter>().mesh = hit.transform.GetComponent<MeshFilter>().mesh;
                        pointerGhost.GetComponent<Renderer>().material = hit.transform.GetComponent<Renderer>().material;
                    }
                }
            }
        }

        if( selectedElement != -1 )
        {
            selectBox.transform.position = uiUnits[selectedElement].position + Vector3.forward + Vector3.up * 0.8f;
        }
    }
    
    public void BeginSelectNewUnit()
    {
        placementMode = PlaceMode.SELECT_NEW_UNIT;
    }

    public void BeginSelectNewDie()
    {
        placementMode = PlaceMode.SELECT_NEW_DIE;
    }

    //CALLED WHEN SELECTING DIE
    void UpdateDiceUI()
    {
        DiceUnit currentUnit = null;
        if (placementMode == PlaceMode.PLACE_DIE)
        {
            AnimatePointer(true, false, false);
            currentUnit = activeUnits[activeUnits.Count - 1];
        }

        //PLACE UI ELEMENTS
        for ( int i = 0; i < diceInventory.Count; i++ )
        {
            int xPos = i % UnitsPerUiRow;
            int yPos = i / UnitsPerUiRow;
            int yRows = diceInventory.Count / UnitsPerUiRow;
            int countOnThisRow = (yPos == yRows) ? diceInventory.Count - yRows * UnitsPerUiRow : UnitsPerUiRow;
            float offset = xPos + (xPos * elementSpacing ) - ( (countOnThisRow - 1 + (countOnThisRow - 1) * elementSpacing ) / 2 );
            diceInventory[i].localPosition = new Vector3(offset,-2f * yPos, 0);
            diceInventory[i].localScale = Vector3.one;
            diceInventory[i].Rotate( new Vector3( elementRotateSpeed * Time.deltaTime, elementRotateSpeed * Time.deltaTime / 2, elementRotateSpeed * Time.deltaTime / 3 ) );
            diceInventory[i].GetComponent<UIDieDisplay>().HideToolTip();
        }

        if (placementMode == PlaceMode.PLACE_DIE)
        {
            if (Input.GetMouseButtonDown(1))
            {
                currentUnit.DoDestroy();
                // We don't need to modify activeUnits after DoDestroy, DoDestroy already did it
                // activeUnits.RemoveAt( activeUnits.Count - 1 );
                AddToUnitInventory(lastPlacedUnit);
                lastPlacedUnit = UnitID.NONE;
                SwitchPlacementMode();
                pointer.gameObject.SetActive(false);
                return;
            }
        }

        //CHECK FOR CLICKING ON UI
        RaycastHit hit;
        Ray ray = uiCam.ScreenPointToRay( Input.mousePosition );
        int layerMask = 1 << 6;
        
        if( Physics.Raycast( ray, out hit, float.PositiveInfinity, layerMask ) )
        {
            hit.transform.localScale = elementScalar;
            hit.transform.GetComponent<UIDieDisplay>().ShowToolTip();
            if( Input.GetMouseButtonUp(0) )
            {
                var selectedItem = diceInventory.IndexOf(hit.transform);

                if (placementMode == PlaceMode.SELECT_NEW_DIE)
                {
                    UnitController.NewDieSelected(diceInventory[selectedItem].GetComponent<UIDieDisplay>().GetDie());
                }
                else
                {
                    selectedElement = selectedItem;
                    if (selectedElement != -1)
                    {
                        currentUnit.SetDice(diceInventory[selectedElement].GetComponent<UIDieDisplay>().GetDie());
                        RemoveFromDiceInventory(diceInventory[selectedElement]);
                        spaceInfo.PlayDieEffect();
                        SwitchPlacementMode();
                    }
                }
            }

            if( FreePlayMode )
            {
                if( Input.mousePosition.x < Screen.width/2 && Input.mousePosition.y < Screen.height * 0.8f )
                {
                    textBox.SetActive(false);
                }
                else
                {
                    textBox.SetActive(true);
                }
            }
        }
        else if( FreePlayMode )
        {
            textBox.SetActive(true);
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
        newUiUnit.GetComponent<MeshFilter>().mesh = unitPrefabs[id].GetComponent<DiceUnit>().UnitModel.GetComponent<MeshFilter>().sharedMesh;
        newUiUnit.GetComponent<Renderer>().material = unitPrefabs[id].GetComponent<DiceUnit>().UnitModel.GetComponent<Renderer>().sharedMaterial;
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
            SetBothInventoriesForFreePlayMode();
            diceKey.SetActive(true);
            infoText.SetActive(false);
            textBox.SetActive(true);
        }
        else
        {
            placementMode = PlaceMode.PLACE_UNIT;
            SetBothInventoriesForFreePlayMode();
            infoText.SetActive(true);
            diceKey.SetActive(false);
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
        pointer.transform.Rotate( 0, 0, pointerRotateSpeed * Time.deltaTime );

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
