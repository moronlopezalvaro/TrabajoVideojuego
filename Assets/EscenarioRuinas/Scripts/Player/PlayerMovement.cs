using UnityEngine;

// Script del personaje: lo mueve a izquierda y derecha, le deja saltar cuando está en el suelo,
// hace que la cámara lo siga y, al llegar a la meta, enseña el mensaje final y lo para.
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField, Min(0f)]
    [Tooltip("Velocidad horizontal en unidades por segundo (1 unidad = 16 píxeles).")]
    private float _movementSpeed = 7f;

    [SerializeField, Min(0f)]
    [Tooltip("Impulso hacia arriba al pulsar Espacio.")]
    private float _jumpForce = 17f;

    [Header("Caídas")]
    [SerializeField]
    [Tooltip("Si el personaje baja de esta altura, vuelve al punto de inicio.")]
    private float _fallLimitY = -3f;

    [Header("Cámara")]
    [SerializeField]
    [Tooltip("X mínima de la cámara: media pantalla (640 px / 2 / 16 = 20).")]
    private float _cameraMinX = 20f;

    [SerializeField]
    [Tooltip("X máxima de la cámara: ancho del escenario menos media pantalla (120 - 20 = 100).")]
    private float _cameraMaxX = 100f;

    [Header("Final")]
    [SerializeField]
    [Tooltip("Mensaje que se enseña al llegar a la meta (empieza desactivado).")]
    private GameObject _finishMessage;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _startPosition;
    private float _horizontalInput;
    private bool _isJumpRequested;
    private bool _isGrounded;
    private bool _hasFinished;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
    }

    private void Update()
    {
        // Si ya hemos llegado a la meta, no leemos el teclado: el personaje se queda quieto.
        if (_hasFinished)
        {
            _horizontalInput = 0f;
            return;
        }

        // El input se lee en Update.
        _horizontalInput = Input.GetAxisRaw("Horizontal");

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
        // Las físicas se aplican en FixedUpdate. Solo cambiamos la X: la Y la controla la gravedad.
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
        // La cámara se mueve en LateUpdate, después de mover al personaje, para que no vaya un frame tarde.
        Transform cameraTransform = Camera.main.transform;
        float cameraX = Mathf.Clamp(transform.position.x, _cameraMinX, _cameraMaxX);
        cameraTransform.position = new Vector3(cameraX, cameraTransform.position.y, cameraTransform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // La meta es un trigger con el tag Finish al final del escenario.
        if (other.CompareTag("Finish"))
        {
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
        // Stay vuelve a comprobarlo mientras seguimos tocando el suelo,
        // por si Exit lo desactivó al separarnos de una pared.
        CheckGround(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _isGrounded = false;
    }

    private void CheckGround(Collision2D collision)
    {
        // Si el contacto empuja hacia arriba, estamos encima de algo (suelo o plataforma),
        // no chocando de lado contra una pared.
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            _isGrounded = true;
        }
    }

    private void FlipSprite()
    {
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
