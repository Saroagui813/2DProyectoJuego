using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ControlEnemigo : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2f;                 
    [SerializeField] private Vector2 desplazamiento = new Vector2(0f, 3f); 
    [SerializeField] private float tolerancia = 0.01f;

    private Vector2 posicionInicial;
    private Vector2 posicionFinal;
    private bool haciaFinal = true;
    private Rigidbody2D fisica;

    private void Awake()
    {
        fisica = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        posicionInicial = fisica.position;
        posicionFinal = posicionInicial + desplazamiento;
    }

    private void FixedUpdate()
    {
        if (velocidad <= 0f)
            return;

        Vector2 objetivo = haciaFinal ? posicionFinal : posicionInicial;

        if (Vector2.Distance(fisica.position, objetivo) <= tolerancia)
        {
            haciaFinal = !haciaFinal;
            objetivo = haciaFinal ? posicionFinal : posicionInicial;
        }

        fisica.MovePosition(
            Vector2.MoveTowards(fisica.position, objetivo, velocidad)
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (collision.gameObject.TryGetComponent(out MovimientoJugador jugador))
        {
            jugador.QuitarVida();
        }
        else
        {
            Debug.LogError("El objeto player necesita MovimientoJugador en la raiz");
        }
    }
}