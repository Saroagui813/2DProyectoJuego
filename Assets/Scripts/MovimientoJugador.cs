using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MovimientoJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 3f;

    [SerializeField] private InputActionReference accionMover;

    [Header("Salto")]
    public float impulso = 5f; //fuerza aplicada para saltar
    [SerializeField] private InputActionReference accionSaltar;
    [SerializeField] private LayerMask groundMask;       // Selecciona la capa "Suelo" aquí
    //[SerializeField] private float longRaycast;


    /*
     *        Tabla comparativa
            Característica	                    [SerializeField] private	                public
            ¿Visible en el Inspector?	                Sí	                                Sí
            ¿Modificable por otros scripts?	        No (Solo este script)   	            Sí (Cualquier script puede leer/escribir)
            Principio de Diseño	                Mantiene el encapsulamiento	                Rompe el encapsulamiento si solo se quería ver en el editor
            Seguridad del Código	        Alta (Evita errores accidentales externos)	    Baja (Cualquier clase puede corromper el dato) 
     
            [SerializeField] private int maxHealth = 100; // Configurable en el Inspector

            // Propiedad pública de solo lectura para el resto de scripts
            public int MaxHealth => maxHealth; 
     
     */

    private Rigidbody2D fisica;
    private SpriteRenderer renderizador;
    private CapsuleCollider2D capsule;
    private Animator animador;
    private Vector2 entradaMovimiento;
    private Boolean saltoPendiente;
    private bool isGrounded;
    private float longRaycast = 1.6f;
    private float umbralCarrera = 0.1f;
    private bool finEnProceso;

    [Header("Reaparición")]
    public Transform spawnPoint;

    [Header("Puntuación")]
    [SerializeField] private int puntuacion;

    [Header("Salud")]
    [SerializeField] private int vidas = 3;
    private int Vidas => vidas;
    private Color colorOriginal;
    [SerializeField, Min(0.01f)] private float segundosInvulnerable = 1f;
    private bool vulnerable;

    [Header("Tiempo")]
    [SerializeField, Min(1)] private float tiempoEmpleado;
    [SerializeField, Min(1)] private int tiempoLimiteNivel;
    private float tiempoInicio;
    private float tiempoReal;

    [Header("Canvas")]
    [SerializeField] private Canvas canvasInterfaz;
    private ControlHUD hud;
    private ControlDatosJuego controlDatosJuego;

    [Header("Audio")]
    [SerializeField] private AudioClip saltoSonido;
    [SerializeField] private AudioClip vidasSonido;
    private AudioSource efectoSonido;

    private void Awake()
    {
        fisica = GetComponent<Rigidbody2D>();

        capsule = GetComponent<CapsuleCollider2D>();

        renderizador = GetComponent<SpriteRenderer>();

        animador = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (accionMover == null || accionMover.action == null)
        {
            Debug.LogError(
                "Asigna Jugador/Mover en Accion Mover.", this);
            enabled = false;
            return;
        }

        accionMover.action.Enable();

        if (accionSaltar==null || accionSaltar.action == null)
        {
            Debug.LogError("Asigna Acción Jugador/Saltar en Acción Saltar.", this);
            enabled = false;
            return;
        }

        accionSaltar.action.Enable();
    }

    private void OnDisable()
    {
        if (accionMover != null && accionMover.action != null)
        {
            accionMover.action.Disable();
        }

        entradaMovimiento = Vector2.zero;

        if(accionSaltar != null && accionSaltar.action != null)
        {
            accionSaltar.action.Disable();  
        }

        if (fisica != null)
        {
            fisica.linearVelocity = new Vector2(0f, fisica.linearVelocity.y);
        }
    }

    private void Update()
    {
        entradaMovimiento = accionMover.action.ReadValue<Vector2>();

        ComprobarFinJuegoTiempo();

        SaltoJugador();

        AnimarJugador();
    }

    private void InicializaHud()
    {
        if (canvasInterfaz != null)
        {
            hud = canvasInterfaz.GetComponent<ControlHUD>();
        }
        else
            Debug.LogError("Asigna el objeto Canvas con el componente ControlHUD al atributo Canvas del Jugador.");

        if (hud != null)
        {
            hud.SetVidas(vidas);
            hud.SetTiempo(tiempoLimiteNivel);
            hud.SetPowerUps(GameObject.FindGameObjectsWithTag("PowerUp").Length);
        }
        else
            Debug.LogError("Asigna el objeto Canvas con el componente ControlHUD al atributo Canvas del Jugador.");
    }

    private void ComprobarFinJuegoTiempo()
    {
        tiempoReal = Time.time - tiempoInicio;
        tiempoEmpleado = Mathf.FloorToInt(tiempoReal);
        int segundosRestantes = Mathf.CeilToInt(Mathf.Max(0f, tiempoLimiteNivel - tiempoReal));
        hud.SetTiempo(segundosRestantes);
        hud.SetPowerUps(GameObject.FindGameObjectsWithTag("PowerUp").Length);

        if (tiempoEmpleado >= tiempoLimiteNivel)
        {
            FinJuego();
            return;
        }
    }

    private void SaltoJugador()
    {
        //Lanzamos un rayo hacia abajo desde el centro del personaje.
        // transform.position: Origen del rayo.
        // Vector2.down: Dirección del rayo hacia abajo.
        // 1.1f: Longitud del rayo (ajústala según el tamaño de tu Sprite/Collider).
        // groundMask: Capa del suelo para ignorar al propio jugador.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, longRaycast, groundMask);

        // Si el rayo impactó contra algo en esa capa, 'hit.collider' no será nulo
        isGrounded = hit.collider != null;

        // 2. DIBUJAR EL RAYO VISUAL EN EL EDITOR
        // Definimos el color: Verde si toca el suelo, Rojo si está en el aire
        Color colorDelRayo = isGrounded ? Color.green : Color.red;

        // Dibujamos la línea en la pestaña Scene
        // Parámetros: Origen, Vector de dirección (Dirección * Longitud), Color
        Debug.DrawRay(transform.position, Vector2.down * longRaycast, colorDelRayo);

        //Debug.Log("Está tocando el suelo (Raycast 2D): " + isGrounded);

        /*  La variable saltoPendiente actúa como una nota que queda escrita hasta que FixedUpdate la consume. 
         *  Si entre dos pasos físicos hay un fotograma que detecta la pulsación y otro que no la detecta, la nota sigue siendo verdadera.
         */
        if (accionSaltar.action.WasPressedThisFrame() && isGrounded)
        {
            saltoPendiente = true;
        }
    }
    private void AnimarJugador()
    {
        if (animador == null)
            return;

        if (!isGrounded)
            animador.Play("JugadorSaltando");
        else if (Mathf.Abs(fisica.linearVelocity.x) > umbralCarrera)
            animador.Play("JugadorCorriendo");
        else
            animador.Play("JugadorParado");
    }


    private void FixedUpdate()
    {
        MoverJugador();

        OrientarSprite();

    }

    private void Start()
    {
        if (renderizador != null)
            colorOriginal = renderizador.color;
        else
            Debug.LogError("El jugador no tiene asignado el srite renderer.");

        tiempoInicio = Time.time;

        controlDatosJuego = ControlDatosJuego.Instancia;
        if (controlDatosJuego == null)
        {
            Debug.LogError("Añade Datos Juego al nivel");
        }
        else
            controlDatosJuego.ReiniciarPartida();

        InicializaHud();
    }

    private void MoverJugador()
    {
        fisica.linearVelocity = new Vector2(
            entradaMovimiento.x * velocidad,
            fisica.linearVelocity.y
        );

        // Experimento temporal: todavía permite saltar en el aire.
        //Vector2.up es (0,1)
        if (saltoPendiente)
        {
            fisica.AddForce(Vector2.up * impulso, ForceMode2D.Impulse);

            //if (saltoSonido != null)
                //efectoSonido.PlayOneShot(saltoSonido);
        }

        //Como el salto ya ha sido atendido, deja de estar pendiente.
        saltoPendiente = false;
    }

    private void OrientarSprite()
    {
        if (renderizador == null)
            return;
        if (fisica.linearVelocity.x > 0)
        {
            renderizador.flipX = false;
        }
        else if (fisica.linearVelocity.x < 0)
        {
            renderizador.flipX = true;
        }
    }

    public void ReaparecerJugador()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("Asigna un spawnpoint al objeto jugador", this);
            enabled = false;
            return;
        }
        fisica.position = (Vector2)spawnPoint.position;
        fisica.linearVelocity = Vector2.zero;
    }

    public void IncrementarPuntos(int cantidad)
    {
        if (cantidad <= 0) return;

        puntuacion += cantidad;
        hud.SetPowerUps(GameObject.FindGameObjectsWithTag("PowerUp").Length);
    }

    public void QuitarVida()
    {
        if (!vulnerable)
        vulnerable = false;
        vidas--;

        if (vidasSonido != null)
            efectoSonido.PlayOneShot(vidasSonido);

        hud.SetVidas(vidas);

        if (renderizador!=null)
        {
            renderizador.color = Color.blue;
        }

        if (vidas <= 0)
        {
            FinJuego();
            return;
        }
        Invoke(nameof(hacerVulnerable), segundosInvulnerable);
    }

    private void hacerVulnerable()
    {
        vulnerable = true;
        if (renderizador != null)
            renderizador.color = colorOriginal;
    }

    public void FinJuego()
    {
        //Debug.Log("Juego finalizado.");
        //gameObject.SetActive(false);
        if (controlDatosJuego == null)
        {
            Debug.LogError("Falta el objeto Control Datos Juego para poder guardar el resultado de la partida", this);
            return;
        }

        controlDatosJuego.GuardarResultado(false, 0);
        SceneManager.LoadScene("FinEscena");
    }

    public void Victoria()
    {
        puntuacion = (int) (vidas * 100 + Mathf.Max(0, tiempoLimiteNivel - tiempoEmpleado));

        saltoPendiente = false;
        fisica.linearVelocity = Vector2.zero;

        controlDatosJuego.GuardarResultado(true, puntuacion);
        SceneManager.LoadScene("FinEscena");
    }
}
