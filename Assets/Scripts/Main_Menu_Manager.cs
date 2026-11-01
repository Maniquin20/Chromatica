using UnityEngine;

public class Main_Menu_Manager : MonoBehaviour
{
    [SerializeField] private string NomeLVL;

    public void Jogar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(NomeLVL);
    }

}
