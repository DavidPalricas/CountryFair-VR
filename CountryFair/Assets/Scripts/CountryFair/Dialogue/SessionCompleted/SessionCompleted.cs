using UnityEngine;
using TMPro; 

public class SessionCompleted : UIDialog
{    
    [Header("Zeca References")]
    [SerializeField]
     private GameObject Zeca;

     [SerializeField]
     private TextMeshProUGUI characterNameText;


     private SessionCompletedData _sessionCompletedData;

    protected override void Awake()
    {    
        if ( !SessionWasCompleted()){
            enabled = false;
            return;
        }
     
        base.Awake();

        if (_data is not SessionCompletedData sessionCompletedData)
        {
            Debug.LogError("Error Converting data to SessionCompletedData.");

            return;
        }

        _sessionCompletedData = sessionCompletedData;
         
        SetCongratsDialogue();
    }

    private bool SessionWasCompleted(){
        GameManager gameManager = GameManager.GetInstance();

        if (!gameManager.IntroCompleted)
        {
            return false;
        }

        return gameManager.FrisbeeSessionCompleted || gameManager.ArcherySessionCompleted;
    }

    protected override System.Type GetJSONDataType()
    {
        return typeof(SessionCompletedData);
    }

    protected override void SetJSONFileName()
    {
        GameManager gameManager = GameManager.GetInstance();

        if (gameManager.FrisbeeSessionCompleted)
        {
            _jsonFileName = "frisbee_session_completed.json";

            gameManager.FrisbeeSessionCompleted = false;

            return;
        }

        if (gameManager.ArcherySessionCompleted)
        {
            _jsonFileName = "archery_session_completed.json";

            gameManager.ArcherySessionCompleted = false;

            return;
        }

       Destroy(gameObject);
    }


    private void SetCongratsDialogue()
    {
        characterNameText.text= "Zeca";

        dialogueBoxText.text = _sessionCompletedData.Congrats[Utils.RandomValueInRange(0, _sessionCompletedData.Congrats.Count)];
    }


    public override void NextStep()
    {    if (enabled)
        {
            Destroy(transform.parent.gameObject);
        }
         
    }
}