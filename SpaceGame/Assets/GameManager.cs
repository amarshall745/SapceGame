using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool pause;
    private bool LockCursor;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        
    }
    void Start()
    {
        lockCursor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void nextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void lockCursor()
    {
        if (LockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            LockCursor = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            LockCursor = true;
        }
    }
}
