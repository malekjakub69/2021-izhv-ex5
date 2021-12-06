using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

/// <summary>
/// The main Player script.
/// </summary>
public class Player : MonoBehaviour
{
#region Editor

    /// <summary>
    /// Camera following the Player character.
    /// </summary>
    [ Header("Global") ]
    public Camera mainCamera;
    
    /// <summary>
    /// Current speed of the Player.
    /// </summary>
    [ Header("Gameplay") ]
    public float speed = 1.0f;
    
#endregion // Editor

#region Internal
    
    /// <summary>
    /// Main RigidBody of the player model.
    /// </summary>
    private Rigidbody mRigidBody;

    /// <summary>
    /// Input controller for this player.
    /// </summary>
    private PlayerInput mPlayerInput;
    
    /// <summary>
    /// Requested look position.
    /// </summary>
    private Vector2 mLookInput = Vector2.zero;
    
    /// <summary>
    /// Requested movement input.
    /// </summary>
    private Vector2 mMoveInput = Vector2.zero;
    
    /// <summary>
    /// Requested zoom input.
    /// </summary>
    private float mZoomInput = 0.0f;
    
#endregion // Internal
    
    /// <summary>
    /// Called when the script instance is first loaded.
    /// </summary>
    private void Awake()
    {
        // Fetch the main camera if we did not receive any other.
        if (mainCamera == null)
        { mainCamera = GameSettings.Instance.mainCamera; }
        
        mRigidBody = GetComponent<Rigidbody>();
        mPlayerInput = GetComponent<PlayerInput>();
    }

    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    { }
    
    // Input System Callbacks: 
    public void OnLook(InputAction.CallbackContext ctx)
    { mLookInput = ctx.ReadValue<Vector2>(); }
    public void OnMove(InputAction.CallbackContext ctx)
    { mMoveInput = ctx.ReadValue<Vector2>(); }
    public void OnZoom(InputAction.CallbackContext ctx)
    { if (ctx.started) { mZoomInput = ctx.ReadValue<float>() / -120.0f; } }
    public void OnToggleDevUI(InputAction.CallbackContext ctx)
    { GameManager.Instance.ToggleDevUI(); }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    { }

    /// <summary>
    /// Update called at fixed time delta.
    /// </summary>
    void FixedUpdate()
    { }
    
    /// <summary>
    /// Move the player in given direction, using the curent speed.
    /// </summary>
    /// <param name="direction">Movement direction.</param>
    public void MovePlayer(Vector3 direction)
    {
        mRigidBody.MovePosition(
            transform.position + 
            direction.normalized * speed * Time.deltaTime
        );
    }
    
    /// <summary>
    /// Rotate the player to face the given position.
    /// </summary>
    /// <param name="target">Position to rotate towards.</param>
    public void RotatePlayer(Vector3 target)
    {
        // Get the direction vector.
        var playerToTarget = target - transform.position;
        
        // Allow only rotation in the XZ plane (play area).
        playerToTarget.y = 0.0f;
        playerToTarget.Normalize();
        
        // Rotate towards it.
        mRigidBody.MoveRotation(Quaternion.LookRotation(playerToTarget));
    }

    /// <summary>
    /// Calculate smoothed version of the given zoom input.
    /// </summary>
    /// <param name="rawZoom">Raw zoom input <-1.0f, 1.0f></param>
    /// <returns>Returns smoothed version.</returns>
    private float CalculateSmoothZoom(float rawZoom)
    {
        var smoothZoom = rawZoom * GameSettings.Instance.zoomAttune;
        return Math.Abs(smoothZoom) < GameSettings.Instance.zoomCutoff ? 0.0f : smoothZoom;
    }
}
