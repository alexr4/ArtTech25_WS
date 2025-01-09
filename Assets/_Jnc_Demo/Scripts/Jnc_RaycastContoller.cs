using UnityEngine;

public class Jnc_RaycastContoller : MonoBehaviour
{
    public new Camera camera;

    void Raycast()
    {
        var ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Break();
        }
    }

    void Update()
    {
        if (camera != null)
        {
            Raycast();
        }
    }
}
