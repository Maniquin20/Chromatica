using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move_CHR : MonoBehaviour
{

    private Rigidbody2D rb;
    private float moveX;
    private bool IsJump;

    public float JumpForce = 3f;
    public float speed = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsJump = false;

        }

    // Update is called once per frame
    void Update()
    {
         moveX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocityX = moveX*speed;

        if(Input.GetButtonDown("Jump") && IsJump == false)
        {
            rb.linearVelocityY = JumpForce;
            IsJump = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D collission)
    {
        if (collission.gameObject.CompareTag("Chao"))
        {
            IsJump = false;
        }
    }
}
