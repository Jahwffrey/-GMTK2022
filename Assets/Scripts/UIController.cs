using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public Transform unitRow;
    public Transform diceRow;
    public GameObject uiModel;
    public GameObject pointerGhost;
    public GameObject selectBox;

    public float elementSpacing = 0.25f;
    public float elementHoverScale = 1.5f;
    public float elementRotateSpeed = 0.25f;

    private List<Transform> units;
    private List<Transform> dice;
    private Vector3 elementScalar;
    private Transform selectedElement;
    
    // Start is called before the first frame update
    void Start()
    {
        units = new List<Transform>();
        dice = new List<Transform>();
        elementScalar = new Vector3( elementHoverScale, elementHoverScale, elementHoverScale );
        selectedElement = null;
        selectBox.SetActive(false);
        //TEST
        for( int i = 0; i < 4; i++ )
        {
            units.Add( Instantiate( uiModel, unitRow, false ).transform );
            units[i].name = "UI Element " + i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //PLACE UI ELEMENTS
        for( int i = 0; i < units.Count; i++ )
        {
            float offset = i + (i * elementSpacing ) - ( ( units.Count - 1 + (units.Count - 1) * elementSpacing ) / 2 );
            units[i].localPosition = new Vector3(offset,0,0);
            units[i].localScale = Vector3.one;
            units[i].Rotate( Vector3.up * elementRotateSpeed );
        }

        //CHECK FOR CLICKING ON UI
        RaycastHit hit;
        Ray ray = GetComponent<Camera>().ScreenPointToRay( Input.mousePosition );
        int layerMask = 1 << 6;
        
        if( Physics.Raycast( ray, out hit, float.PositiveInfinity, layerMask ) )
        {
            hit.transform.localScale = elementScalar;
            if( Input.GetMouseButtonDown(0) )
            {
                selectedElement = hit.transform;
                selectBox.SetActive( true );
                selectBox.transform.position = selectedElement.position + Vector3.forward;
                pointerGhost.GetComponent<MeshFilter>().mesh = hit.transform.GetComponent<MeshFilter>().mesh;
                Debug.Log("Clicked " + hit.transform.name + "!");
            }
        }
    }
}
