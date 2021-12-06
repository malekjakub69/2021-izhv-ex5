using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The main game manager GameObject.
/// </summary>
public class GameManager : MonoBehaviour
{
#region Editor
#endregion // Editor

#region Internal

    /// <summary>
    /// Did we start the game?
    /// </summary>
    private static bool sGameStarted = false;
    
    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    private static GameManager sInstance;
    
    /// <summary>
    /// Getter for the singleton GameManager object.
    /// </summary>
    public static GameManager Instance
    { get { return sInstance; } }
    
#endregion // Internal

    /// <summary>
    /// Called when the script instance is first loaded.
    /// </summary>
    private void Awake()
    {
        // Initialize the singleton instance, if no other exists.
        if (sInstance != null && sInstance != this)
        { Destroy(gameObject); }
        else
        { sInstance = this; }
    }

    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        // Setup the game scene.
        SetupGame();
    }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    { }

    /// <summary>
    /// Setup the game scene.
    /// </summary>
    public void SetupGame()
    { sGameStarted = false; }

    /// <summary>
    /// Set the game to the "started" state.
    /// </summary>
    public void StartGame()
    {
        // Reload the scene as started.
        sGameStarted = true; 
        ResetGame();
    }
    
    /// <summary>
    /// Reset the game to the default state.
    /// </summary>
    public void ResetGame()
    {
        // Reload the active scene, triggering reset...
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Toggle the development User Interface.
    /// </summary>
    public void ToggleDevUI()
    {
        var devUI = GameSettings.Instance.devUI;
        devUI.SetActive(!devUI.activeSelf);
    }
}
