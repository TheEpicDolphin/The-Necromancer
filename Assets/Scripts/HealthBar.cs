using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private int maxHearts = 10;
    private int maxHealth;
    public Image[] heartImages;
    public Sprite[] heartSprites;

    private int currentHealth;

    public int healthColor;

    // Start is called before the first frame update
    void Start()
    {
        SetMaxHealth(maxHearts);
        currentHealth = maxHearts;
    }

    public void SetMaxHealth(int max){
        maxHealth = max;
        for (int i = 0; i < maxHearts; i++){
            if (maxHealth <= i){
                heartImages[i].enabled = false;
            } else{
                heartImages[i].enabled = true;
                heartImages[i].sprite = heartSprites[4];
            }
        }
    }

    public void SetCurrentHealth(){

        for (int i = 0; i < maxHealth; i++){
            if (i < currentHealth){
                heartImages[i].sprite = heartSprites[4];
            } else
            {
                heartImages[i].sprite = heartSprites[5];
            }
        }
    }

    // Decrease health by one. Returns true if character is dead, false if character is alive
    public bool DecrementHealth(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        SetCurrentHealth();
        if (currentHealth == 0)
        {
            return true;
        }
        return false;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
