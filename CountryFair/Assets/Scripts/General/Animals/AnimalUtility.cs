using UnityEngine;

public class AnimalUtility: MonoBehaviour
{
    public struct Stats
    {
        public float hunger;
        public float boredom;
        public float fatigue;
    }

    public Stats stats;


    private void Awake()
    {
       InitializeStats();
    }


    private void InitializeStats()
    {
        stats = new Stats()
        {
            hunger = Random.value,
            boredom = Random.value,
            fatigue = Random.value
        };
    }


    public string DecideNextAction()
    {
        float hunger = stats.hunger;

        float boredom = stats.boredom;

        float fatigue = stats.fatigue;

        if (hunger >= boredom && hunger >= fatigue)
        {
            return "GoEat";
        }

        if (boredom >= fatigue)
        {
            return "GoWalk";
        }

        return "GoIdle";
    }
}