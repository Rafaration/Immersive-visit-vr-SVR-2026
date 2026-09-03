using UnityEngine;

public class SetActive : MonoBehaviour
{
    public GameObject objeto;
    public bool StartActive = true;
    bool isActive;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       isActive = StartActive;
       objeto.SetActive(isActive);
    }

    public void Ativacao()
    {
        if (isActive)
        {
            objeto.SetActive(false);
            isActive = false;
        }
        else
        {
            objeto.SetActive(true);
            isActive = true;
        }
    }
}
