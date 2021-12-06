using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// Script used for changing the current player character.
/// </summary>
public class CharacterSelector : MonoBehaviour
{
	[Header("Character")] 
	[Tooltip("List of available character to switch between")]
	public List<GameObject> characters;
	
	/// <summary>
	/// Currently used character prefab, starting with 1 for the first character.
	/// </summary>
	private int mCurrentCharacter = 0;
	
	private InputManager mInput;
	private GameObject mCharacter;
	private Animator mAnimator;
	private BoxCollider2D mBoxCollider;
	
	/// <summary>
	/// Current character animator.
	/// </summary>
	public Animator charAnimator
	{ get => mAnimator; }
	
	/// <summary>
	/// Current character collider.
	/// </summary>
	public BoxCollider2D charCollider
	{ get => mBoxCollider; }
	
	/// <summary>
	/// Current character GameObject.
	/// </summary>
	public GameObject character
	{ get => mCharacter; }
    
    /// <summary>
    /// Search the children for the Model game object.
    /// </summary>
    /// <returns>The Model game object.</returns>
    [CanBeNull]
    GameObject FindCharacterGO()
    {
	    var modelGO = Util.Common.GetChildByName(gameObject, "Model");
	    if (modelGO != null) { return modelGO; }

	    modelGO = Util.Common.GetChildByScript<Rigidbody2D>(gameObject);
	    
	    return modelGO;
    }

    /// <summary>
    /// Called when script is initialized.
    /// </summary>
    private void Awake()
    { mCharacter = FindCharacterGO(); }

    /// <summary>
	/// Called before the first frame update.
	/// </summary>
	void Start()
	{
		Debug.Assert(characters.Count > 0, "At least one character prefab must be specified!");
		
        mInput = GetComponent<InputManager>();
        mAnimator = null;
        mBoxCollider = null;
	}

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    {
	    SelectCharacter(mInput.selectedCharacter);
    }

    /// <summary>
    /// Select given character as current.
    /// </summary>
    /// <param name="selection">Character selection starting at 1.</param>
    /// <returns>Returns true if the character changed.</returns>
    bool SelectCharacter(int selection)
    {
	    selection = Math.Clamp(selection, 1, characters.Count);
	    if (selection == mCurrentCharacter)
	    { return false; }
	    
	    // Change over to the new character.
	    if (mCharacter != null)
	    { Destroy(mCharacter); }
	    mCharacter = Instantiate(characters[selection - 1], transform);
	    mCurrentCharacter = selection;
	    
	    // Update references.
	    mAnimator = mCharacter.GetComponent<Animator>();
	    mBoxCollider = mCharacter.GetComponent<BoxCollider2D>();
	    
	    // Notify siblings that we changed character.
	    BroadcastMessage("OnCharacterChange", this);

	    return true;
    }
}
