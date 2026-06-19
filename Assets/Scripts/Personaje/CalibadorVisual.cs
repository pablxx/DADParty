using UnityEngine;

public class CalibradorVisual : MonoBehaviour
{
    [Header("Ajuste de Posición Local")]
    [SerializeField] private Vector3 offsetLocal = Vector3.zero;

    [Header("Ajuste de Rotación Local")]
    [SerializeField] private Vector3 rotacionLocal = Vector3.zero;

    void Start()
    {
        AplicarCalibracion();
    }

    private void OnValidate()
    {
        AplicarCalibracion();
    }

    private void AplicarCalibracion()
    {

        transform.localPosition = offsetLocal;
        transform.localRotation = Quaternion.Euler(rotacionLocal);
    }
}