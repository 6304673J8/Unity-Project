﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class RangedEnemyAI : MonoBehaviour
{

    private float dieDelay = 1f;
    public float hp;
    public float damage;
    public float speed;
    public float stoppingDistance;
    public float retreatDistance;
    public float shotDistance = 15;
    private bool stunned;
    SpriteRenderer sprite;
    private Animator animator;
    public Transform player;
    private Rigidbody2D rb;
    private float timeBtwShots;
    public float startTimeBtwShots;

    ParticleSystem damagepart;

    public GameObject fireBall;

    public GameObject key;

    private bool facingLeft;


    private float dying;

    private bool dyingb;

    private int range = 6;

    public bool canDrop;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        damagepart = GetComponent<ParticleSystem>();

    }


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Susana").transform;

        timeBtwShots = startTimeBtwShots;

        rb = this.GetComponent<Rigidbody2D>();

        dyingb = false;
        facingLeft = true;
    }


    void Update()
    {
        if(hp <= 0)
        {
            kill();
        }

        
        if(Vector2.Distance(transform.position,player.position) > stoppingDistance && Vector2.Distance(transform.position, player.position) < range)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        }


        else if(Vector2.Distance(transform.position, player.position) < stoppingDistance && Vector2.Distance(transform.position, player.position) > retreatDistance)
        {
            transform.position = this.transform.position;
        }
        

        else if(Vector2.Distance(transform.position,player.position) < retreatDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, -speed * Time.deltaTime);

        }


        if(timeBtwShots <= 0)
        {
            if(Vector2.Distance(transform.position, player.position) < shotDistance)
            {
                FindObjectOfType<SoundManager>().Play("FireballShot");
                Instantiate(fireBall, transform.position, Quaternion.identity);
                timeBtwShots = startTimeBtwShots;
            }
            
        }

        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        float separation = Vector3.Distance(this.transform.position, player.transform.position);

        if (separation <= 0.7)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (separation > 0.7)
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Earthquake"))
        {

            getStunned();
        }

        if(collision.CompareTag("Lunge"))
        {
            damagepart.Play();
        }

    }

    private void getStunned()
    {
        if (!dyingb)
        {
            stunned = true;
            sprite.color = new Color(0, 0, 1, 1);
            hp -= 20;
            return;
        }


    }

    private void unStun()
    {
        stunned = false;
        sprite.color = new Color(1, 1, 1, 1);
        return;
    }

    void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    void checkPosition()
    {
        if (facingLeft && player.position.x > transform.position.x)
        {
            Flip();
        }

        if (!facingLeft && player.position.x < transform.position.x)
        {
            Flip();
        }
    }

    private void kill()
    {
        dyingb = true;
        sprite.color = new Color(1, 1, 1, 1);
        animator.ResetTrigger("isWalking");
        animator.SetTrigger("isDying");
        dying += Time.deltaTime;
        if (dying >= dieDelay)
        {
            Destroy(gameObject);
            if (canDrop)
            {
                GameObject k = Instantiate(key, transform.position, transform.rotation);
            }
        }
    }
}
