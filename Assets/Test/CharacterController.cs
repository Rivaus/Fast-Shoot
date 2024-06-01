using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private Transform cam;

    [SerializeField]
    private float moveSpeed = 10, walkSpeed = 3, camRotationSpeed = 40, jumpForce = 10;

    [SerializeField]
    private Vector2 maxRotationX;

    private bool wantToJump, walk;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Vector3 movement = this.transform.right * Input.GetAxisRaw("Horizontal") + this.transform.forward * Input.GetAxisRaw("Vertical");

        this.rb.MovePosition(this.transform.position + movement.normalized * Time.fixedDeltaTime * (this.walk ? this.walkSpeed : this.moveSpeed));

        if (this.wantToJump)
        {
            this.rb.AddForce(Vector3.up * jumpForce);
            this.wantToJump = false;
        }
    }

    private void Update()
    {
        this.transform.Rotate(0, this.camRotationSpeed * Time.deltaTime * Input.GetAxisRaw("Mouse X"), 0);

        float mouseY = Input.GetAxisRaw("Mouse Y");
       
        /*if (this.cam.transform.localEulerAngles.x < this.maxRotationX.x || this.cam.transform.localEulerAngles.x > this.maxRotationX.y)
            Debug.Log(this.cam.transform.localEulerAngles.x);*/

        this.cam.Rotate(-this.camRotationSpeed * Time.deltaTime * mouseY, 0, 0, Space.Self);

        if (Input.GetKeyDown(KeyCode.Space) && !this.wantToJump)
        {
            this.wantToJump = true;
        }

        this.walk = !Input.GetKey(KeyCode.LeftShift);
    }
}
