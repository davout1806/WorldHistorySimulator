﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LoadFileDialogPanelScript : DialogPanelScript
{
    public GameObject FileListPanel;
    public Toggle TogglePrefab;

    public Text NoFilesText;

    public Text SelectButtonText;

    public Button SelectButton;
    public Button CancelButton;

    private List<Toggle> _fileToggles = new List<Toggle>();

    private string _basePath;
    private string _pathToLoad;

    private string[] _validExtensions;

    // Update is called once per frame
    void Update()
    {
        ReadKeyboardInput();
    }

    private void ReadKeyboardInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            CancelButton.onClick.Invoke();
        }
    }

    public string GetPathToLoad()
    {
        return _pathToLoad;
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void Initialize(
        string dialogText, 
        string selectButtonText, 
        UnityAction selectAction,
        UnityAction cancelAction,
        string basePath, 
        string[] validExtensions = null)
    {
        SetDialogText(dialogText);

        SelectButtonText.text = selectButtonText;

        SelectButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();

        SelectButton.onClick.AddListener(Hide);
        SelectButton.onClick.AddListener(selectAction);
        CancelButton.onClick.AddListener(Hide);
        CancelButton.onClick.AddListener(cancelAction);

        SelectButton.interactable = false;

        _basePath = basePath;
        _validExtensions = validExtensions;
    }

    private void LoadFileNames()
    {
        FileListPanel.SetActive(true);
        NoFilesText.gameObject.SetActive(false);

        _fileToggles.Add(TogglePrefab);

        string[] files = Directory.GetFiles(_basePath);

        int i = 0;

        foreach (string file in files)
        {
            string ext = Path.GetExtension(file).ToUpper();

            if (_validExtensions != null)
            {
                bool found = false;
                foreach (string validExt in _validExtensions)
                {
                    found |= ext == validExt.ToUpper();
                }

                if (!found) continue;
            }

            string name = Path.GetFileName(file);

            SetFileToggle(name, i);

            i++;
        }

        if (i == 0)
        {
            FileListPanel.SetActive(false);

            string extTypes = string.Join(",", _validExtensions);

            NoFilesText.text = "No files of type {" + extTypes + "} found...";
            NoFilesText.gameObject.SetActive(true);
        }
    }

    private void SetFileToggle(string name, int index)
    {
        Toggle toggle;

        if (index < _fileToggles.Count)
        {
            toggle = _fileToggles[index];
            toggle.GetComponentInChildren<Text>().text = name;
        }
        else
        {
            toggle = AddFileToggle(name);
        }

        toggle.onValueChanged.RemoveAllListeners();

        string path = _basePath + name;

        toggle.onValueChanged.AddListener(value =>
        {
            if (value)
            {
                _pathToLoad = path;
                SelectButton.interactable = true;
            }
        });
    }

    private Toggle AddFileToggle(string name)
    {
        Toggle newToggle = Instantiate(TogglePrefab) as Toggle;

        newToggle.transform.SetParent(FileListPanel.transform, false);
        newToggle.GetComponentInChildren<Text>().text = name;

        _fileToggles.Add(newToggle);

        return newToggle;
    }

    private void ResetToggles()
    {
        bool first = true;

        foreach (Toggle toggle in _fileToggles)
        {
            toggle.isOn = false;

            if (first)
            {
                first = false;
                continue;
            }

            GameObject.Destroy(toggle.gameObject);
        }

        _fileToggles.Clear();
    }

    public override void SetVisible(bool state)
    {
        base.SetVisible(state);

        if (state)
        {
            LoadFileNames();
        }
        else
        {
            ResetToggles();
        }
    }
}
