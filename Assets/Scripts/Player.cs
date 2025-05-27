using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Helpers 
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}

public class Player : MonoBehaviour
{
    Rigidbody _rb;
    Animator _anim;
    Vector3 _input;
    public int currentLevelNumber;
    public PlayerUI ui;
    private Camera _camera;
    public float speed;
    public float speedTurn;
    public bool matrix = false;
    public float jumpSpeed = 1f;
    public float dashSpeed = 1f;
    public float dashLength = 0.02f;
    public float dashCooldown = 3f;
    private bool dashReady = true;
    private int health;
    public int startHealth;
    public Transform hitPoint;
    public float hitRadius = 3f;
    public int hitDamage;
    public float hitReload = 1f;
    private bool hitReady = true;
    public int shootDamage;
    public bool ammo = true;
    public GameObject bulletPrefab;
    private int energy;
    public int startEnergy;
    public float regenerateEnergySpeed = 1f;
    private bool anotherWorld = false;
    private bool swapReady = true;
    public float anotherWorldReload = 0.5f;
    private float stunnedTime = 3f;
    private bool isStunned = false;
    private bool groundPounding = false;
    public float groundPoundSpeed = 20f;
    public float groundPoundRadius = 5f;
    private bool isToggled = false;
    private bool isHelpWindow = false;
    public ParticleSystem hitStunParticles;
    public ParticleSystem dashParticles;
    public ParticleSystem deathParticles;
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public float sfxVolume = 0.7f;
    public float musicVolume = 0.5f;
    public AudioClip hitSound;
    public AudioClip hitStunSound;
    public AudioClip shootSound;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip damagedSound;
    public AudioClip recoveryEnergySound;
    public AudioClip deathSound;
    public AudioClip backgroundMusic;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _camera = Camera.main;
        health = startHealth;
        energy = startEnergy;
        ui.SetAnotherWorld(anotherWorld);
        if (currentLevelNumber == 2) ui.SetTaskLevel2(true);
        else ui.SetTaskLevel2(false);
        if (currentLevelNumber == 12) ui.SetTaskLevel12(true);
        else ui.SetTaskLevel12(false);
        ui.SetHelpWindow(isHelpWindow);
        StartCoroutine(RegenerateEnergy());
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
        PlayerPrefs.SetInt("CurrentLevel", currentLevelNumber);
        PlayerPrefs.Save();
    }

    private void Update() {
        InputGet();

        if (isHelpWindow)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        Look();
        if (IsGrounded()) {
            _anim.SetBool("isFalling", false);
        } else {
            _anim.SetBool("isFalling", true);
        }
        if (transform.position.y < -10)
        {
            Death();
        }
    }

    private void FixedUpdate() {
        if (_input != Vector3.zero)
        {
            Move();
        }
        _anim.SetBool("isRun", _input.magnitude > 0.1f);
        if (!IsGrounded() && _rb.linearVelocity.y < 0) {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * 2.5f * Time.deltaTime;
        }
    }





    private void InputGet()
    {
        _input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
            foreach (var col in colliders)
            {
                if ((col.CompareTag("DoobleJump") && anotherWorld) || IsGrounded())
                {
                    Jump();
                }
            }
        }
        if (dashReady && energy >= 5 && Input.GetKeyDown(KeyCode.LeftShift) && _input != Vector3.zero)
        {
            Dash(_input);
        }
        if (!IsGrounded() && Input.GetMouseButtonDown(0) && !groundPounding)
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(transform.position, Vector3.down, out raycastHit, 5f))
            {
                StartGroundPound();
            }
        }
        if (IsGrounded() && groundPounding)
        {
            EndGroundPound();
        }

        if (Input.GetMouseButton(0) && IsGrounded())
        {
            RotateToMouse();
            Hit(hitDamage);
        }
        if (Input.GetMouseButton(1) && ammo)
        {
            RotateToMouse();
            Shoot(shootDamage);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateToMouse();
            Interaction();
            if (anotherWorld)
            {
                ToggleDoor();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            WorldSwap();
        }
        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Escape))
        {
            isHelpWindow = !isHelpWindow;
            ui.SetHelpWindow(isHelpWindow);
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }

    private void RotateToMouse() {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float enter)) {
            Vector3 targetPoint = ray.GetPoint(enter);
            targetPoint.y = transform.position.y;

            Vector3 direction = (targetPoint - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Look() {
        if (_input == Vector3.zero) return;

        var rot = Quaternion.LookRotation(_input.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, speedTurn * Time.deltaTime);
    }

    private void Move() {
        float currentSpeed = 4;
        if (anotherWorld) currentSpeed = speed;
        _rb.MovePosition(transform.position + _input.normalized * currentSpeed * Time.deltaTime);
    }

    private void Jump() {
        sfxSource.PlayOneShot(jumpSound);
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, jumpSpeed, _rb.linearVelocity.z);
        SpendEnergy(1);
    }

    private void Dash(Vector3 inputDirection) {
        sfxSource.PlayOneShot(dashSound);
        if (dashParticles != null) dashParticles.Play();

        Vector3 dashDir = inputDirection.normalized;
        Vector3 dashVelocity = dashDir * dashSpeed;
        _rb.linearVelocity = new Vector3(dashVelocity.x, _rb.linearVelocity.y, dashVelocity.z);

        StartCoroutine(DashStop());
        SpendEnergy(5);
        dashReady = false;
        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashStop() {
        yield return new WaitForSecondsRealtime(dashLength);
        _rb.linearVelocity = new Vector3(0, 0, 0);
    }

    private IEnumerator DashCooldown() {
        yield return new WaitForSecondsRealtime(dashCooldown);
        dashReady = true;
    }





    private IEnumerator RegenerateEnergy()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(regenerateEnergySpeed);
            if (energy < startEnergy)
            {
                energy++;
                if (anotherWorld)
                {
                    energy += 2;
                    if (energy > startEnergy) energy = startEnergy;
                }
                ui.SetEnergy(energy);
            }
        }
    }
    
    public void SpendEnergy(int spend) {
        energy -= spend;
        if (energy < 0) energy = 0;
        ui.SetEnergy(energy);
    }

    private bool IsGrounded()
    {
        return Mathf.Abs(_rb.linearVelocity.y) < 0.01f;
    }

    public void GetDamage(int damage) {
        sfxSource.PlayOneShot(damagedSound);
        health -= damage;
        if (health < 0) health = 0;
        ui.SetHealth(health);
        if (health <= 0) {
            Death();
        }
    }

    private void Death() {
        Debug.Log("GAME OVER");
        sfxSource.PlayOneShot(deathSound);
        if (deathParticles != null) deathParticles.Play();
        Transform model = transform.childCount > 0 ? transform.GetChild(0) : null;
        if (model != null)
        {
            model.gameObject.SetActive(false);
            var renderer = model.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_Smoothness"))
            {
                renderer.material.SetFloat("_Smoothness", 0f);
            }
        }
        GetComponent<Collider>().enabled = false;
        _rb.isKinematic = true;
        this.enabled = false;
        StartCoroutine(RestartLevel());
    }

    private IEnumerator RestartLevel() {
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene(currentLevelNumber, LoadSceneMode.Single);
    }
    
    private void StartGroundPound() {
        _anim.SetBool("isHitStun", true);
        groundPounding = true;
        _rb.linearVelocity = new Vector3(0, -groundPoundSpeed, 0);
    }

    private void EndGroundPound() {
        _anim.SetBool("isHitStun", false);
        hitStunParticles.Play();
        sfxSource.PlayOneShot(hitStunSound);
        groundPounding = false;
        Collider[] colliders = Physics.OverlapSphere(transform.position, groundPoundRadius);
        
        foreach (var collider in colliders) {
            if (collider.TryGetComponent<MeleeEnemy>(out MeleeEnemy enemy)) {
                enemy.Stunned();
            }
            if (collider.TryGetComponent<RangeEnemy>(out RangeEnemy enemy2)) {
                enemy2.Stunned();
            }
            if (collider.TryGetComponent<SpecialEnemy>(out SpecialEnemy enemy3)) {
                enemy3.Stunned();
            }
        }
    }

    public void Hit(int damage)
    {
        if (hitReady)
        {
            sfxSource.PlayOneShot(hitSound);
            _anim.SetTrigger("hit");
            Collider[] colliders = Physics.OverlapSphere(hitPoint.position, hitRadius);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].TryGetComponent<MeleeEnemy>(out _))
                {
                    Debug.Log("DAMAGE");
                    MeleeEnemy enemy = colliders[i].GetComponent<MeleeEnemy>();
                    enemy.GetDamage(damage, false);
                }
                if (colliders[i].TryGetComponent<RangeEnemy>(out _))
                {
                    Debug.Log("DAMAGE");
                    RangeEnemy enemy = colliders[i].GetComponent<RangeEnemy>();
                    enemy.GetDamage(damage, false);
                }
                if (colliders[i].TryGetComponent<SpecialEnemy>(out _))
                {
                    Debug.Log("DAMAGE");
                    SpecialEnemy enemy = colliders[i].GetComponent<SpecialEnemy>();
                    enemy.GetDamage(damage, false);
                }
                if (colliders[i].TryGetComponent<Portal>(out _))
                {
                    Debug.Log("DAMAGE");
                    Portal portal = colliders[i].GetComponent<Portal>();
                    portal.GetDamage(damage);
                }
            }
            StartCoroutine(HitReloaded());
        }
    }

    private IEnumerator HitReloaded() {
        hitReady = false;
        yield return new WaitForSecondsRealtime(hitReload);
        hitReady = true;
    }

    public void Shoot(int damage) {
        sfxSource.PlayOneShot(shootSound);
        _anim.SetTrigger("shoot");
        ammo = false;
        ui.SetAmmo(ammo);
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1000f);  
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;
        bool targetIsInvisible = false;
        
        Vector3 mouseWorldPosition = Vector3.zero;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position.y);
        
        if (groundPlane.Raycast(ray, out float enter)) {
            mouseWorldPosition = ray.GetPoint(enter);
        }

        foreach (var collider in colliders) {
            if (collider.CompareTag("Enemy")) {
                float distanceToMouse = Vector3.Distance(mouseWorldPosition, collider.transform.position);
                MonoBehaviour enemy = collider.GetComponent<MonoBehaviour>();
                if (enemy != null && distanceToMouse < closestDistance) {
                    closestDistance = distanceToMouse;
                    closestEnemy = collider.transform;
                    if (enemy is MeleeEnemy meleeEnemy)
                    {
                        targetIsInvisible = meleeEnemy.typeInvisible;
                    }
                    else if (enemy is RangeEnemy rangeEnemy)
                    {
                        targetIsInvisible = rangeEnemy.typeInvisible;
                    }
                    else if (enemy is SpecialEnemy specialEnemy)
                    {
                        targetIsInvisible = specialEnemy.typeInvisible;
                    }
                }
            }
        }

        if (closestEnemy != null && !targetIsInvisible) {
            Vector3 shootPos = new Vector3(closestEnemy.position.x, closestEnemy.position.y, closestEnemy.position.z);;
            if (closestDistance >= 10f) {
                float targetHeight = closestEnemy != null ? closestEnemy.position.y : transform.position.y;
                shootPos = new Vector3(mouseWorldPosition.x, targetHeight, mouseWorldPosition.z);
            }
            GameObject obj = Instantiate(bulletPrefab, transform.GetChild(0).position, Quaternion.identity);
            obj.GetComponent<Bullet>().position = shootPos;
            obj.GetComponent<Bullet>().damage = shootDamage;
        }
    }

    public void Interaction() {
        float distance = 10f;
        GameObject portalObj = GameObject.FindGameObjectWithTag("Portal");
        if (portalObj != null)
        {
            distance = Vector3.Distance(transform.position, portalObj.transform.position);
            if (distance <= 3f)
            {
                Portal portal = portalObj.GetComponent<Portal>();
                if (portal != null && portal.isOpen)
                {
                    Debug.Log("PORTAL");
                    SceneManager.LoadScene(currentLevelNumber + 1, LoadSceneMode.Single);
                }
            }
        }
        GameObject crystalObj = GameObject.FindGameObjectWithTag("Crystal");
        if (crystalObj != null)
        {
            distance = Vector3.Distance(transform.position, crystalObj.transform.position);
            if (distance <= 3f)
            {
                Crystal crystal = crystalObj.GetComponent<Crystal>();
                if (crystal != null)
                {
                    crystal.CureOrigin();
                    StartCoroutine(ReturnToMenuAfterDelay());
                }
            }
        }
    }

    private IEnumerator ReturnToMenuAfterDelay() {
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }






    public void WorldSwap()
    {
        if (swapReady && !isStunned)
        {
            if (anotherWorld) anotherWorld = false;
            else anotherWorld = true;
            ui.SetAnotherWorld(anotherWorld);
            SetEnemiesVisibility(!anotherWorld);
            SetDoobleJumpVisibility(anotherWorld);
            SetAmmoRestoreVisibility(anotherWorld);
            StartCoroutine(AnotherWorldReloaded());
            if (musicSource != null) musicSource.volume = anotherWorld ? musicVolume / 4 : musicVolume;
        }
    }

    private IEnumerator AnotherWorldReloaded() {
        swapReady = false;
        yield return new WaitForSecondsRealtime(anotherWorldReload);
        swapReady = true;
    }

    private void SetAmmoRestoreVisibility(bool visible) {
        GameObject[] ammoRestores = GameObject.FindGameObjectsWithTag("AmmoRestore");
        foreach (GameObject obj in ammoRestores) {
            AmmoRestore currentAmmoRestore = obj.GetComponentInChildren<AmmoRestore>();
            if (currentAmmoRestore != null) {
                currentAmmoRestore.SetAnotherWorldVisible(visible);
            }
        }
    }

    private void SetDoobleJumpVisibility(bool visible) {
        GameObject[] doobleJumps = GameObject.FindGameObjectsWithTag("DoobleJump");
        foreach (GameObject obj in doobleJumps) {
            Renderer objRenderer = obj.GetComponent<Renderer>();
            if (objRenderer != null) {
                Color color = objRenderer.material.color;
                color.a = visible ? 0.6f : 0f;
                objRenderer.material.color = color;
            }
        }
    }

    private void SetEnemiesVisibility(bool visible) {
        MeleeEnemy[] enemies = FindObjectsOfType<MeleeEnemy>();
        foreach (MeleeEnemy enemy in enemies) {
            Renderer enemyRenderer = enemy.GetComponentInChildren<Renderer>();
            if (enemyRenderer != null && !enemy.typeInvisible) {
                Color color = enemyRenderer.material.color;
                color.a = visible ? 1f : 0f;
                float smoothnessValue = visible ? 0.5f : 0.0f;
                if (enemy.typeCrystal) {
                    color.a = visible ? 0.6f : 0f;
                    smoothnessValue = 0f;
                }
                enemyRenderer.material.color = color;
                enemyRenderer.material.SetFloat("_Smoothness", smoothnessValue);
            }
        }
        RangeEnemy[] enemies2 = FindObjectsOfType<RangeEnemy>();
        foreach (RangeEnemy enemy in enemies2) {
            Renderer enemyRenderer = enemy.GetComponent<Renderer>();
            if (enemyRenderer != null && !enemy.typeInvisible) {
                Color color = enemyRenderer.material.color;
                color.a = visible ? 1f : 0f;
                float smoothnessValue = visible ? 0.5f : 0.0f;
                if (enemy.typeCrystal) {
                    color.a = visible ? 0.6f : 0f;
                    smoothnessValue = 0f;
                }
                enemyRenderer.material.color = color;
                enemyRenderer.material.SetFloat("_Smoothness", smoothnessValue);
            }
        }
        SpecialEnemy[] enemies3 = FindObjectsOfType<SpecialEnemy>();
        foreach (SpecialEnemy enemy in enemies3) {
            Renderer enemyRenderer = enemy.GetComponent<Renderer>();
            if (enemyRenderer != null && !enemy.typeInvisible) {
                Color color = enemyRenderer.material.color;
                color.a = visible ? 1f : 0f;
                float smoothnessValue = visible ? 0.5f : 0.0f;
                if (enemy.typeCrystal) {
                    color.a = visible ? 0.6f : 0f;
                    smoothnessValue = 0f;
                }
                enemyRenderer.material.color = color;
                enemyRenderer.material.SetFloat("_Smoothness", smoothnessValue);
            }
        }
    }






    private void ToggleDoor()
    {
        if (!isToggled)
        {
            GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
            if (doors.Length == 0) return;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position.y);
            Vector3 mouseWorldPosition = Vector3.zero;

            if (groundPlane.Raycast(ray, out float enter))
            {
                mouseWorldPosition = ray.GetPoint(enter);
            }

            GameObject closestDoor = null;
            float closestDistance = float.MaxValue;

            foreach (var door in doors)
            {
                float distance = Vector3.Distance(mouseWorldPosition, door.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDoor = door;
                }
            }

            if (closestDoor != null)
            {
                Renderer doorRenderer = closestDoor.GetComponent<Renderer>();
                UnityEngine.AI.NavMeshObstacle obstacle = closestDoor.GetComponent<UnityEngine.AI.NavMeshObstacle>();

                if (doorRenderer != null)
                {
                    bool isTransparent = doorRenderer.material.color.a < 1f;

                    Color color = doorRenderer.material.color;
                    color.a = isTransparent ? 1f : 0.2f;
                    doorRenderer.material.color = color;

                    Door door = closestDoor.GetComponent<Door>();

                    door.playerCollider.enabled = isTransparent;
                    door.bulletCollider.enabled = isTransparent;
                    obstacle.enabled = isTransparent;

                    isToggled = true;
                    StartCoroutine(ToggleDoorReload());
                }
            }
        }
    }

    private IEnumerator ToggleDoorReload() {
        yield return new WaitForSecondsRealtime(0.25f);
        isToggled = false;
    }






    public void Stunned()
    {
        WorldSwap();
        isStunned = true;
        StartCoroutine(RecoverFromStun());
    }

    private IEnumerator RecoverFromStun() {
        yield return new WaitForSecondsRealtime(stunnedTime);
        isStunned = false;
        WorldSwap();
    }

    public bool GetAnotherWorldState() {
        return anotherWorld;
    }
}