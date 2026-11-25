using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ControlEnemigoLateral : MonoBehaviour
{
    [Header ("Movimiento")]
    public float velocidadLateral = 2f;
    private Vector2 posicionIzquierda;
    public Vector2 posicionDerecha;
    private SpriteRenderer renderizador;
    private Animator animador;
    private bool movimientoHaciaLateral = true;
    [SerializeField] private float toleranciaLateral = 0.5f;


    private Rigidbody2D fisicaLateral;

    private void Awake()
    {
        fisicaLateral = GetComponent<Rigidbody2D>();

        renderizador = GetComponent<SpriteRenderer>();

        animador = GetComponent<Animator>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        posicionIzquierda = transform.position;
        Debug.Log(posicionDerecha);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (velocidadLateral <= 0)
            return;

        Vector2 posicionObjetivo = movimientoHaciaLateral ? posicionDerecha : posicionIzquierda;
        //Debug.LogError("movimiento hacia lateral" + movimientoHaciaLateral);
        //Debug.Log(Vector2.Distance(fisicaLateral.position, posicionObjetivo));


        if (Vector2.Distance(fisicaLateral.position,posicionObjetivo)<= toleranciaLateral)
        {
            //Debug.LogError("No llega al if");
            if (posicionObjetivo == posicionDerecha)
            {
                //Debug.LogError("Movimiento hacia la izquierda, movimiento hacia lateral" + movimientoHaciaLateral);
                posicionObjetivo = posicionIzquierda;
                if (renderizador != null) renderizador.flipX = false;
            }
            else
            {
                //Debug.LogError("Movimiento hacia la derecha, movimiento hacia lateral" + movimientoHaciaLateral);
                posicionObjetivo = posicionDerecha;
                if (renderizador != null) renderizador.flipX = true;
            }

            movimientoHaciaLateral = !movimientoHaciaLateral;
            //Debug.LogError("movimiento hacia lateral" + movimientoHaciaLateral);
            posicionObjetivo = movimientoHaciaLateral ? posicionIzquierda : posicionDerecha;

        }

        fisicaLateral.MovePosition(Vector2.MoveTowards(fisicaLateral.position, posicionObjetivo, velocidadLateral));


        OrientarSprite();

        AnimarJugador();
    }

    private void OrientarSprite()
    {
        if (renderizador == null)
            return;
        if (fisicaLateral.linearVelocity.x < 0)
        {
            renderizador.flipX = true;
        }
        else if (fisicaLateral.linearVelocity.x > 0)
        {
            renderizador.flipX = false;
        }
    }

    private void AnimarJugador()
    {
        if (animador == null)
            return;
        else
            animador.Play("EnemigoAndando");
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
