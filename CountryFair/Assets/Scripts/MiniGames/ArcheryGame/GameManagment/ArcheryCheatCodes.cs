using UnityEngine;
using System.Linq;

public class  ArcheryCheatCodes : CheatCodes
{   
    private string _balloonColorToScore = "";

    private Transform _arrowTransform = null;

    private Arrow _arrowComponent = null;

    /// <summary>
    /// Initializes the maximum cheat code length based on defined cheat codes.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Start()
    {   
        base.Start();

        GameObject arrow = GameObject.FindGameObjectWithTag("Arrow");

        if (arrow == null)
        {
            Debug.LogError("Arrow GameObject not found.");
            return;
        }

        _arrowTransform = arrow.transform;

        if (!arrow.TryGetComponent<Arrow>(out var arrowComponent))
        {
            Debug.LogError("Arrow component not found on Arrow GameObject.");

            return;
        }

        _arrowComponent = arrowComponent;

        _balloonColorToScore = PlayerPrefs.GetString("BalloonColorToScore", "red").ToLower();
    }


    protected override void ActivateCheat(string cheatCode)
    {
        base.ActivateCheat(cheatCode);

        switch (cheatCode)
        {
            case "miss":
                _arrowComponent.Launch(0f);
                return;
            case "score":
                // Random value of launch force, to ensure the arrow is no kinematic
                 _arrowComponent.Launch(5f);
                _arrowTransform.position = GetCorrectBalloonPosition() + new Vector3(0, 0f, 0);
                return;
            default:
                Debug.LogWarning("Unhandled cheat code: " + cheatCode);
                return;
        }
    }

    private Vector3 GetCorrectBalloonPosition()
    {
        Transform targetBalloonTransform = GameObject.FindGameObjectsWithTag("Balloon")
            .FirstOrDefault(balloon => balloon.GetComponent<BalloonArcheryGame>().GetBalloonColorName().ToLower() == _balloonColorToScore)
            .transform;
            
        if (targetBalloonTransform == null){

            Debug.LogError("Correct balloon color not found!");
            return Vector3.zero;
        }

        return targetBalloonTransform.position;
    }
}