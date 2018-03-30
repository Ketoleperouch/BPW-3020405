using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour {
    
    //Variables
    #region Setup
    [Header("Setup")]
    public float clampAngle = 80f;
    public float moveSpeed = 3.0f;
    public float jumpForce = 250f;
    public float sensitivity = 150f;
    public float reloadDelay = 0.9f;
    public float shotExplosionForce = 600f;
    public float crouchSpeed = 5;
    public float crouchColHeight = 1.2f;
    public float crouchColY = -0.85f;
    public Vector3 originOffset = Vector3.zero;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayers;
    public LayerMask hitLayers;
    public string targetTag = "Enemy";
    public bool flipVerticalAxis = false;
    public bool disableMovement = false;
    public bool startWithEmptyClips = false;
    public GameObject m_playerCam;
    public GameObject shotParticles;
    public Transform barrel;
    public ParticleSystem muzzle;
    #endregion
    #region Weapon Specific
    [Header("Weapon Specific")]
    public float pistolDelay = 0.1f;
    public float pistolRecoil = 2.2f;
    public float pistolDamage = 1.0f;

    public float shotgunDelay = 1.2f;
    public float shotgunRecoil = 15.0f;
    public float shotgunDamage = 2.5f;

    public float rifleDelay = 0.05f;
    public float rifleRecoil = 1.5f;
    public float rifleDamage = 0.8f;
    #endregion
    #region Input
    [Header("Input")]
    public int fireButton = 0;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode reload = KeyCode.R;
    public KeyCode crouch = KeyCode.LeftControl;
    #endregion
    #region Stats
    [Header("Stats")]
    public int maxHealth = 100;
    public int health = 100;
    //Pistol
    public int pistolClipSize = 8;
    public int currentPistolClipSize { get; set; }
    public int totalPistolAmmo { get; set; }
    //Shotgun
    public int shotgunClipSize = 4;
    public int currentShotgunClipSize { get; set; }
    public int totalShotgunAmmo { get; set; }
    //Rifle
    public int rifleClipSize = 35;
    public int currentRifleClipSize { get; set; }
    public int totalRifleAmmo { get; set; }

    public int currentWeapon;

    [HideInInspector] public bool dead = false;
    #endregion
    #region HUD
    [Header("HUD")]
    public Text healthText;
    public Text ammoText;
    public Sprite[] crosshairTypes;
    public Image crosshair;
    public UIFader damageFader;
    public CanvasGroup blackScreen;
    #endregion
    #region Audio
    [Header("Audio")]
    public AudioClip[] pistolShotSounds;
    public AudioClip[] shotgunShotSounds;
    public AudioClip[] rifleShotSounds;
    public AudioClip reloadSound;
    public AudioClip switchWeaponSound;
    public AudioClip[] impactSounds;
    [HideInInspector] public AudioSource aSource;
    #endregion

    #region Private Variables
    //Private variables
    private float range = 30f;
    private float originalSpeed;
    private float shotDelay;
    private float shotRecoil;
    private float shotDamage;
    private float recoilBuffer;
    private bool grounded = true;
    private bool reloading = false;
    private Animator anim;
    private float xRot;
    private CapsuleCollider playerCol;
    private float originalColHeight;
    private float originalColY;
    private Rigidbody rb;
    private Vector3 move;
    private Vector2 originalSize;
    private List<EnemyToken> enemies;
    private float exitTimer;
    private bool exittable = false;
    #endregion

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UITextHandler.LogText("Look over there. The recon ship has returned. With the information it has on board, we might be able to save the world!", 0.1f);
        Physics.queriesHitTriggers = false;
        enemies = new List<EnemyToken>();
        anim = GetComponentInChildren<Animator>();
        currentPistolClipSize = pistolClipSize;
        rb = GetComponent<Rigidbody>();
        aSource = GetComponent<AudioSource>();
        originalSpeed = moveSpeed;
        originalSize = crosshair.GetComponent<RectTransform>().sizeDelta;
        shotRecoil = pistolRecoil;
        shotDamage = pistolDamage;
        playerCol = GetComponent<CapsuleCollider>();
        originalColHeight = playerCol.height;
        originalColY = playerCol.center.y;
        QueryAllEnemies();
        Respawn();
        StartCoroutine(FadeBlackScreen(0));
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Respawn()
    {
        //Setup stats
        health = maxHealth;

        //Setup weapons
        if (!startWithEmptyClips)
        {
            totalPistolAmmo = pistolClipSize * 5;
            currentPistolClipSize = pistolClipSize;

            totalShotgunAmmo = shotgunClipSize * 5;
            currentShotgunClipSize = shotgunClipSize;

            totalRifleAmmo = rifleClipSize * 5;
            currentRifleClipSize = rifleClipSize;
        }
        else
        {
            totalPistolAmmo = 0;
            currentPistolClipSize = 0;
        }

        currentWeapon = 0;
        UpdateWeapons();
    }

    private void Update()
    {
        if (dead)
        {
            return;
        }
        Move();
        CheckGrounded();
        CheckSwitch();
        VerticalLook();
        Shoot();
        UpdateCanvas();
        UpdateWeapons();

        health = Mathf.Clamp(health, 0, maxHealth);
        anim.SetBool("Unarmed", disableMovement);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!exittable)
            {
                UITextHandler.LogText("Press escape again to return to the menu.");
                exitTimer = Time.time + 2;
                exittable = true;
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
        if (Time.time > exitTimer)
        {
            exittable = false;
        }
    }

    private void Move()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");
        var rotation = Input.GetAxis("Mouse X") * sensitivity;

        move = new Vector3(x, 0, y).normalized;

        if (!disableMovement)
            transform.Translate(new Vector3(move.x * moveSpeed, grounded ? 0 : rb.velocity.y * Time.deltaTime, move.z * moveSpeed) * Time.deltaTime);
        transform.Rotate(Vector3.up, rotation * Time.deltaTime);

        if (Input.GetKeyDown(jumpKey) && grounded && !disableMovement)
        {
            rb.AddForce(new Vector3(0, jumpForce * rb.mass, 0));
        }

        if (Input.GetKey(crouch))
        {
            m_playerCam.transform.localPosition = new Vector3(0, Mathf.Lerp(m_playerCam.transform.localPosition.y, -0.5f, Time.deltaTime * crouchSpeed), 0);
            playerCol.height = crouchColHeight;
            playerCol.center = new Vector3(playerCol.center.x, crouchColY, playerCol.center.z);
            moveSpeed = originalSpeed / 2;
        }
        else
        {
            m_playerCam.transform.localPosition = new Vector3(0, Mathf.Lerp(m_playerCam.transform.localPosition.y, 0f, Time.deltaTime * crouchSpeed), 0);
            playerCol.height = originalColHeight;
            playerCol.center = new Vector3(playerCol.center.x, originalColY, playerCol.center.z);
            moveSpeed = originalSpeed;
        }
    }

    private void VerticalLook()
    {
        var mouseX = (flipVerticalAxis ? Input.GetAxis("Mouse Y") : -Input.GetAxis("Mouse Y")) * sensitivity;

        xRot += mouseX * Time.deltaTime;
        xRot += recoilBuffer;
                
        Quaternion localRotation = Quaternion.Euler(xRot, 0, 0);
        Camera.main.transform.localRotation = localRotation;

        recoilBuffer = Mathf.MoveTowards(recoilBuffer, 0, Time.deltaTime * 2);
        xRot = Mathf.Clamp(xRot, -clampAngle, clampAngle);
    }

    private void CheckGrounded()
    {
        grounded = Physics.Raycast(transform.position + originOffset, Vector3.down, groundCheckRadius, groundLayers);
    }

    private void CheckSwitch()
    {
        RaycastHit hit;
        Vector3 center = m_playerCam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(center, m_playerCam.transform.forward, out hit, 2.5f))
        {
            if (hit.collider.CompareTag("Switch") && Input.GetKeyDown(KeyCode.E))
            {
                hit.collider.GetComponent<Switch>().Activate();
            }
        }
    }

    private void Shoot()
    {
        if (disableMovement)
        {
            return;
        }
        if ((currentWeapon != 2 ? Input.GetMouseButtonDown(fireButton) : Input.GetMouseButton(fireButton)) && !reloading)
        {
            PrepareForWeaponFire();
        }
        switch (currentWeapon)
        {
            case 0:
                if (Input.GetKeyDown(reload) && currentPistolClipSize < pistolClipSize && totalPistolAmmo > 0 && !reloading)
                {
                    StartCoroutine(Reload());
                }
                break;
            case 1:
                if (Input.GetKeyDown(reload) && currentShotgunClipSize < shotgunClipSize && totalShotgunAmmo > 0 && !reloading)
                {
                    StartCoroutine(Reload());
                }
                break;
            case 2:
                if (Input.GetKeyDown(reload) && currentRifleClipSize < rifleClipSize && totalRifleAmmo > 0 && !reloading)
                {
                    StartCoroutine(Reload());
                }
                break;
        }
    }

    private void PrepareForWeaponFire()
    {
        if (currentWeapon == 0)
        {
            //Check if ammo left, otherwise reload.
            if (currentPistolClipSize > 0)
            {
                if (Time.time > shotDelay)
                {
                    //Set delay
                    shotDelay = Time.time + pistolDelay;
                    //Ammo available, shoot.
                    FireBullet();
                }
            }
            else if (currentPistolClipSize < pistolClipSize && totalPistolAmmo > 0 && Time.time > shotDelay)
            {
                //No ammo in current clip, reload.
                StartCoroutine(Reload());
            }
            else
            {
                //Out of ammo.
                anim.SetTrigger("Shoot");
            }
            anim.SetBool("NoAmmo", totalPistolAmmo <= 0 && currentPistolClipSize <= 0);
        }
        if (currentWeapon == 1)
        {
            //Check if ammo left, otherwise reload.
            if (currentShotgunClipSize > 0)
            {
                if (Time.time > shotDelay)
                {
                    //Set delay
                    shotDelay = Time.time + shotgunDelay;
                    //Ammo available, shoot.
                    FireBullet();
                }
            }
            else if (currentShotgunClipSize < shotgunClipSize && totalShotgunAmmo > 0 && Time.time > shotDelay)
            {
                //No ammo in current clip, reload.
                StartCoroutine(Reload());
            }
            else
            {
                //Out of ammo.
                anim.SetTrigger("Shoot");
            }
            anim.SetBool("NoAmmo", totalShotgunAmmo <= 0 && currentShotgunClipSize <= 0);
        }
        if (currentWeapon == 2)
        {
            //Check if ammo left, otherwise reload.
            if (currentRifleClipSize > 0)
            {
                if (Time.time > shotDelay)
                {
                    //Set delay
                    shotDelay = Time.time + rifleDelay;
                    //Ammo available, shoot.
                    FireBullet();
                }
            }
            else if (currentRifleClipSize < rifleClipSize && totalRifleAmmo > 0 && Time.time > shotDelay)
            {
                //No ammo in current clip, reload.
                StartCoroutine(Reload());
            }
            else
            {
                //Out of ammo.
                anim.SetTrigger("Shoot");
            }
            anim.SetBool("NoAmmo", totalRifleAmmo <= 0 && currentRifleClipSize <= 0);
        }
    }

    private void FireBullet()
    {
        //Set stuff
        anim.SetTrigger("Shoot");
        switch (currentWeapon)
        {
            case 0:
                currentPistolClipSize--;
                AudioSource.PlayClipAtPoint(pistolShotSounds[Random.Range(0, pistolShotSounds.Length)], barrel.position);
                break;
            case 1:
                currentShotgunClipSize--;
                AudioSource.PlayClipAtPoint(shotgunShotSounds[Random.Range(0, shotgunShotSounds.Length)], barrel.position);
                break;
            case 2:
                currentRifleClipSize--;
                AudioSource.PlayClipAtPoint(rifleShotSounds[Random.Range(0, rifleShotSounds.Length)], barrel.position);
                break;
        }
        muzzle.Stop(true);
        muzzle.Play(true);
        //Add recoil, smaller recoil buffer when shotgun equipped
        xRot -= shotRecoil;
        recoilBuffer = shotRecoil / (currentWeapon == 1 ? 15 : 8);
        //Shot Impact at center of screen
        RaycastHit hit;
        Vector3 shotOrigin = m_playerCam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        float crouchAccuracyFactor = Input.GetKey(crouch) ? 100 - recoilBuffer : 60 - recoilBuffer;
        Vector3 shotInaccuracy = new Vector3(Random.Range(-shotRecoil, shotRecoil), Random.Range(-shotRecoil, shotRecoil), Random.Range(-shotRecoil, shotRecoil)) / crouchAccuracyFactor;
        Debug.DrawRay(barrel.position, m_playerCam.transform.forward * range + shotInaccuracy, Color.yellow);
        if (Physics.Raycast(shotOrigin, m_playerCam.transform.forward + shotInaccuracy, out hit, range, hitLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.tag == targetTag)
            {
                if (hit.collider.GetComponent<Rigidbody>())
                    hit.collider.GetComponent<Rigidbody>().AddExplosionForce(shotExplosionForce, hit.point, 1);
                //Search for affected scripts
                Damage(hit);
            }
            Instantiate(shotParticles, hit.point, Quaternion.identity);
        }
        // Shotgun adds five more bullets.
        if (currentWeapon == 1)
        {
            for (int i = 0; i < 5; i++)
            {
                shotInaccuracy = new Vector3(Random.Range(-shotRecoil, shotRecoil), Random.Range(-shotRecoil, shotRecoil), Random.Range(-shotRecoil, shotRecoil)) / crouchAccuracyFactor;
                Debug.DrawRay(barrel.position, m_playerCam.transform.forward * range + shotInaccuracy, Color.yellow);
                if (Physics.Raycast(shotOrigin, m_playerCam.transform.forward + shotInaccuracy, out hit, range, hitLayers, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.tag == targetTag)
                    {
                        if (hit.collider.GetComponent<Rigidbody>())
                            hit.collider.GetComponent<Rigidbody>().AddExplosionForce(shotExplosionForce, hit.point, 1);
                        //Search for affected scripts
                        Damage(hit);
                    }
                    Instantiate(shotParticles, hit.point, Quaternion.identity);
                }
            }
        }
    }

    private void Damage(RaycastHit hit)
    {
        if (hit.collider.GetComponent<Destructable>())
        {
            hit.collider.GetComponent<Destructable>().Damage(hit.point, shotDamage);
        }
        if (hit.collider.GetComponentInParent<Destructable>())
        {
            hit.collider.GetComponentInParent<Destructable>().Damage(hit.point, shotDamage);
        }
    }

    private IEnumerator Reload()
    { 
        reloading = true;
        anim.SetTrigger("Reload");
        switch (currentWeapon)
        {
            case 0:
                while (totalPistolAmmo > 0 && currentPistolClipSize < pistolClipSize)
                {
                    currentPistolClipSize++;
                    totalPistolAmmo--;
                }
                break;
            case 1:
                while (totalShotgunAmmo > 0 && currentShotgunClipSize < shotgunClipSize)
                {
                    currentShotgunClipSize++;
                    totalShotgunAmmo--;
                    yield return new WaitForSeconds(0.1f);
                }
                break;
            case 2:
                while (totalRifleAmmo > 0 && currentRifleClipSize < rifleClipSize)
                {
                    currentRifleClipSize++;
                    totalRifleAmmo--;
                }
                break;
        }
        yield return new WaitForSeconds(reloadDelay);
        if (currentWeapon == 2)
        {
            //Longer Rifle Loading time
            yield return new WaitForSeconds(reloadDelay);
        }
        reloading = false;
    }

    public void ShakeScreen(float duration, float intensity)
    {
        StartCoroutine(ShakeScreenI(duration, intensity));
    }

    private IEnumerator ShakeScreenI(float duration, float intensity)
    {
        Vector3 originalPosition = m_playerCam.transform.position;
        float times = 0;
        while (times < duration * 10)
        {
            m_playerCam.transform.position = m_playerCam.transform.position + Random.insideUnitSphere * intensity / 10;
            times++;
            intensity /= 1.1f;
            yield return null;
        }
        originalPosition = m_playerCam.transform.position;
        m_playerCam.transform.position = originalPosition;
    }

    public void UpdateWeapons()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            crosshair.sprite = crosshairTypes[0];
            if (currentWeapon != 0)
            {
                aSource.clip = switchWeaponSound;
                aSource.Play();
            }
            currentWeapon = 0;
            shotRecoil = pistolRecoil;
            shotDamage = pistolDamage;
            StopCoroutine(Reload());
            reloading = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            crosshair.sprite = crosshairTypes[1];
            if (currentWeapon != 1)
            {
                aSource.clip = switchWeaponSound;
                aSource.Play();
            }
            currentWeapon = 1;
            shotRecoil = shotgunRecoil;
            shotDamage = shotgunDamage;
            StopCoroutine(Reload());
            reloading = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            crosshair.sprite = crosshairTypes[2];
            if (currentWeapon != 2)
            {
                aSource.clip = switchWeaponSound;
                aSource.Play();
            }
            currentWeapon = 2;
            shotRecoil = rifleRecoil;
            shotDamage = rifleDamage;
            StopCoroutine(Reload());
            reloading = false;
        }
        if (disableMovement)
        {
            crosshair.sprite = crosshairTypes[3];
        }
        anim.SetInteger("Weapon", currentWeapon);
        if (currentWeapon == 1)
        {
            crosshair.GetComponent<RectTransform>().sizeDelta = originalSize + (new Vector2(recoilBuffer, recoilBuffer) * 10);
        }
        else
        {
            crosshair.GetComponent<RectTransform>().sizeDelta = originalSize + (new Vector2(recoilBuffer, recoilBuffer) * 80);
        }
    }

    public void TakeDamage(int damage, Vector3 point)
    {
        health -= damage;
        aSource.clip = impactSounds[Random.Range(0, impactSounds.Length)];
        aSource.Play();
        ShakeScreen(4, damage * 0.5f);
        damageFader.Flash();
        if (health <= 0)
        {
            health = 0; //Health should not be a negative digit
            UpdateCanvas(); //Force canvas update
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        dead = true;
        UITextHandler.LogText("You are dead!");
        yield return StartCoroutine(FadeBlackScreen(1));
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    public IEnumerator FadeBlackScreen(float alpha)
    {
        while (!Mathf.Approximately(blackScreen.alpha, alpha))
        {
            blackScreen.alpha = Mathf.MoveTowards(blackScreen.alpha, alpha, Time.deltaTime * 2);
            yield return null;
        }
    }

    private void UpdateCanvas()
    {
        healthText.text = "Health: " + health;
        switch (currentWeapon)
        {
            case 0:
                ammoText.text = "Ammo: " + currentPistolClipSize + " / " + totalPistolAmmo;
                break;
            case 1:
                ammoText.text = "Ammo: " + currentShotgunClipSize + " / " + totalShotgunAmmo;
                break;
            case 2:
                ammoText.text = "Ammo: " + currentRifleClipSize + " / " + totalRifleAmmo;
                break;
        }
    }

    private void QueryAllEnemies()
    {
        enemies.Clear();
        //Seek for hostile unit scripts, like Turrets, and add to Enemies List.
        foreach (EnemyToken enemy in FindObjectsOfType<EnemyToken>())
        {
            enemies.Add(enemy);
        }
    }

    public bool InDanger()
    {
        QueryAllEnemies();
        for (int i = 0; i < enemies.ToArray().Length; i++)
        {
            if (enemies[i].currentTarget == transform)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + originOffset, groundCheckRadius);
    }
}