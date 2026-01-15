using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CarnyWise))]
public class GameManager : MonoBehaviour
{   
    [SerializeField]
    protected int sessionScoreGoal = 3;

    private CarnyWise _carnyWise;


    protected virtual void Awake()
    {
       PlayerPrefs.SetInt("SessionGoal", sessionScoreGoal);
        _carnyWise = GetComponent<CarnyWise>();
    }

    public void SessionGoalReached()
    {   _carnyWise.SessionGoalReached();
        Invoke(nameof(ReturnToFair), 5f);
    }

    public virtual void IncreaseDifficulty()
    {
      Debug.Log("This method should be overridden in derived classes to implement difficulty increase logic in different mini-games.");  
    }


    public virtual void DecreaseDifficulty()
    {
      Debug.Log("This method should be overridden in derived classes to implement difficulty decrease logic in different mini-games.");  
    }

    private void ReturnToFair()
    {
        SceneManager.LoadScene("CountryFair");
    }
}
