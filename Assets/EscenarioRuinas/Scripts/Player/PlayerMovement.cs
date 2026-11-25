using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")] //mover el personaje de izquierda a derecha
    [SerializeField, Min(0f)]
    [Tooltip("Velocidad horizontal en unidades por segundo (1 unidad = 16 píxeles).")]
    private float _movementSpeed = 7f;

    [SerializeField, //saltar
    [Tooltip("Impulso hacia arriba al pulsar Espacio.")]
    private float _jumpForce = 17f;

    [Header("Caídas")] //reiniciar si se cae
    [SerializeField]
    [Tooltip("Si el personaje baja de esta altura, vuelve al punto de inicio.")]
    private float _fallLimitY = -3f;

    //movimiento de la camara
    [Header("Cámara")]
    [SerializeField]
    [Tooltip("X mínima de la cámara: media pantalla (640 px / 2 / 16 = 20).")]
    private float _cameraMinX = 20f;

    [SerializeField]
    [Tooltip("X máxima de la cámara: ancho del escenario menos media pantalla (120 - 20 = 100).")]
    private float _cameraMaxX = 100f;

    [Header("Final")] //mensaje de llegar a la meta
    [SerializeField]
    [Tooltip("Mensaje que se enseña al llegar a la meta (empieza desactivado).")]
    private GameObject _finishMessage;

    //referencias a componentes y variables internas
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _startPosition;
    private float _horizontalInput;
    private bool _isJumpRequested;
    private bool _isGrounded;
    private bool _hasFinished;

    private void Start()
    {
        //guarda los componentes y la posición inicial para reaparecer
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
    }

    private void Update()
    {
        //parar al personaje cuando llega a la meta
        if (_hasFinished)
        {
            _horizontalInput = 0f;
            return;
        }

        _horizontalInput = Input.GetAxisRaw("Horizontal");

        //saltar al espacio
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _isJumpRequested = true;
        }

        FlipSprite();

        if (transform.position.y < _fallLimitY)
        {
            Respawn();
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_horizontalInput * _movementSpeed, _rb.linearVelocity.y);

        if (_isJumpRequested)
        {
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            _isJumpRequested = false;
            _isGrounded = false;
        }
    }

    private void LateUpdate()
    {
        //la camara se mueve en LateUpdate despues de mover al personaje para que no vaya con un frame de retraso
        Transform cameraTransform = Camera.main.transform;
        //limita la X de la camara para que no se salga de los bordes del escenario
        float cameraX = Mathf.Clamp(transform.position.x, _cameraMinX, _cameraMaxX);
        cameraTransform.position = new Vector3(cameraX, cameraTransform.position.y, cameraTransform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            //muestra el mensaje final bloquea el control y detiene al personaje.
            Debug.Log("Has acabado!");
            _finishMessage.SetActive(true);
            _hasFinished = true;
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGround(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGround(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _isGrounded = false;
    }

    private void CheckGround(Collision2D collision)
    {
        //colision del suelo
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            _isGrounded = true;
        }
    }

    private void FlipSprite()
    {
        //cambia la direccion del sprite segun el movimiento
        if (_horizontalInput > 0f)
        {
            _spriteRenderer.flipX = false;
        }
        else if (_horizontalInput < 0f)
        {
            _spriteRenderer.flipX = true;
        }
    }

    private void Respawn()
    {
        transform.position = _startPosition;
        _rb.linearVelocity = Vector2.zero;
    }
}
