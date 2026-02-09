using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the activation and deactivation of emoji GameObjects based on specific types.
/// </summary>
public class ActivateEmoji : MonoBehaviour
{ 
   /// <summary>
   /// A dictionary to store emoji GameObjects, keyed by their lowercase names.
   /// </summary>
   private readonly Dictionary<string, GameObject> _emojis = new ();

   /// <summary>
   /// The currently active emoji GameObject.
   /// </summary>
   private GameObject currentEmojiActive = null;

   /// <summary>
   /// Defines the various types of emojis available.
   /// </summary>
   public enum EmojiType
   {
       /// <summary>
       /// Represents a sad expression.
       /// </summary>
       SAD,

       /// <summary>
       /// Represents a happy expression.
       /// </summary>
       HAPPY,

       /// <summary>
       /// Represents an angry expression.
       /// </summary>
       ANGRY,

       /// <summary>
       /// Represents a disgusted expression.
       /// </summary>
       DISGUST,

       /// <summary>
       /// Represents a surprised expression.
       /// </summary>
       SURPRISE,

       /// <summary>
       /// Represents a fearful expression.
       /// </summary>
       FEAR,

       /// <summary>
       /// Represents a neutral expression.
       /// </summary>
       NEUTRAL
   }

    /// <summary>
    /// Unity Awake method. Initializes the emoji dictionary.
    /// </summary>
    private void Awake()
    {
        SetEmojis();
    }

    /// <summary>
    /// Finds all child GameObjects, adds them to the dictionary, and sets the 'neutral' emoji as active by default.
    /// Logs an error if the 'neutral' emoji is not found.
    /// </summary>
    private void SetEmojis()
    {
        GameObject[] emojis =  Utils.GetChildren(transform);

        foreach (GameObject emoji in emojis)
        {   
            string emojiName = emoji.name.ToLower();

            _emojis[emojiName] = emoji;

            if (emojiName == "neutral")
            {
                 currentEmojiActive = emoji;
            }
        }


        if (currentEmojiActive == null)
        {
            Debug.LogError("Neutral emoji(Default emoji) not found among children of Emoji GameObject.");
        }
    }

 

    /// <summary>
    /// Activates the emoji corresponding to the given type and deactivates the previously active one.
    /// </summary>
    /// <param name="type">The type of emoji to activate.</param>
    public void Activate(EmojiType type)
    {   
        string emojiName = type.ToString().ToLower();

        if (_emojis.TryGetValue(emojiName, out GameObject emoji))
        {   
            currentEmojiActive.SetActive(false);
            
            emoji.SetActive(true);
            currentEmojiActive = emoji;

            return;
        }

        Debug.LogError("Invalid emoji type: " + type);
    }
}
