﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class SusanaControlled : MonoBehaviour
{
    //testing 
    public bool saver;

    PlayerInputs controls;

    //Movement
    Vector2 move;
    Vector2 fakeMove;
    public bool canMove = false;
   public bool facingRight = true;

    //hp
    public HealthBar healthBar;

    bool isHurt;
    public int health;
    public int level;
    [SerializeField] private int maxHealth = 350;

    //inventory
    public int nPotions;
    public int nKeys;
    public int nBombs;

    [SerializeField] private float lungeDistance;

    //Animation
    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer sprite;
    ParticleSystem part;
    public ParticleSystem smokePartLeft;
    public ParticleSystem smokePartRight;
    public ParticleSystem smokePartUp;
    public ParticleSystem smokePartDown;
    public ParticleSystem damagepart;



    bool isMoving;

    bool hasLunged;

    //Collision Check
    [SerializeField]
    private Tilemap floorTilemap;
    [SerializeField]
    private Tilemap collisionTilemap;
    [SerializeField]
    private Tilemap damagingTilemap;
    [SerializeField]
    private Tilemap healingTilemap;
    [SerializeField] float speed;

    [Header("Abilities")]

    public GameObject earthquakePrefab;
    public GameObject lungePrefab;
    public GameObject potionPrefab;
    public GameObject ouch;
    public AbilitiesControlled abilities;
    public bool defending;
    public bool lunging;
    public bool quaking;
    
    //new animations
    private State state;
    public Animator animator;

    public bool isJumping;
    private int idleID;
    private int runID;
    private int lungeID;
    private int hurtID;

    private ParticleSystem ps;

    private enum State
    {
        IDLE,
        MOVING,
        ATTACK,
        HEALING
    }

    private void Awake()
    {
        //testing 
        saver = false;
        hasLunged = false;
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        part = GetComponent<ParticleSystem>();
        controls = new PlayerInputs();
        health = maxHealth;

        state = State.IDLE;

        //items 
        nPotions = 0;
        nKeys = 0;
        nBombs = 0;

        #region INPUCTACTIONS
        controls.Susana.Move.performed += ctx => SendMessage(ctx.ReadValue<Vector2>());

        controls.Susana.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Susana.Move.canceled += ctx => move = Vector2.zero;

        //controls.Susana.Action.performed += ctx => Interact();
        controls.Susana.Heal.performed += ctx => Heal();

        //Test Abilities
        controls.Susana.Lunge.performed += ctx => Lunge();
        controls.Susana.Shield.started += ctx => Shield();
        controls.Susana.Shield.canceled += ctx => Shield();
        controls.Susana.Earthquake.started += ctx => Earthquake();

    }
    private void OnEnable()
    {
        controls.Susana.Enable();
    }
    private void OnDisable()
    {
        controls.Susana.Disable();
    }

    void SendMessage(Vector2 coordinates)
    {
        //Debug.Log("Thumb-stick coordinates" + coordinates);
    }
    #endregion

    private void Start()
    {
        healthBar.SetMaxHealth(health);
        animator = GetComponent<Animator>();
        ps = GetComponent<ParticleSystem>();
        hurtID = Animator.StringToHash("Hurt");
        runID = Animator.StringToHash("Movement");
    }

    public void UpdateHealth(int mod)
    {
        //animator.SetTrigger("Hurt");
        isHurt = true;
        health -= mod;

        if (health > maxHealth) {
            health = maxHealth;
        }else if (health <= 0)
        {
            animator.SetTrigger("Hurt");
            health = 0;
            //healthBar.SetHealth(health);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        isHurt = false;
    }

    private void Update()
    {
        healthBar.SetHealth(health);
        //health += health;
        fakeMove = move;
        if (saver == true)
        {
            LoadPlayer();
        }
        if (health <= 0)
        {
            //animator.SetTrigger("Dead");
            health = 0;
            healthBar.SetHealth(health);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if(health <= 0)
        {
            LoadPlayer();
        }
    }
    void FixedUpdate()
    {
        Vector3Int gridPos = floorTilemap.WorldToCell(transform.position + (Vector3)move);
        //isMoving = false;
        bool hasMoved = false;

        if (move.x != 0 || move.y != 0)
        {
            if (facingRight == false && move.x > 0)
            {
                Flip();
                hasMoved = true;
            }
            else if (facingRight == true && move.x < 0)
            {
                Flip();
                hasMoved = true;
            }
            if (floorTilemap.HasTile(gridPos) || !collisionTilemap.HasTile(gridPos))
            {
                transform.position += (Vector3)move * speed * Time.fixedDeltaTime;
                hasMoved = true;
            }
        }
        else
        {
            hasMoved = false;
        }
        animator.SetBool(runID, hasMoved);
        if (hasLunged == true)
        {
            if(facingRight)
            {
                if(move.y == 0)
                {
                    smokePartRight.Play();
                }
                if(move.y > 0)
                {
                    smokePartUp.Play();
                }
                if(move.y < 0)
                {
                    smokePartDown.Play();
                }
            }

            if(!facingRight)
            {
                if (move.y == 0)
                {
                    smokePartLeft.Play();
                }
                if (move.y > 0)
                {
                    smokePartUp.Play();
                }
                if (move.y < 0)
                {
                    smokePartDown.Play();
                }
            }
            transform.position += (Vector3)move * speed * lungeDistance * Time.fixedDeltaTime;
            
            hasLunged = false;
        }
    }

    #region SKILLS
    public void Lunge()
    {
        move = controls.Susana.Move.ReadValue<Vector2>();
        if (CanLunge(move) && abilities.lungeCooldown == false)
        {
            LungeLogic();
            hasLunged = true;
            animator.SetTrigger("Lunge");
            //transform.position += (Vector3)move * lungeDistance * Time.deltaTime;
            //rb.velocity = new Vector2(lungeDistance, rb.velocity.y);
        }
    }
    private bool CanLunge(Vector2 lungeToNext)
    {
        Vector3Int gridPos = floorTilemap.WorldToCell(transform.position + (Vector3)move);
        Vector3Int lungeGridPos = floorTilemap.WorldToCell(transform.position + (Vector3)move * 2);
        if (!floorTilemap.HasTile(gridPos) || collisionTilemap.HasTile(gridPos) ||
            !floorTilemap.HasTile(lungeGridPos) || collisionTilemap.HasTile(lungeGridPos))
        {
            return false;
        }
        else if (move.x == 0 && move.y == 0)
        {
            return false;
        }
        lunging = true;
        return true;
    }

    public void LungeLogic()
    {
        Vector2 pos = transform.position;

        GameObject lungeFX = Instantiate(lungePrefab, pos, transform.rotation);
    }

    public void Earthquake()
    {
        quaking = true;
        if (abilities.earthquakeCooldown == false)
        {
            quaking = true;
            //sprite.color = new Color(0, 0, 1, 1);
            EarthquakeLogic();
            animator.SetTrigger("Quake");
            Camera.main.GetComponent<CameraFollow>().shakeDuration = 0.2f;
        }
        //sprite.color = new Color(1, 1, 1, 1);
        //Camera.main.GetComponent<CameraShake>().shakeDuration = 0.2f;
        //gameHandler.GetComponent<CameraShake>().shakeDuration = 0.2f;
    }

    public void EarthquakeLogic()
    {
        Vector2 pos = transform.position;

        GameObject earthquakeFX = Instantiate(earthquakePrefab, pos, transform.rotation);
    }


    private void Shield()
    {
        /*if (health < 100)
        {
            Debug.Log(health + "JODER");
            state = State.HEALING;
            defending = true;
            health = health * 10;
            UpdateHealth(health);
        }
        state = State.IDLE;*/
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    public void SavePlayer()
    {
        SaveSystem.SaveSusana(this);
    }

    public void LoadPlayer()
    {
        SusanaData data = SaveSystem.LoadSusana();

        //level = susana.level;
        health = data.health;
        nPotions = data.potions;
        nKeys = data.keys;
        nBombs = data.bombs;
        Vector3 position;
        position.x = data.position[0];
        position.y = data.position[1];
        position.z = data.position[2];
        transform.position = position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "SavePoint")
        {
            SavePlayer();
        }
        else if(collision.tag == "Fireball")
        {
            animator.SetTrigger("Hurt");
            health -= 20;
        }
        else if (collision.tag == "Potion")
        {
            GameManager.Instance.potionNumber++;
            nPotions++;
        }
        else if (collision.tag == "Key")
        {
            GameManager.Instance.keyNumber = 1;
            nKeys++;
        }
        else if (collision.tag == "NPC")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else if(collision.tag == "HealingTile")
        {
            part.Play();
        }
        else if(collision.tag == "Stairs")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1,LoadSceneMode.Single);
        }

        else if(collision.tag == "Truffle")
        {
            SceneManager.LoadScene("FinalScene", LoadSceneMode.Single);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "DamagingTile")
        {
            sprite.color = Color.red;
            animator.SetTrigger("Hurt");
            UpdateHealth(1);
        }
        else if (collision.tag == "HealingTile")
        {
            part.Play();
            UpdateHealth(-2);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //This to a manager
        if (collision.tag == "SavePoint")
        {
            if (saver == true)
            {
                LoadPlayer();
            }
        }

        if(collision.tag == "DamagingTile")
        {
            sprite.color = new Color(1, 1, 1, 1);
        }

        if (collision.tag == "HealingTile")
        {
            part.Stop();
        }
    }

    /*Save System Test Example
     * public void continueMatch()
    {
        SceneManager.LoadScene("MainRoomv3", LoadSceneMode.Single);
        LoadPlayer();
    }*/

    public void TakeDamage(int damage)
    {
        health -= damage;
        animator.SetTrigger("Hurt");
        damagepart.Play();
        GameObject au = Instantiate(ouch, transform.position, transform.rotation);
    }

    public void Heal()
    {
        state = State.HEALING;
        const int healAmount = 100;

        if (GameManager.Instance.potionNumber >= 1)
        {
            Debug.Log("Has usado una poción!");
            if (health <= (maxHealth - healAmount))
            {
                //Heart emission here
                //PotionLogic();
                health += healAmount;
                GameManager.Instance.potionNumber--;
                FindObjectOfType<SoundManager>().Play("Bite");
            }
            else if (health == maxHealth)
            {
                //PotionLogic();
                //Heart emission here
                health = maxHealth;
            }
        }
    }
}
