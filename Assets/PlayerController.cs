using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInput;

[RequireComponent(typeof(Inventory))]
public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float jForce;
    [SerializeField] float mvSpeed;
    [SerializeField] GameObject cameraRot;
    [SerializeField] GameObject cameraRot2;
    [SerializeField] GameObject bulletRef;
    [SerializeField] GameObject bombRef;
    [SerializeField] GameObject meleeHurtboxRef;
    Inventory inventory;
    PlayerInput controls;
    [SerializeField] float kbForce = 300f;
    [SerializeField] float shootSpd = 4f;
    [SerializeField] float shootBombSpd = 2f;

    

    public float winTimer = 0f;

    Vector2 forward
    {
        get
        {

            float sin = Mathf.Sin(-1 * cameraRot.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
            float cos = Mathf.Cos(-1 * cameraRot.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
            return new Vector2(-sin, cos);
        }
    }

    Coroutine shootCoro = null;

    bool onGround;
    

    public void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerInput();
            // Tell the "gameplay" action map that we want to get told about
            // when actions get triggered.
            //controls.Newactionmap.SetCallbacks(this);
        }
        controls.Newactionmap.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory>();
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (controls.Newactionmap.Jump.ReadValue<float>()>.5 && onGround)
        {
            rb.AddForce(new Vector3(0f,jForce,0f));
        }


        if (controls.Newactionmap.Shoot.ReadValue<float>() > .5 && shootCoro ==null)
        {
            shootCoro = StartCoroutine(Shoot());
        }

        Vector2 axis = controls.Newactionmap.Move.ReadValue<Vector2>();
        float sin = Mathf.Sin(-1*cameraRot.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
        float cos = Mathf.Cos(-1*cameraRot.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
        axis = new Vector2(cos * axis.x - sin * axis.y, sin * axis.x + cos * axis.y);
        rb.AddForce(new Vector3(mvSpeed * axis.x, 0, mvSpeed * axis.y));


        Vector2 aim = controls.Newactionmap.Aim.ReadValue<Vector2>();

        cameraRot.transform.Rotate(new Vector3(0, 1, 0), aim.x, Space.World);
        cameraRot2.transform.Rotate(new Vector3(aim.y,0, 0));
    }


    public void OnMove(InputAction.CallbackContext context)
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            onGround = true;
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            onGround = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            onGround = false;
        }
    }

    IEnumerator Shoot()
    {
        if (inventory.seedCount <= 0)
        {
            yield return null;
            shootCoro = null;
        }
        else
        {
            inventory.seedCount--;
            GameObject bullet = Instantiate(bulletRef);
            bullet.transform.position = transform.position;
            bullet.GetComponent<Rigidbody>().velocity = new Vector3(
                forward.x,
                -Mathf.Sin(cameraRot2.transform.rotation.eulerAngles.x * Mathf.Deg2Rad),
                forward.y).normalized * shootSpd;
            bullet.GetComponent<GameObjectRef>().go.Add(this.gameObject);
            bullet.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            yield return new WaitForSeconds(.5f);
            shootCoro = null;

        }


    }
    IEnumerator ShootBomb()
    {
        if (inventory.acornCount <= 0)
        {
            yield return null;
            shootCoro = null;
        }
        else
        {
            inventory.acornCount--;
            GameObject bomb = Instantiate(bombRef);
            bomb.transform.position = transform.position;
            float sin = Mathf.Sin(-1 * cameraRot.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
            float cos = Mathf.Cos(-1 * cameraRot.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
            bomb.GetComponent<Rigidbody>().velocity = new Vector3(
                forward.x,
                -Mathf.Sin(cameraRot2.transform.rotation.eulerAngles.x * Mathf.Deg2Rad) + 1,
                forward.y).normalized * shootBombSpd;
            bomb.GetComponent<GameObjectRef>().go.Add(gameObject);
            bomb.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            yield return new WaitForSeconds(.5f);
            shootCoro = null;
        }


    }
    IEnumerator Melee()
    {
        GameObject meleeHB = Instantiate(meleeHurtboxRef);
        meleeHB.transform.position = transform.position + new Vector3(forward.normalized.x,0,forward.normalized.y)*.1f;
        meleeHB.GetComponent<GameObjectRef>().go.Add(gameObject);
        yield return new WaitForSeconds(.5f);
        Destroy(meleeHB);
        shootCoro = null;


    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("WinZone"))
        {
            winTimer += Time.deltaTime;
        }
    }

}