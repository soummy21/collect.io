using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool enableControls = true;
    [SerializeField] TextMeshProUGUI playerName;

    private PhotonView photonView;
    public PlayerManager playerManager { get; private set; }

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = transform.GetChild(0).GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            playerName.text = PhotonNetwork.NickName;

        }else
        {
            playerName.text = photonView.Owner.NickName;
        }
    }
    
    private void OnEnable()
    {
        GameMessages.OnPauseGame += StopInput;
        GameMessages.OnPlayGame += StartInput;
        GameMessages.OnGameSessionEnded += StopInput;
    }

    private void OnDisable()
    {
        GameMessages.OnPauseGame -= StopInput;
        GameMessages.OnPlayGame -= StartInput;
        GameMessages.OnGameSessionEnded -= StopInput;
    }

    private void FixedUpdate()
    {
        if (!enableControls) return;

        if (photonView.IsMine)
        {
            MovePlayer();
            FlipSprite();
        }


    }

    private void StopInput()
    {
        if(photonView.IsMine)
        {
            playerRigidbody.velocity = Vector2.zero;
            enableControls = false;
        }

    }

    private void StartInput()
    {   
        if(photonView.IsMine) enableControls = true;

    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.GetChild(0).localScale = new Vector2(Mathf.Sign(playerRigidbody.velocity.x), transform.localScale.y);

        }
    }

    private void MovePlayer()
    {
        //Input and Movement
        var inputData = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        playerRigidbody.velocity = inputData * speed * Time.deltaTime;

        //Animation
        bool isMoving = playerRigidbody.velocity.sqrMagnitude > Mathf.Epsilon;
        playerAnimator.SetBool(Identifiers.Move, isMoving);
    }


    public void SetPlayerManager(PlayerManager player)
    {
        playerManager = player;

        //Flip sprite for Team B
        if(playerManager.myTeam == Teams.B)
        {
            transform.GetChild(0).localScale = new Vector2(-1, transform.localScale.y);
        }
            

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        GameplayEvents.UpdateScoreboard?.Invoke(playerManager.myTeam);

    }



}
