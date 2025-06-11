using UnityEngine;
using UnityEngine.Events;

public class MouseClickController : MonoBehaviour
{
    public Vector3 clickPosition;
    
    public UnityEvent<Vector3> OnClick;
    // Update is called once per frame
    void Update()
    {
        // Get the mouse click position in world space
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
            {
                Vector3 rawPos = hitInfo.point;

                // Snap to grid center (assuming 1x1 tiles)
                float snappedX = Mathf.Floor(rawPos.x) + 0.5f;
                float snappedZ = Mathf.Floor(rawPos.z) + 0.5f;

                clickPosition = new Vector3(snappedX, 0, snappedZ);

                OnClick.Invoke(clickPosition);
            }
        }
        
        DebugExtension.DebugWireSphere(clickPosition, Color.yellow, .1f);
        Debug.DrawLine(Camera.main.transform.position, clickPosition, Color.yellow);
    }
}
