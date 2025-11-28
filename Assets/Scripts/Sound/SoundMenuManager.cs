using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject soundMenu;
    private bool isActive = false;

    private void Start()
    {
        soundMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (soundMenu != null)
            {
                isActive = !isActive;
                soundMenu.SetActive(isActive);
            }
        }
    }
}
