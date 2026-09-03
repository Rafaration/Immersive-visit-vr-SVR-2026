using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TourWaypoint
{
    public string nome;
    public Transform ponto;
    public float dwellTime = 5f;
    public List<GameObject> plataformas = new List<GameObject>();
}

public class GuidedTourManager : MonoBehaviour
{
    [Header("Referências")]
    public Transform xrRig;
    public List<TourWaypoint> waypoints = new List<TourWaypoint>();

    [Header("UI do HUD")]
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI timeRemainingText;
    public TextMeshProUGUI totalTimeText;

    private float totalTourTimeElapsed = 0f;
    private bool isTourRunning = false;

        private GameObject currentStartButton = null;
// Cache de plataformas únicas para desativar todas de uma vez
    private List<GameObject> todasPlataformas = new List<GameObject>();

    void Awake()
    {
        // Coleta todas as plataformas únicas e garante que comecem desativadas
        foreach (var wp in waypoints)
        {
            if (wp.plataformas != null)
            {
                foreach (var p in wp.plataformas)
                {
                    if (p != null && !todasPlataformas.Contains(p))
                    {
                        todasPlataformas.Add(p);
                        //p.SetActive(false);
                    }
                }
            }
        }
    }

    public void StartTour(GameObject startButton = null)
    {
        if (isTourRunning) return;
        if (waypoints.Count == 0) { Debug.LogWarning("Nenhum waypoint configurado."); return; }

        currentStartButton = startButton;
        if (currentStartButton != null) currentStartButton.SetActive(false);

        isTourRunning = true;
        totalTourTimeElapsed = 0f;
        StartCoroutine(TourRoutine());
    }

    private IEnumerator TourRoutine()
    {
        foreach (var waypoint in waypoints)
        {
            // Desativa todas as plataformas e ativa só a deste waypoint (índice 0)
            foreach (var p in todasPlataformas)
                if (p != null) p.SetActive(false);

            if (waypoint.plataformas != null && waypoint.plataformas.Count > 0 && waypoint.plataformas[0] != null)
            {
                waypoint.plataformas[0].SetActive(true);
                // Aguarda 2 frames de física antes de teleportar
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
            }

            // Teleporta e atualiza nome do local
            TeleportarDireto(waypoint.ponto);
            if (locationText != null) locationText.text = waypoint.nome;

            // Contador de permanência e total no mesmo loop (sincronizados pelo mesmo deltaTime)
            float timeRemaining = waypoint.dwellTime;
            while (timeRemaining > 0)
            {
                float dt = Time.deltaTime;
                timeRemaining -= dt;
                totalTourTimeElapsed += dt;

                if (timeRemainingText != null)
                    timeRemainingText.text = $"{Mathf.CeilToInt(Mathf.Max(0f, timeRemaining))}s";

                if (totalTimeText != null)
                {
                    int min = Mathf.FloorToInt(totalTourTimeElapsed / 60f);
                    int sec = Mathf.FloorToInt(totalTourTimeElapsed % 60f);
                    totalTimeText.text = $"Total: {min:00}:{sec:00}";
                }

                yield return null;
            }
        }

        // Fim do tour
        if (locationText != null) locationText.text = "Visita Concluída";
        if (timeRemainingText != null) timeRemainingText.text = "0s";
        if (totalTimeText != null)
        {
            int min = Mathf.FloorToInt(totalTourTimeElapsed / 60f);
            int sec = Mathf.FloorToInt(totalTourTimeElapsed % 60f);
            totalTimeText.text = $"Total: {min:00}:{sec:00}";
        }

        // Aguarda 5 segundos antes de reabilitar o botão
        yield return new WaitForSeconds(5f);

        isTourRunning = false;
        if (locationText != null) locationText.text = "Precione o botão para iniciar";
        
        if (currentStartButton != null)
        {
            currentStartButton.SetActive(true);
            currentStartButton = null;
        }
    }

    private void TeleportarDireto(Transform destino)
    {
        if (xrRig == null || Camera.main == null) return;

        var cc = xrRig.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Transform cameraTransform = Camera.main.transform;

        // Calcula offset entre cabeça e rig para manter a posição relativa
        Vector3 offset = xrRig.position - cameraTransform.position;
        xrRig.position = destino.position + offset;

        // Corrige rotação baseado na cabeça: gira o rig no eixo Y pela diferença de yaw
        float yawDiff = Mathf.DeltaAngle(cameraTransform.eulerAngles.y, destino.eulerAngles.y);
        xrRig.Rotate(0f, yawDiff, 0f);

        Physics.SyncTransforms();
        if (cc != null) cc.enabled = true;
    }
}
