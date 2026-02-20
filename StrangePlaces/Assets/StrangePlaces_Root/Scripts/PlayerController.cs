using System.Collections;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region General Variables
    [SerializeField] GameObject walkDust;
    [SerializeField] float movementMult = 1;//cuando el player se mueva, consumirá más
    //[SerializeField] float secondsOfEnergy = 180f;//3 o 4 minutos

    [Header("Movement & Interaction")]
    [SerializeField] float speed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float maxForce = 1f; //Fuerza máxima de aceleración

    [SerializeField] bool isSprinting;

    [SerializeField] float interactingCooldown = 0.1f;
    [SerializeField] bool canInteract = true;

    [Header("GroundCheck")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] GameObject groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] bool isGrounded;

    //Input Variables
    [Header("References for UI")]
    [SerializeField] GameObject menuCamera;

    [SerializeField] GameObject adviceDialogue;//pasar a dialogueManager? o notification manager
    [SerializeField] TMP_Text adviceText;

    [SerializeField] bool playerPaused;//quitar esta o la siguiente? o no?
    bool interacting;
    Vector2 moveInput;
    [Header("Player References")]
    [SerializeField] Rigidbody playerRb;
    [SerializeField] Animator anim;
    [SerializeField] RectTransform camCompass;
    [SerializeField] AudioSource playerSpeaker;

    [SerializeField] Transform camTransform;
    [SerializeField] Transform holdingPoint;
    [SerializeField] bool hasTurned = false;
    [SerializeField] float rotationTime = 20f;


    [SerializeField] float timeSinceMove;
    Quaternion mapRotation;
    bool npcInRange;
    #endregion

    void Update()
    {
        //if (GameManager.Instance.playerInDialogue) playerPaused = true;
        //else playerPaused = false;

        GroundCheck();

        if (!playerPaused)//congelar también las stats? o solo en las cabinas telefónicas
        {
            if (interacting) StartCoroutine(InteractRoutine());
        }

        timeSinceMove += Time.deltaTime;

        if (timeSinceMove >= 20f)
        {
            //anim.SetTrigger("varyIdle");
            timeSinceMove = -10;
        }
    }
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundCheckRadius, groundLayer);
    }
    private void FixedUpdate()
    {
        if (!playerPaused && !GameManager.Instance.playerInDialogue) //&& !GameManager.Instance.menuOpened)
        {
            Movement();
            //poner que la siga a la camara en vez de al personaje
            if (camCompass != null)
            {
                //rota flecha
                //camCompass.rotation = Quaternion.Euler(0f, 0f, GameManager.Instance.camController.transform.eulerAngles.y);
                //rota cámara
                //mapRotation = GameManager.Instance.mapCamera.rotation;
                //GameManager.Instance.mapCamera.rotation = Quaternion.Euler(90f, GameManager.Instance.camController.transform.eulerAngles.y, 0f);
            }
        }
    }
    void Movement() //añadir que tolere escalones ligeros
    {
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;
        if (!hasTurned && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationTime * Time.deltaTime);
        }

        Vector3 currentVelocity = playerRb.linearVelocity;
        Vector3 targetVelocity = moveDirection;
        targetVelocity *= isSprinting ? sprintSpeed : speed;

        // Calcular el cambio de velocidad (aceleración)
        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);
        if (moveInput.x != 0 || moveInput.y != 0)
        {
            if (movementMult != 2) movementMult = 2f;
            timeSinceMove = 0;
            anim.SetBool("isWalking", true);
            //anim.SetInteger("playerState", isSprinting ? 2 : 1);
        }
        else
        {
            movementMult = 1f;
            isSprinting = false;
            if (anim.GetBool("isWalking"))
            {
                timeSinceMove = 0;
                anim.SetBool("isWalking", false);
            }
        }
        playerRb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    public void NewAdvice(string adviceToSay)
    {
        adviceDialogue.SetActive(false);
        adviceDialogue.SetActive(true);
        //AudioManager.Instance.PlaySound(0, false);
        adviceText.text = adviceToSay;
    }
    void Interact()
    {
       if(canInteract && npcInRange)
       {
            if (anim.GetBool("isWalking")) anim.SetBool("isWalking", false);
            playerRb.linearVelocity = Vector3.zero;
            if (GameManager.Instance.heldObjectMesh != null)
            {
                GameManager.Instance.heldObjectMesh.transform.parent = holdingPoint;
                GameManager.Instance.heldObjectMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
            DialogueManager.Instance.DialogueCall();
       }
    }
    IEnumerator InteractRoutine()
    {
        interacting = false;
        if (canInteract) Interact();
        canInteract = false;
        yield return new WaitForSeconds(interactingCooldown);
        canInteract = true;
    }
    void Jump()
    {
        if (isGrounded) playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("NPC") || other.gameObject.CompareTag("Interactable") || other.gameObject.CompareTag("Pickable"))
        {
            DialogueManager.Instance.RegisterInfo(other.gameObject.GetComponent<DialogueInfo>());
            npcInRange = true;
            if (other.gameObject.CompareTag("Pickable")) GameManager.Instance.heldObjectMesh = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("NPC") || other.gameObject.CompareTag("Interactable") || other.gameObject.CompareTag("Pickable"))
        {
            DialogueManager.Instance.RegisterInfo(null);
            npcInRange = false;
            if (other.gameObject.CompareTag("Pickable")) GameManager.Instance.heldObjectMesh = null;
        }
    }
    #region Input Methods
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !playerPaused) interacting = true;
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !playerPaused) isSprinting = !isSprinting;
        //cambiar input actions para que sea doble toque de tecla/(movimiento rápido/apretar joystick)
    }

    #endregion
}