using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] List<Image> menuItems;
    [SerializeField] List<Image> Back;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    int selectedItem = 0;

    private void Awake()
    {
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        menu.GetComponent<Image>().enabled = true;
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            selectedItem +=2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selectedItem -= 2;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            selectedItem++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            selectedItem--;

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);

        if (prevSelection != selectedItem)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z) | Input.GetKeyDown(KeyCode.Space))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            menu.GetComponent<Image>().enabled = false;

            onBack?.Invoke();
            CloseMenu();
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == selectedItem)
            {
                Back[i].color = Color.black;
            }
            else
            {
                Back[i].color = Color.white;
            }
        }
    }
}
