using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public static Movement instance = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private Rigidbody2D rb;
    public float speed = 10.0f;
    public float jumpForce = 50.0f;

    public LayerMask ground;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();        
    }

    void Update()
    {
        if(!StateManager.instance.IsConnecting)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            Vector2 dir = new Vector2 (x, y);

            walk(dir);

            if (Input.GetKeyDown(KeyCode.Space))
                jump();
        }
    }

    void walk(Vector2 dir)
    {
        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
    }

    void jump()
    {
        if (checkGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            rb.velocity += Vector2.up * jumpForce;
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
        }
    }

    public void resetMovement()
    {
        rb.velocity = Vector2.zero;
    }

    bool checkGrounded()
    {
        foreach(Vector2 pos in Grid.instance.groundChecks)
        {
            if (Physics2D.OverlapCircle(pos + new Vector2(transform.position.x, transform.position.y - 0.5f), 0.25f,ground))
                return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (Grid.instance != null && Grid.instance.groundChecks != null)
        {
            foreach (Vector2 cell in Grid.instance.groundChecks)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(new Vector3(cell.x, cell.y - 0.5f, 0.0f) + transform.position, 0.25f);
            }
        }
    }
}
