
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public GameObject tilemapCinza;
    public GameObject tilemapColorido;

    public void AtivarCor()
    {
        tilemapCinza.SetActive(false);
        tilemapColorido.SetActive(true);
    }
}
