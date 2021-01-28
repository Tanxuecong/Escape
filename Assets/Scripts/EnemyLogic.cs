using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyLogic : MonoBehaviour
{
    private Vector3 move_direction;
    private bool target_in_view;

    Animator shot_animator;

    int current_path;
    int current_hide_path;
    float shooting_cd = 1.0f;
    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;
    float stay_peek = 3.0f;
    int magBulletsVal = 30;
    int remainingBulletsVal = 90;
    int magSize = 30;
    int hit_success_rate = 20;
    GameObject[] hide_arr;

    public bool hide_peek = false; // set to true to enable enemy hide and peek
    public GameObject[] path_arr;
    public GameObject[] hide_arr1; // hide_arr1[0]:hide hide_arr1[1]:peek
    public GameObject[] hide_arr2; // hide_arr2[0]:hide hide_arr2[1]:peek
    public bool isDead;
    public float move_speed;
    public float health = 100;
    public GameObject player;
    public Camera enemy_view;
    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public GameObject BulletHole;
    public GameObject MuzzleFlash;
    public GameObject ShotSound;
    public GameObject gunMag;
    public GameObject handMag;


    // Start is called before the first frame update
    void Start()
    {
        current_path = 0;
        current_hide_path = 0;
        // Initializing animator values
        shot_animator = GetComponent<Animator>();
        shot_animator.SetFloat("walk_forward", 0.0f);
        shot_animator.SetFloat("walk_backward", 0.0f);
        shot_animator.SetFloat("walk_right", 0.0f);
        shot_animator.SetFloat("walk_left", 0.0f);

        //player = GameObject.Find("player");
    }

    // Update is called once per frame
    void Update()
    {
        if (health > 0)
        {
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z); // prevent enemy move along y-axis
        }
        Vector3 player_pos = player.transform.position;
        player_pos = new Vector3(player_pos.x + 0.5f, 0f, player_pos.z); // x + 0.5f to make enemy aim exactly at player
        
        // update shooting cooldown
        cool_down_update();

        if (!isDead)
        {

            target_in_view = is_target_in_view(enemy_view, player_pos);
            if (!hide_peek && target_in_view && !player.GetComponent<Gun>().isDead || (!hide_peek && health != 100 && !player.GetComponent<Gun>().isDead)) // find player
            {
                // not walking
                move_direction = new Vector3(0f, 0f, 0f);
                animation_movement(move_direction);

                // rotate towards player
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player_pos - transform.position), Time.deltaTime * 5);

                // if enemy distance to player is longer than 10 meters
                if (Vector3.Distance(player_pos, transform.position) > 10f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, player_pos, move_speed * Time.deltaTime * 3f);
                    Vector3 pos_diff = player_pos - transform.position;
                    shot_animator.SetBool("run", true);
                }
                else // distance is <= 10 meters
                {
                    shot_animator.SetBool("run", false);

                    if (gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead) // shoot player
                    {
                        move_direction = new Vector3(0f, 0f, 0f);
                        animation_movement(move_direction);
                        shotDetection();
                        addEffects();
                        shot_animator.SetBool("fire", true);
                        gunShotTime = 0.2f; // shoot 5 bullets per second
                        magBulletsVal = magBulletsVal - 1;

                        if (magBulletsVal <= 0 && remainingBulletsVal > 0)
                        {
                            shot_animator.SetBool("reloadAfterFire", true);
                            gunReloadTime = 2.5f;
                            Invoke("reloaded", 2.5f);
                        }
                    }
                    else
                    {
                        shot_animator.SetBool("fire", false);
                    }
                }
            }
            else if (hide_peek && target_in_view && !player.GetComponent<Gun>().isDead || (hide_peek && health != 100 && !player.GetComponent<Gun>().isDead)) // bonus for hide_peek
            {
                // rotate towards player
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player_pos - transform.position), Time.deltaTime * 5);
                
                // check which hide position is the closest
                if (Vector3.Distance(hide_arr1[0].transform.position, transform.position) < Vector3.Distance(hide_arr2[0].transform.position, transform.position))
                {
                    // hide_arr1 is closer
                    hide_arr = hide_arr1;
                }
                else
                {
                    hide_arr = hide_arr2;
                }

                if (Vector3.Distance(hide_arr[current_hide_path].transform.position, transform.position) < 1f && stay_peek <= 0)
                {
                    current_hide_path++;
                    // check whether it moves to the last target
                    if (current_hide_path >= hide_arr.Length)
                        current_hide_path = 0;

                    stay_peek = 3.0f; // peeking time
                }

                // hide peek movement
                if (!player.GetComponent<Gun>().isDead && Vector3.Distance(hide_arr[current_hide_path].transform.position, transform.position) < 1f && current_hide_path == 1 && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead) // shoot player in peek position
                {
                    shotDetection();
                    addEffects();
                    shot_animator.SetBool("fire", true);
                    gunShotTime = 0.2f; // shoot 5 bullets per second
                    magBulletsVal = magBulletsVal - 1;

                    if (magBulletsVal <= 0 && remainingBulletsVal > 0)
                    {
                        shot_animator.SetBool("reloadAfterFire", true);
                        gunReloadTime = 2.5f;
                        Invoke("reloaded", 2.5f);
                    }
                }
                else
                {
                    shot_animator.SetBool("fire", false);
                    Vector3 target_pos = hide_arr[current_hide_path].transform.position;
                    transform.position = Vector3.MoveTowards(transform.position, target_pos, move_speed * Time.deltaTime * 2f);
                    Vector3 pos_diff = target_pos - transform.position;
                    move_direction = pos_diff.normalized;
                    animation_movement(move_direction);
                }
            }
            else if (!target_in_view)
            {
                // check if the enemy reach the target position
                if (Vector3.Distance(path_arr[current_path].transform.position, transform.position) < 1f)
                {
                    current_path++;
                    // check whether it moves to the last target
                    if (current_path >= path_arr.Length)
                        current_path = 0;
                }

                Vector3 target_pos = path_arr[current_path].transform.position;
                // rotate towards target position
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target_pos - transform.position), Time.deltaTime * 3);

                transform.position = Vector3.MoveTowards(transform.position, target_pos, move_speed * Time.deltaTime);
                Vector3 pos_diff = target_pos - transform.position;
                move_direction = pos_diff.normalized;
                animation_movement(move_direction);
            }
        }
        else // enemy is dead
        {
            shot_animator.SetBool("run", false);
            if (gun.transform.parent)
            {
                // give gun rigid body and collider
                Rigidbody rb = gun.AddComponent(typeof(Rigidbody)) as Rigidbody;
                BoxCollider col = gun.AddComponent(typeof(BoxCollider)) as BoxCollider;
                col.size = new Vector3(0.047f, 0.27f, 1.02f);
                col.center = new Vector3(-0.006f, 0.04f, 0.012f);
                // make the gun independent
                gun.transform.parent = null;
            }
            
        }
    }

    bool is_target_in_view(Camera cam, Vector3 target_position)
    {
        // takes the given camera's view frustum and returns six planes that form it
        // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        var planes = GeometryUtility.CalculateFrustumPlanes(cam);
        for (int i = 0; i < 6; i++)
        {
            // if plane not facing towards the target_position
            if (planes[i].GetDistanceToPoint(target_position) < 0 || Vector3.Distance(target_position, transform.position) > 15f)
            {
                return false;
            }
        }
        return true;
    }

    void animation_movement(Vector3 move_direction)
    {
        // The enemy should not move
        if (move_direction.magnitude < 0.1f)
        {
            shot_animator.SetFloat("walk_forward", -1f);
            shot_animator.SetFloat("walk_backward", -1f);
            shot_animator.SetFloat("walk_right", -1f);
            shot_animator.SetFloat("walk_left", -1f);
            shot_animator.SetFloat("animation_speed", 0.0f);
        }
        else // The player should move
        {
            float forwardSpeed = move_direction.z;
            if (forwardSpeed > 0) // making forward walking speed faster
            {
                forwardSpeed = forwardSpeed * 2;
            }

            // Running the correct animation
            shot_animator.SetFloat("walk_forward", forwardSpeed);
            shot_animator.SetFloat("walk_backward", -move_direction.z);
            shot_animator.SetFloat("walk_right", move_direction.x);
            shot_animator.SetFloat("walk_left", -move_direction.x);

            // Setting animation running speed
            shot_animator.SetFloat("animation_speed", Mathf.Sqrt(Mathf.Pow(move_direction.x, 2f) + Mathf.Pow(forwardSpeed, 2f)));
        }
    }

    void cool_down_update()
    {
        if (shooting_cd >= 0.0f)
        {
            shooting_cd -= Time.deltaTime;
        }
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunReloadTime >= 0.0f)
        {
            gunReloadTime -= Time.deltaTime;
        }
        if (stay_peek >= 0.0f)
        {
            stay_peek -= Time.deltaTime;
        }
    }

    void shotDetection() // 20% chance hit the player
    {
        int layerMask = 1 << 9;
        layerMask = ~layerMask;
        RaycastHit rayHit;

        bool is_hit = false;
        bool is_hit_chest = false;
        bool is_hit_hand = false;
        bool is_hit_head = false;
        bool is_hit_leg = false;
        int rand_ishit = Random.Range(0, 100);
        int rand = Random.Range(0, 100);

        Vector3 player_body_position = player.transform.GetChild(1).transform.GetChild(0).transform.position;

        if (rand_ishit < hit_success_rate)
        {
            is_hit = true;
        }

        if (0 <= rand && rand < 30) // 30% hit chest
        {
            is_hit_chest = true;
        }
        else if (30 <= rand && rand < 50) // 20% hit hand
        {
            is_hit_hand = true;
        }
        else if (50 <= rand && rand < 60) // 10% hit head
        {
            is_hit_head = true;
        }
        else // 40% hit leg
        {
            is_hit_leg = true;
        }

        if (Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position).normalized, out rayHit, 100.0f, layerMask))
        {
            if (is_hit && rayHit.transform.tag == "Player")
            {
                if (is_hit_chest) // hit chest
                {
                    rayHit.transform.GetComponent<Gun>().Being_shot(30);
                }
                else if (is_hit_hand) // hit hand
                {
                    rayHit.transform.GetComponent<Gun>().Being_shot(10);
                }
                else if (is_hit_head) // hit head
                {
                    rayHit.transform.GetComponent<Gun>().Being_shot(100);
                }
                else // hit leg
                {
                    rayHit.transform.GetComponent<Gun>().Being_shot(20);
                }
            }
            else if (rayHit.transform.tag == "Untagged")
            {
                // bullet hole
                GameObject BulletHoleObject = Instantiate(BulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
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

    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
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

    void reloaded() // what happend after gun is done reloading
    {
        int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
        int addedBullets = newMagBulletsVal - magBulletsVal;
        magBulletsVal = newMagBulletsVal;
        remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        shot_animator.SetBool("reloadAfterFire", false);
    }

    public void Being_shot(float damage) // getting hit from player
    {
        health = health - damage;
        if (health <= 0)
        {
            isDead = true;
            shot_animator.applyRootMotion = true;
            shot_animator.SetBool("dead", true);
        }
    }
}
