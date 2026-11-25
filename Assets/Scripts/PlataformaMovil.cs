using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlataformaMovil : MonoBehaviour
{

    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;
    [SerializeField] private float tolerancia = 0.01f;
    [SerializeField, Min(0f)] private float velocidad = 2f;

    private Rigidbody2D fisica;
    private bool haciaB = true;
    private float haciaA = 0.01f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        fisica = GetComponent<Rigidbody2D>();
    }

    public Vector2 Velocidad { get; private set; }
    void Start()
    {
        if (puntoA==null || puntoB == null)
        {
            Debug.LogError("Asigna el puntoA y el puntoB a la plataforma móvil", this);

            return;
        }

        fisica.position = (Vector2)puntoA.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 destino = haciaB ? (Vector2)puntoB.position : (Vector2)puntoA.position;

        if (Vector2.Distance(fisica.position, destino) <= tolerancia)
        {
            haciaB = !haciaB;
            destino = haciaB ? (Vector2)puntoB.position : (Vector2)puntoA.position;
        }

        Vector2 siguiente = Vector2.MoveTowards(fisica.position, destino, velocidad * Time.fixedDeltaTime);

        fisica.MovePosition(siguiente);
    }

    private void OnDrawGizmos()
    {
        if (puntoA == null || puntoB == null)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(puntoA.position, puntoB.position);
    }
}
