using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class Gun : MonoBehaviour {

    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public GameObject gun2;
    public Animator animator;
    
    public GameObject spine;
    public GameObject handMag;
    public GameObject gunMag;
    public GameObject handMag2;
    public GameObject gunMag2;

    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;
    float gunSwitchTime = 1.2f;
    Quaternion previousRotation;
    public float health = 100;
    public bool isDead;
    bool isAk = false;



    public Text magBullets;
    public Text remainingBullets;

    int magBulletsVal = 30;
    int remainingBulletsVal = 90;
    int magSize = 30;

    int AKmagBulletsVal = 30;
    int AKremainingBulletsVal = 90;
    int AKmagSize = 30;
    public GameObject headMesh;
    public static bool leftHanded { get; private set; }

    public GameObject BulletHole;
    public GameObject WoodBulletHole;
    public GameObject BloodEffect;
    public GameObject MuzzleFlash;
    public GameObject ShotSound;
    public GameObject hp_ui;


    // Use this for initialization
    void Start() {
        headMesh.GetComponent<SkinnedMeshRenderer>().enabled = false; // Hiding player character head to avoid bugs :)
    }

    // Update is called once per frame
    void Update() {
        // update UI hp
        hp_ui.GetComponent<Text>().text = health.ToString();

        // Cool down times
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunReloadTime >= 0.0f)
        {
            gunReloadTime -= Time.deltaTime;
        }
        if (gunSwitchTime >= 0.0f)
        {
            gunSwitchTime -= Time.deltaTime;
        }

        if (!isAk)
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead)
            {
                shotDetection(); // Should be completed

                addEffects(); // Should be completed

                animator.SetBool("fire", true);
                gunShotTime = 0.5f;

                // Instantiating the muzzle prefab and shot sound

                magBulletsVal = magBulletsVal - 1;
                if (magBulletsVal <= 0 && remainingBulletsVal > 0)
                {
                    animator.SetBool("reloadAfterFire", true);
                    gunReloadTime = 2.5f;
                    Invoke("reloaded", 2.5f);
                }
            }
            else
            {
                animator.SetBool("fire", false);
            }

            if ((Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.R)) && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && remainingBulletsVal > 0 && magBulletsVal < magSize && !isDead)
            {
                animator.SetBool("reload", true); // run reload animation -> call ReloadEvent(1) & ReloadEvent(2)
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.0f);
            }
            else
            {
                animator.SetBool("reload", false);
            }

            // press q to switch weapon
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Q) && gunSwitchTime <= 0.0f)
            {
                animator.SetBool("switch", true); // run switch animation -> call SwitchEvent(1)
                gunSwitchTime = 1.2f;
            }
            else
            {
                animator.SetBool("switch", false);
            }
            updateText();
        }
        else
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && gunShotTime <= 0 && gunReloadTime <= 0.0f && AKmagBulletsVal > 0 && !isDead)
            {
                shotDetection(); // Should be completed

                addEffects(); // Should be completed

                animator.SetBool("fire", true);
                gunShotTime = 0.5f;

                // Instantiating the muzzle prefab and shot sound

                AKmagBulletsVal = AKmagBulletsVal - 1;
                if (AKmagBulletsVal <= 0 && AKremainingBulletsVal > 0)
                {
                    animator.SetBool("reloadAfterFire", true);
                    gunReloadTime = 2.5f;
                    Invoke("reloaded", 2.5f);
                }
            }
            else
            {
                animator.SetBool("fire", false);
            }

            if ((Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.R)) && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && AKremainingBulletsVal > 0 && AKmagBulletsVal < AKmagSize && !isDead)
            {
                animator.SetBool("reload", true); // run reload animation -> call ReloadEvent(1) & ReloadEvent(2)
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.0f);
            }
            else
            {
                animator.SetBool("reload", false);
            }

            // press q to switch weapon
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Q) && gunSwitchTime <= 0.0f)
            {
                animator.SetBool("switch", true); // run switch animation -> call SwitchEvent(1)
                gunSwitchTime = 1.2f;
            }
            else
            {
                animator.SetBool("switch", false);
            }
            AKupdateText();
        }
       
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "ammo")
        {
            remainingBulletsVal = 90;
        }
    }

    public void Being_shot(float damage) // getting hit from enemy
    {
        health = health - damage;
        if (health <= 0)
        {
            isDead = true;
            transform.GetComponent<CharacterMovement>().isDead = true;
            // make player's body lay on the ground
            transform.GetComponent<CharacterController>().height = 0f;
            transform.GetComponent<CharacterController>().radius = 0.01f;
            animator.SetBool("dead", true);
            headMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
    }

    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
    {
        if (!isAk)
        {
            if (eventNumber == 1)
            {
                gunMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
                handMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
            else if (eventNumber == 2)
            {
                gunMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
                handMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
        }
        else
        {
            if (eventNumber == 1)
            {
                gunMag2.GetComponent<MeshRenderer>().enabled = false;
                handMag2.GetComponent<MeshRenderer>().enabled = true;
            }
            else if (eventNumber == 2)
            {
                gunMag2.GetComponent<MeshRenderer>().enabled = true;
                handMag2.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public void SwitchEvent(int eventNumber)
    {
        if (eventNumber == 1)
        {
            if (!isAk)
            {
                gun.active = false;
                gun2.active = true;
                isAk = true;
            }
            else
            {
                gun.active = true;
                gun2.active = false;
                isAk = false;
            }
        }
    }

    void reloaded() // what happend after gun is done reloading
    {
        if (!isAk)
        {
            int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
            int addedBullets = newMagBulletsVal - magBulletsVal;
            magBulletsVal = newMagBulletsVal;
            remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        }
        else
        {
            int newMagBulletsVal = Mathf.Min(AKremainingBulletsVal + AKmagBulletsVal, AKmagSize);
            int addedBullets = newMagBulletsVal - AKmagBulletsVal;
            AKmagBulletsVal = newMagBulletsVal;
            AKremainingBulletsVal = Mathf.Max(0, AKremainingBulletsVal - addedBullets);
        }
        animator.SetBool("reloadAfterFire", false);

    }

    void updateText()
    {
        magBullets.text = magBulletsVal.ToString() ;
        remainingBullets.text = remainingBulletsVal.ToString();
    }

    void AKupdateText()
    {
        magBullets.text = AKmagBulletsVal.ToString();
        remainingBullets.text = AKremainingBulletsVal.ToString();
    }

    void shotDetection() // Detecting the object which player shot 
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        RaycastHit rayHit;
        if (Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position).normalized, out rayHit, 100.0f, layerMask))
        {
            if (rayHit.transform.tag == "head")
            {
                print("headshot");
                rayHit.transform.root.transform.GetComponent<EnemyLogic>().Being_shot(100); // 100 damage for headshot
                GameObject BloodEffectObject = Instantiate(BloodEffect, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(BloodEffectObject, 0.2f);
            }
            else if (rayHit.transform.tag == "enemy_hand")
            {
                print("handshot");
                rayHit.transform.root.transform.GetComponent<EnemyLogic>().Being_shot(10); // 10 damage for hand
                GameObject BloodEffectObject = Instantiate(BloodEffect, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(BloodEffectObject, 0.2f);
            }
            else if (rayHit.transform.tag == "enemy_leg")
            {
                print("legshot");
                rayHit.transform.root.transform.GetComponent<EnemyLogic>().Being_shot(20); // 20 damage for leg
                GameObject BloodEffectObject = Instantiate(BloodEffect, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(BloodEffectObject, 0.2f);
            }
            else if (rayHit.transform.tag == "enemy_chest")
            {
                print("chestshot");
                rayHit.transform.root.transform.GetComponent<EnemyLogic>().Being_shot(30); // 30 damage for chest
                GameObject BloodEffectObject = Instantiate(BloodEffect, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(BloodEffectObject, 0.2f);
            }
            else if (rayHit.transform.tag == "Untagged")
            {
                // bullet hole
                GameObject BulletHoleObject = Instantiate(BulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(BulletHoleObject, 1.0f);
            }
            else if (rayHit.transform.tag == "Cover")
            {
                // bullet hole
                GameObject BulletHoleObject = Instantiate(WoodBulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(BulletHoleObject, 1.0f);
            }
        }
    }

    void addEffects() // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        // muzzle flash
        GameObject flash = Instantiate(MuzzleFlash, end.transform.position, end.transform.rotation);
        flash.GetComponent<ParticleSystem>().Play();
        Destroy(flash, 1.0f);

        // shot sound
        Destroy((GameObject)Instantiate(ShotSound, transform.position, transform.rotation), 2.0f);
    }

}
