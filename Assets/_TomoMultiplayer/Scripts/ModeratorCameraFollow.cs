using UnityEngine;

public class ModeratorCameraFollow : MonoBehaviour
{
    [SerializeField] Vector2 limits;
    [SerializeField] float speed = 10f;

    private bool canFollow = false;

    private void Update()
    {
        if (!canFollow) return;

        float inputData = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
        transform.position += Vector3.right * inputData;
        //Clamp
        if(transform.position.x >= limits.y) transform.position = new Vector3(limits.y, transform.position.y, transform.position.z);
        if(transform.position.x <= limits.x) transform.position = new Vector3(limits.x, transform.position.y, transform.position.z);
    }

    public void UpdateCameraFollow(bool camMovementEnabled)
    {
        canFollow = camMovementEnabled;
    }

}
