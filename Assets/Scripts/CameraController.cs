using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float CamSpeedH = 2.0f;
    public float CamSpeedV = 2.0f;
    protected float CamDist = 4.5f;
    private float CamYaw = 0.0f;
    private float CamPitch = 45.0f;

    //protected LayerMask CameraLayerMask;

    private void Start()
    {
        CamDist = MainCamera().transform.position.magnitude;
        //CameraLayerMask = ~LayerMask.GetMask(new[] { "Water", "EditHelper", "Passthrough", "Player" });
    }

    protected Camera MainCamera()
    {
        return Camera.main;
    }

    protected Ray ray;
    protected RaycastHit hit;
    public void ControlCam()
    {
        /*if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                CamDist -= Input.mouseScrollDelta.y * 0.25f;
            }

            // Mouse free / building
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {*/
            // Move camera
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            CamYaw += CamSpeedH * Input.GetAxis("Mouse X");
            CamPitch -= CamSpeedV * Input.GetAxis("Mouse Y");
            if (CamPitch > 89.5f)
            {
                CamPitch = 89.5f;
            }
            if (CamPitch < 0)//-89.5f)
            {
            CamPitch = 0;// -89.5f;
            }
        //}

        // https://gamedev.stackexchange.com/questions/104693/how-to-use-input-getaxismouse-x-y-to-rotate-the-camera
        MainCamera().transform.eulerAngles = new Vector3(CamPitch, CamYaw, 0.0f);

        Vector3 pos = transform.position - MainCamera().transform.forward * CamDist;// + MainCamera().transform.right * -1.1f + MainCamera().transform.up * 0.5f;
        //var dirVect = pos - transform.position;
        //ray = new Ray(transform.position, dirVect.normalized);
        MainCamera().transform.position = pos;
    }

    public void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
