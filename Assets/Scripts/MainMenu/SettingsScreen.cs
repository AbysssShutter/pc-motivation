using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsScreen : MonoBehaviour
{
    private bool isActive = false;

    public void ShowOrClose() {
        isActive = !isActive;
        gameObject.SetActive(isActive);
    }
}
