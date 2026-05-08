using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections;
using TMPro;


public class PlayerMovement : NetworkBehaviour
{
    public float speed;
    public float acceleration;
    public float deceleration;
    public float gravity;
    public CharacterController controller;
    public Camera cam;
    public AudioListener listener;
    public float jumpHeight;
    public PlayerControls controls;
    public TextMeshProUGUI SpeedUpText;
    public TextMeshProUGUI JumpUpText;
    private Coroutine speedRoutine;
    private Coroutine jumpRoutine;
    Vector3 velocity;
    Vector3 currentMove;

    void Awake()
    {
        controls = new PlayerControls();
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SpeedUpText = GameObject.Find("SpeedUpText").GetComponent<TextMeshProUGUI>();
            JumpUpText = GameObject.Find("JumpUpText").GetComponent<TextMeshProUGUI>();

            SpeedUpText.gameObject.SetActive(false);
            JumpUpText.gameObject.SetActive(false);
        }

        if (!IsOwner)
        {
            cam.gameObject.SetActive(false);
            listener.enabled = false;
        }
    }
    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (controller.isGrounded)
        {
            velocity.y = -2f;
            if (controls.Player.Jump.triggered)
            {
                velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
            }
            
        }

        Vector2 input = controls.Player.Move.ReadValue<Vector2>();

        float currentAccel;
        float x = input.x;
        float z = input.y;

        Vector3 move = transform.right * x + transform.forward * z;

        if (input == Vector2.zero || Vector3.Dot(currentMove, move) < 0)
        {
            currentAccel = deceleration;
        }
        else
        {
            currentAccel = acceleration;
        }

        currentMove = Vector3.Lerp(currentMove, move, currentAccel * Time.deltaTime);
        controller.Move(currentMove * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other.CompareTag("SpeedBoost"))
        {
            if(speedRoutine != null)
            {
                StopCoroutine(speedRoutine);
                speed -= 10f; 
            }
            speedRoutine = StartCoroutine(SpeedBoostRoutine());
            RequestPowerupDestructionServerRpc(other.gameObject);
        }

        if(other.CompareTag("JumpBoost"))
        {
            if(jumpRoutine != null)
            {
                StopCoroutine(jumpRoutine);
                jumpHeight -= 1.5f;
            }
            jumpRoutine = StartCoroutine(JumpBoostRoutine());
            RequestPowerupDestructionServerRpc(other.gameObject);
        }
    }
    [ServerRpc]
    void RequestPowerupDestructionServerRpc(NetworkObjectReference targer)
    {
        if (targer.TryGet(out NetworkObject obj))
        {
            obj.Despawn();
        }
    }
    private IEnumerator SpeedBoostRoutine()
    {
        SpeedUpText.gameObject.SetActive(true);
        speed += 10f;
        for(int i =5; i > 0; i--)
        {
            SpeedUpText.text = "Speed Boost: " + i + "s";
            yield return new WaitForSeconds(1f);
        }
        speed -= 10f;
        SpeedUpText.gameObject.SetActive(false);
        speedRoutine = null;
    }

    private IEnumerator JumpBoostRoutine()
    {
        JumpUpText.gameObject.SetActive(true);
        jumpHeight += 1.5f;
        for (int i = 5; i > 0; i--)
        {
            JumpUpText.text = "Jump Boost: " + i + "s";
            yield return new WaitForSeconds(1f);
        }
        jumpHeight -= 1.5f;
        JumpUpText.gameObject.SetActive(false);
        jumpRoutine = null;
    }

}
