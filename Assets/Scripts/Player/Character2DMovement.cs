using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Simple 2D character movement processor.
/// </summary>
public class Character2DMovement : MonoBehaviour
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 4.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 6.0f;
	[Tooltip("Rotation speed of the character")]
	public float RotationSpeed = 1.0f;
	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f;
	[Tooltip("Animation divider for the movement speed")]
	public float MoveSpeedAnimation = 6.0f;

	[Space(10)]
	[Tooltip("The maximum jump speed")]
	public float JumpSpeed = 1.2f;
	[Tooltip("Ramping of the jump speed")]
	public float JumpChangeRate = 0.1f;
	[Tooltip("Maximum time a jump can be held")]
	public float JumpDuration = 0.5f;
	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = 15.0f;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.1f;
	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	private float mTargetHorSpeed;
	private float mHorizontalSpeed;
	private float mTargetVerSpeed;
	private float mVerticalSpeed;
	private float mAnimationBlend;
	private float mTerminalVelocity = -53.0f;

	private float mJumpTimeoutDelta;
	private float mJumpDurationDelta;
	private float mFallTimeoutDelta;

	private bool mHeadingRight;
	
	private Character2DController mController;
	private CharacterSelector mSelector;
	private InputManager mInput;
	
    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        mController = GetComponent<Character2DController>();
        mSelector = GetComponent<CharacterSelector>();
        mInput = GetComponent<InputManager>();

        mTargetHorSpeed = 0.0f;
        mTargetVerSpeed = 0.0f;

        mJumpTimeoutDelta = JumpTimeout;
        mJumpDurationDelta = 0.0f;
        mFallTimeoutDelta = FallTimeout;

        mHeadingRight = true;
    }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    {
	    mTargetHorSpeed = mInput.sprint ? SprintSpeed : MoveSpeed;
	    if (mInput.move == Vector2.zero)
	    { mTargetHorSpeed = 0.0f; }
    }
    
    /// <summary>
    /// Update called at fixed intervals.
    /// </summary>
    void FixedUpdate ()
    {
	    MoveHorizontal();
	    JumpAndGravity();
	    AnimateCharacter();
	    
		var movement = new Vector3(
			mHorizontalSpeed * Math.Sign(mInput.move.x), 
			mVerticalSpeed, 
			0.0f
		);
		
	    mController.Move(movement * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Perform horizontal movement calculations.
    /// </summary>
    void MoveHorizontal()
    {
		var currentHorizontalSpeed = new Vector3(mController.velocity.x, 0.0f, mController.velocity.z).magnitude;

		var speedOffset = 0.1f;
		var inputMagnitude = mInput.analogMovement ? Math.Abs(mInput.move.x) : 1.0f;

		if (currentHorizontalSpeed < mTargetHorSpeed - speedOffset || 
		    currentHorizontalSpeed > mTargetHorSpeed + speedOffset)
		{
			mHorizontalSpeed = Mathf.Lerp(
				currentHorizontalSpeed, 
				mTargetHorSpeed * inputMagnitude, 
				Time.fixedDeltaTime * SpeedChangeRate
			);
			mHorizontalSpeed = Mathf.Round(mHorizontalSpeed * 1000f) / 1000f;
		}
		else
		{ mHorizontalSpeed = mTargetHorSpeed; }
    }
    
    /// <summary>
    /// Perform vertical movement calculations.
    /// </summary>
	private void JumpAndGravity()
	{
		if (mController.isGrounded)
		{
			mFallTimeoutDelta = FallTimeout;

			if (mInput.jump && mJumpTimeoutDelta <= 0.0f)
			{
				mTargetVerSpeed = Mathf.Sqrt(JumpSpeed * 2.0f * Gravity);
				mJumpTimeoutDelta = JumpTimeout;
				mJumpDurationDelta = JumpDuration; 
			}
			else
			{ mTargetVerSpeed = mVerticalSpeed; }
			
			if (mJumpTimeoutDelta >= 0.0f)
			{ mJumpTimeoutDelta -= Time.fixedDeltaTime; }
		}
		else
		{
			mTargetVerSpeed = mInput.jump && mJumpDurationDelta >= 0.0f
				? Mathf.Sqrt(JumpSpeed * 2.0f * Gravity)
				: mVerticalSpeed;
			
			if (mJumpDurationDelta >= 0.0f)
			{ mJumpDurationDelta -= Time.fixedDeltaTime; }
			
			if (mFallTimeoutDelta >= 0.0f)
			{ mFallTimeoutDelta -= Time.fixedDeltaTime; }
		}
		
		var currentVerticalSpeed = mController.velocity.y;
		
		var speedOffset = 0.1f;
		var inputMagnitude = 1.0f;

		if (currentVerticalSpeed < mTargetVerSpeed - speedOffset || 
			currentVerticalSpeed > mTargetVerSpeed + speedOffset)
		{
			mVerticalSpeed = Mathf.Lerp(
				currentVerticalSpeed, 
				mTargetVerSpeed * inputMagnitude, 
				Time.fixedDeltaTime * JumpChangeRate
			);
			mVerticalSpeed = Mathf.Round(mVerticalSpeed * 1000f) / 1000f;
		}
		else
		{ mVerticalSpeed = mTargetVerSpeed; }
		
		if (mVerticalSpeed > mTerminalVelocity)
		{ mVerticalSpeed -= Gravity * Time.fixedDeltaTime; }
	}

    /// <summary>
    /// Run animation according to the current state.
    /// </summary>
    ///
    /// 
    void AnimateCharacter()
    {
	    /*
	     * Task #1a: Orienting the character
	     *
	     * Let us start by at least orienting the character, making him face the
	     * correct way based on the desired direction of movement. Currently, when
	     * walking left or right, the sprite is always looking the same.
	     * 
	     * To fix this, we need to orient the character properly. This can be done
	     * in several ways, but the easiest is to modify the transform of the GO
	     * based on its intended orientation.
	     *
	     * Let's presume that by default the character is heading right, represented
	     * by the member variable *mHeadingRight* being true. You can detect direction
	     * we are driving the character to go by examining the value of *mInput.move.x*.
	     *
	     * Now, modify the GO transform (hint: either scale or rotation will work) to
	     * fix this problem.
	     *
	     * Helpful variables and methods:
	     *   * Transform of the Game Object: *transform*
	     *   * Scale and Rotation: *transform.localScale* and *transform.localRotation*
	     *   * Input direction: *mInput.move.x*
	     *   * Persistent heading flag: *mHeadingRight*
	     *   * Rotating a local rotation by an axis: localRotation *= Quaternion.Euler(...)
	     */
	    if (mInput.move.x  < 0)
	    {
		    if (mHeadingRight)
		    {
			    transform.localRotation *= Quaternion.Euler(0, 180, 0);
			    mHeadingRight = false;
		    }
	    }
	    else if (mInput.move.x > 0)
	    {		    
		    if (!mHeadingRight)
		    {
			    transform.localRotation *= Quaternion.Euler(0, 180, 0);
			    mHeadingRight = true;
		    }
		    
	    }

	    var animator = mSelector.charAnimator;
	    if (animator != null)
	    {
			var currentVerticalSpeed = mController.velocity.y;
			var currentHorizontalSpeed = new Vector3(mController.velocity.x, 0.0f, mController.velocity.z).magnitude;
			
			// Property values: 
			var speed = currentHorizontalSpeed;
			var moveSpeed = Math.Abs(mTargetHorSpeed / MoveSpeedAnimation);
			var crouch = mInput.crouch;
			var grounded = mController.isGrounded;
			var jump = mInput.jump;
			var falling = !mController.isGrounded && mFallTimeoutDelta <= 0.0f;

			animator.SetFloat("Speed",speed);
			animator.SetBool("Jump", jump);
			animator.SetBool("Grounded", grounded);
			animator.SetBool("Crouch", crouch);
			animator.SetFloat("MoveSpeed", moveSpeed);


			/*
			 * Task #1a: Passing properties to the Animator
			 * 
			 * After rotating the character, he should now be able to look in the
			 * correct direction, based on the movement. However, more detailed
			 * animations are still missing.
			 *
			 * To fix this, you will need to pass the current state of the character
			 * to the Animator. Each Animator in Unity has several properties which
			 * can be programmatically set during runtime. These properties can be
			 * used to drive the animation from this gameplay code.
			 * 
			 * In our case, we have following common properties:
			 *   1) Speed (float) : Current movement speed of the character.
			 *   2) MoveSpeed (float) : Target movement animation speed.
			 *   3) Jump (bool) : Is the character jumping?
			 *   4) Grounded (bool) : Is the character currently on the ground?
			 *   5) Fall (bool) : Is the character falling?
			 *   6) Crouch (bool) Is the character crouching?
			 * These properties can be found in ani of the animation controllers
			 * in the top-left "Parameters" tab.
			 *
			 * When you start the game and try performing any of the actions,
			 * such as walking (WASD), jumping (<SPACE>), or crouching (<CTRL>),
			 * the corresponding animation is not played.
			 *
			 * To fix this, we need to actually set the animator properties using
			 * its methods. This way, the gameplay code triggers the corresponding
			 * states within the Animator state machine.
			 * 
			 * The correct property values have been prepared above. After setting
			 * the values, all of the animations should play correctly.
			 *
			 * Helpful variables and methods:
			 *   * Property values prepared above
			 *   * Current Animator instance: *animator*
			 *   * Animator methods: *SetFloat* and *SetBool*
			 */
	    }
    }
}
