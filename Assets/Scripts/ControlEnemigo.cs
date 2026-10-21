using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ControlEnemigo : MonoBehaviour
{
    [Header ("Movimiento")]
    public float velocidad = 2f;
    private Vector2 posicionInicial;
    public Vector2 posicionFinal;
    private bool movimientoHaciaArriba = true;
    [SerializeField] private float tolerancia = 0.01f;


    private Rigidbody2D fisica;

    private void Awake()
    {
        fisica = GetComponent<Rigidbody2D>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        posicionInicial = transform.position;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (velocidad <= 0)
            return;

        //if (movimientoHaciaArriba)
        //{
        //    posicionObjetivo = posicionFinal;
        //}
        //else
        //{
        //    posicionObjetivo = posicionInicial;
        //}

        Vector2 posicionObjetivo = movimientoHaciaArriba ? posicionFinal : posicionInicial;

        if (Vector2.Distance(fisica.position,posicionObjetivo)<=tolerancia)
        {
            //if (movimientoHaciaArriba)
            //{
            //    movimientoHaciaArriba = false;
            //    posicionObjetivo = posicionFinal;
            //}
            //else
            //{
            //    movimientoHaciaArriba = true;
            //    posicionObjetivo = posicionFinal;
            //}
            movimientoHaciaArriba = !movimientoHaciaArriba;
            posicionObjetivo = movimientoHaciaArriba ? posicionFinal : posicionInicial;
        }

        fisica.MovePosition(Vector2.MoveTowards(fisica.position, posicionObjetivo, velocidad));

    }
}
