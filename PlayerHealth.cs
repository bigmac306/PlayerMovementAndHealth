using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    [Space]
    public Transform spwanPoint;
    public PlayerMovement playerMovement;

    [HideInInspector] public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void OnTakeDamage(int _damage)
    {
        currentHealth -= _damage;
        if (currentHealth <= 0)
        {
            death();  
        }
    }

    IEnumerator ExecuteWithDelay()
    {
        yield return new WaitForSeconds(3f);
    }
    
    public void HealPlayer(int _healAmount)
    {
        currentHealth += _healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    void death()
    {
        playerMovement.PlayerCrouch();
        StartCoroutine(ExecuteWithDelay());
        transform.position = spwanPoint.position;
        playerMovement.FixPlayerCrouchHight();
        currentHealth = maxHealth;
    }
}