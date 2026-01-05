using UnityEngine;
public class Crowd: MonoBehaviour
{
    private IdlePerson[] people;


    private void Awake()
    {
        people = GetComponentsInChildren<IdlePerson>();
    }


    public void Cheer()
    {
        foreach (IdlePerson person in people)
        {
            person.Jump();
        }
    }
}