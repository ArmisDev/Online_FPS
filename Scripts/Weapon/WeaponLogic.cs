using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armis.ItemTypes.Weapon;
using Armis;
using TMPro;

public class WeaponLogic : MonoBehaviour
{
    [Header("Weapon Parameters")]
    public GameObject muzzleFlash;
    public ParticleSystem muzzleFlashPart;
    public ParticleSystem MuzzleSparks;
    public float currentMag;
    private float timeSinceLastFire;
    private int recountReload;
    private bool canShoot;
    private bool canReload;
    public bool canLowerWeapon;
    private bool isLowerd;
    private bool isADS;
    public bool canADS;
    public bool isReloadingWeapon;
    public float aimInTime;
    [SerializeField] private Controller_scr controllerScript;
    [SerializeField] private TextMeshProUGUI ammoHUD;

    #region - Weapon Aim -
    public Transform pointA;
    public Transform pointB;
    public float speed = 1.0f;

    private bool isMoving = false;
    private float startTime;
    private float journeyLength;
    private Vector3 startPosition;
    #endregion

    //Vectors
    public Vector3 weaponStartPos;
    Vector3 weaponStartPosTemp;
    public Vector3 weaponAimPos;
    private Vector3 currentWeaponPos;
    public Vector3 targetWeaponPos;
    private Vector3 targetweaponPosVelocity = Vector3.zero;

    RaycastHit raycastHit;

    public ItemTypes_Weapon itemTypes_Weapon;
    [SerializeField] private AudioSource weaponAudio;
    [SerializeField] private AudioClip[] weaponSounds;

    [Header("Animation Parameters")]
    public Animator gunAnimator;
    [Tooltip("Array is used to store animation clips for weapon. This can be used to assign corutine lengths based off animation lengths")]
    public AnimationClip[] animationClips;

    private float walkAniCapX;
    private float walkAniCapY;

    // Start is called before the first frame update

    private void Awake()
    {
        ammoHUD.text = "AMMO " + "\n" + itemTypes_Weapon.magSize;
    }

    void Start()
    {
        muzzleFlash.SetActive(false);
        canReload = true;
        isLowerd = false;
        canShoot = true;
        weaponAudio = GetComponent<AudioSource>();
        recountReload = itemTypes_Weapon.magSize;

        weaponStartPos = controllerScript.weaponTrans.localPosition;

        startPosition = transform.position;
        pointA = controllerScript.weaponTrans;
        journeyLength = Vector3.Distance(pointA.position, pointB.position);

        startPosition = transform.position;
    }

    #region - Animation -

    public void HandleAnimations(CharacterController characterController, bool isRunning, bool isGrounded)
    {
        //Walk
        //moveVelocity = Mathf.Lerp(0,1, characterController.velocity.magnitude * 0.1f);

        if (characterController.velocity.magnitude > 1f && isGrounded && !isReloadingWeapon)
        {
            gunAnimator.SetFloat("Horizontal", 1f, 0.6f, Time.deltaTime);
            gunAnimator.SetFloat("Vertical", 2f, 0.1f, Time.deltaTime);
        }

        else
        {
            gunAnimator.SetFloat("Horizontal", 0, 0.1f, Time.deltaTime);
            gunAnimator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
        }

        //Sprint
        if (characterController.velocity.magnitude > 0.1f && isGrounded && isRunning && !isReloadingWeapon)
        {
            gunAnimator.SetBool("Running", true);
        }

        else gunAnimator.SetBool("Running", false);

        //Falling
        if(!isGrounded)
        {
            gunAnimator.SetBool("Jumping", true);
        }

        else gunAnimator.SetBool("Jumping", false);

        //Holster
        if(canLowerWeapon && !isADS)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !isRunning && !isReloadingWeapon && !gunAnimator.GetBool("Firing") && !isLowerd)
            {
                gunAnimator.SetBool("Lowered", true);
                isLowerd = true;
                canShoot = false;
                canReload = false;
                canADS = false;
            }

            else if (Input.GetKeyDown(KeyCode.Q) && isLowerd)
            {
                gunAnimator.SetBool("Lowered", false);
                isLowerd = false;
                canShoot = true;
                canReload = true;
                canADS = true;
            }
        }
    }

    public void HandleShootLogic(bool isRunning)
    {
        currentMag = itemTypes_Weapon.magSize;

        if (canShoot && !isRunning)
        {
            timeSinceLastFire += Time.deltaTime;
            

            if (itemTypes_Weapon.fireRate <= timeSinceLastFire && itemTypes_Weapon.magSize != 0)
            {
                #region - Weapon Feedback -
                itemTypes_Weapon.magSize--;
                gunAnimator.SetBool("Firing", true);
                weaponAudio.PlayOneShot(weaponSounds[Random.Range(0, 2)]);
                timeSinceLastFire = 0f;
                #endregion
                #region - Weapon Raycast -
                Physics.Raycast(transform.position, transform.forward, out raycastHit, itemTypes_Weapon.rangeOfWeapon);

                RaycastLogic(raycastHit);
                canReload = false;

                muzzleFlash.SetActive(true);
                muzzleFlashPart.Play();
                MuzzleSparks.Play();
                #endregion

            }

            else
            {
                canReload = true;
                gunAnimator.SetBool("Firing", false);
                muzzleFlash.SetActive(false);
                muzzleFlashPart.Stop();
                MuzzleSparks.Stop();
            }
        }

        else return;
    }

    public void WeaponAimIn()
    {
        if(canADS)
        {
            if(Input.GetButtonDown("Fire2") && !isADS)
            {
                gunAnimator.SetBool("Aim", true);
                isADS = true;
                controllerScript.canRun = false;
                canLowerWeapon = false;
            }

            else if(Input.GetButtonDown("Fire2") && isADS)
            {
                gunAnimator.SetBool("Aim", false);
                isADS = false;
                controllerScript.canRun = true;
                canLowerWeapon = true;
            }
        }
    }

    public void WeaponAimOut()
    {
        //currentWeaponPos = controllerScript.weaponTrans.position;

        //targetWeaponPos = Vector3.SmoothDamp(targetWeaponPos, currentWeaponPos, ref targetweaponPosVelocity, aimInTime);
    }

    public void RaycastLogic(RaycastHit hit)
    {
        if (hit.collider != null)
        {
            Debug.Log("I hit something at " + hit.normal);
        }

        else Debug.Log("Hit Nothing");
    }

    public void HandleReload()
    {
        StartCoroutine(ReloadCorutine());
    }

    public void StopReload()
    {
        StopCoroutine(ReloadCorutine());
    }

    IEnumerator ReloadCorutine()
    {
        if (canReload && !isReloadingWeapon)
        {
            canShoot = false;
            isReloadingWeapon = true;
            gunAnimator.SetBool("Reloading", true);
            gunAnimator.Play("reload");
            weaponAudio.PlayOneShot(weaponSounds[3]);
            yield return new WaitForSeconds(animationClips[0].length);
            itemTypes_Weapon.magSize = recountReload;
            gunAnimator.SetBool("Reloading", false);
            isReloadingWeapon = false;
            canShoot = true;
        }

        else yield break;
    }

    private void HUDHandler()
    {
        ammoHUD.text = "AMMO " + "\n" + itemTypes_Weapon.magSize;
    }

    private void Update()
    {
        HUDHandler();
    }

    #endregion
}
