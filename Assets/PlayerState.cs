using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerState : NetworkBehaviour
{
    private TextMeshProUGUI roleText;
    public enum PlayerRoleEnum
    {
        Hunter,
        Runner
    }
    public NetworkVariable<float> timeEscaped = new NetworkVariable<float>(0f);
    public NetworkVariable<PlayerRoleEnum> CurrentRole = new NetworkVariable<PlayerRoleEnum>(PlayerRoleEnum.Runner);
    
    Collider playerCollider;
    public void PlayerCollided(PlayerState otherPlayer)
    {
        if(!IsServer) return;
        if(CurrentRole.Value == PlayerRoleEnum.Hunter && otherPlayer.CurrentRole.Value == PlayerRoleEnum.Runner) 
            GameManager.Instance.CatchRunner();

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerState otherPlayerState = other.GetComponent<PlayerState>();
            if (otherPlayerState != null)
            {
                PlayerCollided(otherPlayerState);
            }
        }
    }

    [ClientRpc]
    public void TeleportClientRpc(Vector3 newPosition)
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = newPosition;

        if (cc != null) cc.enabled = true;
    }

    void Start()
    {
        playerCollider = GetComponent<Collider>();
        roleText = GameObject.FindWithTag("RoleText").GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        if(!IsLocalPlayer) return;
        if (CurrentRole.Value == PlayerRoleEnum.Runner)
        {
            roleText.text = "Role: Runner";
        }
        else if (CurrentRole.Value == PlayerRoleEnum.Hunter)
        {
            roleText.text = "Role: Hunter";
        }
    }
}
