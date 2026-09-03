using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Waypoint
{
    public string nome;
    public Transform ponto;
}

public class TeleportManager : MonoBehaviour
{
    [Header("Referências")]
    public Transform xrRig; // XR Rig (ex: XR Origin)
    public Transform uiCanvas; // Canvas do UI (Tela com as opções de botão)
    public List<Waypoint> waypoints = new List<Waypoint>();

    [Header("Configurações")]
    public float cooldown = 0.5f;

    private bool podeTeleportar = true;

    // ============================
    // FUNÇÃO CHAMADA PELOS BOTÕES
    // ============================
    public void Teleportar(int index)
    {
        if (!podeTeleportar) return;

        if (index < 0 || index >= waypoints.Count)
        {
            Debug.LogWarning("Índice de waypoint inválido");
            return;
        }

        StartCoroutine(TeleporteCooldown(waypoints[index]));
    }

    // ============================
    // TELEPORTE COM COOLDOWN
    // ============================
    private IEnumerator TeleporteCooldown(Waypoint destino)
    {
        podeTeleportar = false;

        TeleportarDireto(destino);

        yield return new WaitForSeconds(cooldown);

        podeTeleportar = true;
    }

    // ============================
    // TELEPORTE (VR)
    // ============================
    private void TeleportarDireto(Waypoint destino)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("Main Camera não encontrada.");
            return;
        }

        Transform cameraTransform = Camera.main.transform;

        // Salva rotação relativa do UI em relação à cabeça/câmera (para reaplicar depois)
        Quaternion uiRelativeRotation = Quaternion.Inverse(cameraTransform.rotation) * uiCanvas.rotation;

        // Calcula offset entre cabeça e rig para manter a posição relativa
        Vector3 offset = xrRig.position - cameraTransform.position;
        Vector3 offsetUI = uiCanvas.position - cameraTransform.position;

        // Move posição corretamente 
        xrRig.position = destino.ponto.position + offset;
        uiCanvas.position = destino.ponto.position + offsetUI;

        // Corrige rotação baseado na cabeça: gira o rig no eixo Y pela diferença de yaw (menor ângulo)
        float yawDiff = Mathf.DeltaAngle(cameraTransform.eulerAngles.y, destino.ponto.eulerAngles.y);
        xrRig.Rotate(0f, yawDiff, 0f);

        // Reaplica rotação do UI mantendo o mesmo offset relativo à câmera (agora que a câmera já foi rotacionada)
        uiCanvas.rotation = cameraTransform.rotation * uiRelativeRotation;
    }
}