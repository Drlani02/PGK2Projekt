using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class PlayerState : NetworkBehaviour
{
    public enum PlayerRoleEnum
    {
        Hunter,
        Runner
    }
    public NetworkVariable<PlayerRoleEnum> CurrentRole = new NetworkVariable<PlayerRoleEnum>(PlayerRoleEnum.Runner);
    
    Collider playerCollider;
    public void PlayerColided(PlayerState otherPlayer)
    {
        if(!IsServer) return;
        if (CurrentRole.Value == PlayerRoleEnum.Runner)
        {
            Debug.Log("Player collided with another player. Switching roles.");
            CurrentRole.Value = PlayerRoleEnum.Hunter;
            playerCollider.enabled = false;
            otherPlayer.CurrentRole.Value = PlayerRoleEnum.Runner;
            otherPlayer.playerCollider.enabled = true;
        }
        else if(CurrentRole.Value == PlayerRoleEnum.Hunter)
        {
            Debug.Log("Player collided with another player. Switching roles.");
            CurrentRole.Value = PlayerRoleEnum.Runner;
            playerCollider.enabled = true;
            otherPlayer.CurrentRole.Value = PlayerRoleEnum.Hunter;
            otherPlayer.playerCollider.enabled = false;

        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerState otherPlayerState = collision.gameObject.GetComponent<PlayerState>();
            if (otherPlayerState != null)
            {
                PlayerColided(otherPlayerState);
            }
        }
    }

    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
