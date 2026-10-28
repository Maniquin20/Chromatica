
using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform jogador;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float suavidade = 0.15f;

    private Vector3 velocidade = Vector3.zero;

    void LateUpdate()
    {
        if (jogador == null) return;

        Vector3 destino = jogador.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            destino,
            ref velocidade,
            suavidade
        );
    }
}
