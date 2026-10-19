using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Loads the CountryFair hub scene, transitioning the player out of a mini-game.
/// </summary>
public class ReturnToFair : MonoBehaviour
{
    /// <summary>
    /// Immediately loads the CountryFair hub scene, discarding the current mini-game scene.
    /// </summary>
    /// <remarks>Invoked via Inspector by the return buttons in the mini-game scenes, and by the <c>return</c> cheat.</remarks>
    public void Return()
    {
        SceneManager.LoadScene(GameManager.HubScene);
        ConnectToWebApp.Instance.UpdateScene(GameManager.HubScene.ToLower());
    }
}
