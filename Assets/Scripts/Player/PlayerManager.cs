using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerHealth playerHealth = new();
    private PlayerMovement playerMovement;
    private PlayerWeapon playerWeapon;

    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerWeapon PlayerWeapon => playerWeapon;

    private void Awake()
    {
        SessionController.Instance.PlayerManager = this;
    }

}
