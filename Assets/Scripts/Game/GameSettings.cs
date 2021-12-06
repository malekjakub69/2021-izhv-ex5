using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ReSharper disable InvalidXmlDocComment

/// <summary>
/// Container for settings used in the game.
/// </summary>
public class GameSettings : MonoBehaviour
{
#region Editor

    // References to GameObjects within the scene.
    [ Header("Game Objects") ]

    /// <summary>
    /// Reference to the main game manager.
    /// </summary>
    public GameObject gameManager;
    
    /// <summary>
    /// Reference to the player.
    /// </summary>
    public GameObject player;

    /// <summary>
    /// Main camera used for viewing the scene.
    /// </summary>
    public Camera mainCamera;

    /// <summary>
    /// Root of the developer UI.
    /// </summary>
    public GameObject devUI;
    
    [Header("Control Settings")]
    /// <summary>
    /// Attenuation for the smooth zoom.
    /// </summary>
    public float zoomAttune = 0.6f;
    
    /// <summary>
    /// Cutoff value for the smooth zoom.
    /// </summary>
    public float zoomCutoff = 0.1f;
    
    // Game Settings
    [Header("Game Settings")] 
    
    // World Settings
    [ Header("World Settings") ]
    
    /// <summary>
    /// Mask for the objects in the ground layer.
    /// </summary>
    public LayerMask groundLayer;

#endregion // Editor

#region Internal

    /// <summary>
    /// Currently used GameManager component.
    /// </summary>
    [CanBeNull] 
    private GameManager mGameManager;
    
    /// <summary>
    /// Singleton instance of the Settings.
    /// </summary>
    private static GameSettings sInstance;
    
    /// <summary>
    /// Getter for the singleton Settings object.
    /// </summary>
    public static GameSettings Instance
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
        
        if (gameManager)
        { mGameManager = gameManager.GetComponent<GameManager>(); }
    }
    
    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    { }
}
