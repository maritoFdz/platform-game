using UnityEngine;

public class TesterNarrador : MonoBehaviour
{
    [ContextMenu("Pruebita1")]
    public void Prueba1()
    {
        Debug.Log(DialogueContainer.instance.GetDialogue("prueba_01"));
    }

    [ContextMenu("Pruebita2")]
    public void Prueba2()
    {
        Debug.Log(DialogueContainer.instance.GetDialogue("chivo_01"));
    }

    [ContextMenu("Pruebita3")]
    public void Prueba3()
    {
        Debug.Log(DialogueContainer.instance.GetDialogue("prueba_02"));
    }

    [ContextMenu("Pruebita4")]
    public void Prueba4()
    {
        Debug.Log(DialogueContainer.instance.GetDialogue("prueba_03"));
    }
}
