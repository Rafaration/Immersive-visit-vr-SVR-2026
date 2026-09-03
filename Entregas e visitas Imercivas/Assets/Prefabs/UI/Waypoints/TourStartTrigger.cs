using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TourStartTrigger : MonoBehaviour
{
    public GuidedTourManager tourManager;

    void Start()
    {
        var interactable = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelect);
        }
        else
        {
            Debug.LogWarning("No XRBaseInteractable found on " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Evita que o chao ou o proprio corpo inicie a visita. Aceita apenas se o nome tiver Hand ou Controller.
        if (other.name.Contains("Hand") || other.name.Contains("Controller") || other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>() != null)
        {
            if (tourManager != null) 
            {
                tourManager.StartTour(gameObject);
            }
        }
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        if (tourManager != null)
        {
            tourManager.StartTour(gameObject);
        }
    }
}
