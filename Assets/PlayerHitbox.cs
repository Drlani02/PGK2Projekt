using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    private PlayerState myMainPlayerState;


    void Start()
    {
        myMainPlayerState = GetComponentInParent<PlayerState>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (myMainPlayerState.CurrentRole.Value == PlayerState.PlayerRoleEnum.Runner) return;
        if (other.CompareTag("PlayerHitbox") && other.gameObject != this.gameObject)
        {
            PlayerHitbox otherHitbox = other.GetComponent<PlayerHitbox>();
            if (otherHitbox != null)
            {
                myMainPlayerState.PlayerCollided(otherHitbox.myMainPlayerState);
            }
        }
    }
}