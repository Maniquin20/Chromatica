
using UnityEngine;
using System.Collections;

public class BossArenaController : MonoBehaviour
{
    private Move_CHR player;
    private bool started;

    [Header("Boss")]
    public GameObject bossObject;
    public BossController bossController;

    [Header("Dialogo")]
    [TextArea(2, 5)]
    public string[] introLines;

    [Header("Camera")]
    public UnityEngine.Camera arenaCamera;
    public MonoBehaviour cameraFollow;
    public float cameraMoveSpeed = 3f;
    public Vector2 cameraOffset;

    [Header("Barreiras da arena")]
    public GameObject[] arenaBarriers;

    void Start()
    {

        // Esconde as barreiras até a apresentação terminar.
        foreach (GameObject barrier in arenaBarriers)
        {
            if (barrier != null)
                barrier.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (started)
            return;

        Move_CHR foundPlayer =
            other.GetComponentInParent<Move_CHR>();

        if (foundPlayer == null)
            return;

        started = true;
        player = foundPlayer;

        player.SetControlLocked(true);

        if (DialogueManager.Instance != null &&
            introLines != null &&
            introLines.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(
                introLines,
                StartCameraSequence
            );
        }
        else
        {
            StartCameraSequence();
        }
    }

    private void StartCameraSequence()
    {
        StartCoroutine(MoveCameraAndStartFight());
    }

    private IEnumerator MoveCameraAndStartFight()
    {
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        if (arenaCamera != null && bossObject != null)
        {
            // Calcula o ponto médio entre jogador e boss.
            Vector3 midpoint =
                (player.transform.position + bossObject.transform.position) / 2f;

            Vector3 target = new Vector3(
                midpoint.x + cameraOffset.x,
                midpoint.y + cameraOffset.y,
                arenaCamera.transform.position.z
            );

            while (Vector3.Distance(
                arenaCamera.transform.position,
                target
            ) > 0.02f)
            {
                arenaCamera.transform.position =
                    Vector3.MoveTowards(
                        arenaCamera.transform.position,
                        target,
                        cameraMoveSpeed * Time.deltaTime
                    );

                yield return null;
            }

            arenaCamera.transform.position = target;
        }

        // Mostra as paredes e ativa suas colisões.
        foreach (GameObject barrier in arenaBarriers)
        {
            if (barrier != null)
                barrier.SetActive(true);
        }

        bossController.StartFight();

        player.SetControlLocked(false);
    }

    public void OnBossDefeated()
    {
        Debug.Log("Arena concluída!");

        foreach (GameObject barrier in arenaBarriers)
        {
            if (barrier != null)
                barrier.SetActive(false);
        }

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        if (player != null)
            player.SetControlLocked(false);
    }
}
