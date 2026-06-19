using UnityEngine;

public class OscilacionLiquida : MonoBehaviour
{
    [Header("Configuración de Ondas")]
    [SerializeField] private float tiempoDecaimiento = 2f;
    [SerializeField] private float frecuenciaOscilacion = 10f;
    [SerializeField] private float amortiguacionMasa = 5f;

    [Header("Sensibilidad de Fuerzas")]
    [SerializeField] private float sensibilidadMovimiento = 0.2f;
    [SerializeField] private float sensibilidadRotacion = 0.05f;
    [SerializeField] private float limiteFuerzaMax = 0.5f;


    private MeshRenderer miRenderer;
    private MaterialPropertyBlock miBloquePropiedades;

    private Vector3 posicionUltimoFrame;
    private Vector3 rotacionUltimoFrame;
    private float fuerzaX = 0f;
    private float fuerzaZ = 0f;
    private float cronometroTiempo = 0f;
    private float rotXCalculado = 0f;
    private float rotZCalculado = 0f;

    void Awake()
    {
        miRenderer = GetComponent<MeshRenderer>();
        miBloquePropiedades = new MaterialPropertyBlock();
    }

    void Start()
    {
        posicionUltimoFrame = transform.position;
        rotacionUltimoFrame = transform.localEulerAngles;
    }

    void Update()
    {
        if (miRenderer != null)
        {
            Vector3 posicionActual = transform.position;
            Vector3 velocidadMovimiento = (posicionActual - posicionUltimoFrame) / Time.deltaTime;
            Vector3 rotacionActual = transform.localEulerAngles;

            float deltaRotX = Mathf.DeltaAngle(rotacionUltimoFrame.x, rotacionActual.x);
            float deltaRotZ = Mathf.DeltaAngle(rotacionUltimoFrame.z, rotacionActual.z);
            Vector3 velocidadAngular = new Vector3(deltaRotX, 0f, deltaRotZ) / Time.deltaTime;

            float impactoX = (velocidadMovimiento.z * sensibilidadMovimiento) + (velocidadAngular.x * sensibilidadRotacion);
            float impactoZ = (velocidadMovimiento.x * sensibilidadMovimiento) + (velocidadAngular.z * sensibilidadRotacion);

            if (Mathf.Abs(impactoX) > 0.01f || Mathf.Abs(impactoZ) > 0.01f)
            {
                cronometroTiempo = 0f;
                fuerzaX = fuerzaX + Mathf.Clamp(impactoX, -limiteFuerzaMax, limiteFuerzaMax);
                fuerzaZ = fuerzaZ + Mathf.Clamp(impactoZ, -limiteFuerzaMax, limiteFuerzaMax);
            }

            cronometroTiempo = cronometroTiempo + Time.deltaTime;
            float factorDecaimiento = Mathf.Exp(-amortiguacionMasa * cronometroTiempo);

            if (cronometroTiempo >= tiempoDecaimiento)
            {
                factorDecaimiento = 0f;
                fuerzaX = 0f;
                fuerzaZ = 0f;
            }

            float ondaSinu = Mathf.Sin(frecuenciaOscilacion * cronometroTiempo);
            rotXCalculado = fuerzaX * ondaSinu * factorDecaimiento;
            rotZCalculado = fuerzaZ * ondaSinu * factorDecaimiento;

            // --- ADAPTACIÓN DE LA SOLUCIÓN 3 ---
            // 1. Extraemos las propiedades actuales que tiene el MeshRenderer de esta botella
            miRenderer.GetPropertyBlock(miBloquePropiedades);

            // 2. Le inyectamos los nuevos valores calculados de la oscilación a este bloque local
            miBloquePropiedades.SetFloat("_rotX", rotXCalculado);
            miBloquePropiedades.SetFloat("_rotZ", rotZCalculado);

            // 3. Le devolvemos el bloque modificado al MeshRenderer para que actualice solo sus datos
            miRenderer.SetPropertyBlock(miBloquePropiedades);

            posicionUltimoFrame = posicionActual;
            rotacionUltimoFrame = rotacionActual;
        }
    }

}