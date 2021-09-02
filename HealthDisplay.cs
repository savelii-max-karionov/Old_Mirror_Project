using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private Image healthBarImage = null;

    private void Awake()
    {
        // subscribing to the event that will fire in the health script
        // when the current health changes
        health.ClientOnHealthUpdated += HandleHealthUpdated;
    }

    private void OnDestroy()
    {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    // default unity methond when the mouse enters the objects collider/trigger
    private void OnMouseEnter()
    {
        healthBarParent.SetActive(true);
    }

    private void OnMouseExit()
    {
        healthBarParent.SetActive(false);
    }

    // this functions get's called when the event is fired
    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        // fillAmout takes the value between 0 and 1
        healthBarImage.fillAmount = (float) currentHealth / maxHealth;
    }


}
