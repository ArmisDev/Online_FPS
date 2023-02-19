using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Armis
{
    public class Controller_scr : MonoBehaviour
    {
        [Header("Player Parameters")]
        [SerializeField] private float _defaultSpeed = 0.2f;
        [SerializeField] private float _runningSpeed = 0.08f;
        [SerializeField] private float _jumpForce = 5.0f;
        [SerializeField] private float _jumpSpeed = 5.0f;
        [SerializeField] [Range(0.01f, 0.5f)] private float _mvmtSmoothing;
        [SerializeField] private float _jumpSpeedSmoothing;
        [SerializeField] private float _gravityForce;
        public Transform weaponTrans;

        [Header("Camera Parameters")]
        [SerializeField] private Transform pivotTrans;
        [SerializeField] private Transform mainCameraTrans;
        [SerializeField] private float _lookSensitivity;
        [SerializeField] [Range(0.01f, 0.5f)] private float _lookSmoothing;

        [Header("Debug Stuff")]
        public float characterMagnitude;

        //public Animator gunAnimator;
        //public AnimationClip reloadAni;
        private bool isReloading;
        public bool canRun;
        private float moveVelocity;

        private WeaponLogic weaponLogic;

        //Private Variables
        [HideInInspector]
        public CharacterController characterController;
        #region - Input Floats -
        [HideInInspector]
        public float moveX;
        [HideInInspector]
        public float moveY;
        [HideInInspector]
        public float mouseX;
        [HideInInspector]
        public float mouseY;
        #endregion

        #region - Movement Vectors -
        private Vector3 targetDir = Vector3.zero;
        private Vector3 targetDirVelocity = Vector3.zero;
        Vector3 upwardForce;
        #endregion

        #region - Movement Floats -
        float _jumpSpeedVelocity;
        float _jumpCooldown = 0.5f;
        public float _timeSinceLanding = 0;
        #endregion

        #region - Movement Bools -
        public bool isRunning;
        public bool isMoving;
        public bool isGrounded;
        #endregion

        #region - Look Floats -
        float _lookRotation;
        float _tiltAngle;
        float _cameraPitch;
        #endregion

        #region - Look Bools -
        public bool isAiming;
        #endregion

        #region - Look Vectors -
        Vector2 currentDelta = Vector2.zero;
        Vector2 currentDeltaVelocity = Vector2.zero;
        Vector3 rotationEuler;
        #endregion

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            weaponLogic = GetComponentInChildren<WeaponLogic>();

            characterController = GetComponent<CharacterController>();

            isRunning = false;
            isGrounded = false;
            isAiming = false;
        }

        void HandleInput()
        {
            #region - Player Controls / Mouse Look Controls -
            moveX = Input.GetAxisRaw("Horizontal");
            moveY = Input.GetAxisRaw("Vertical");
            mouseX = Input.GetAxisRaw("Mouse X");
            mouseY = Input.GetAxisRaw("Mouse Y");
            #endregion
        }

        #region - Movement/Jump/MouseLook -

        void HandleMovement(float _delta)
        {
            Vector3 direction = transform.forward * moveY;
            direction += transform.right * moveX;
            direction.Normalize();

            targetDir = Vector3.SmoothDamp(targetDir, direction, ref targetDirVelocity, _mvmtSmoothing);

            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f);
            //Debug.DrawRay(transform.position, Vector3.down * 1.1f, Color.green);

            if (hit.collider != null)
            {
                isGrounded = true;
            }

            else isGrounded = false;

            //Jump

            //if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            //{
            //    _jumpSpeed = _jumpForce;
            //}

            //_jumpSpeed = Mathf.SmoothDamp(_jumpSpeed, 0f, ref _jumpSpeedVelocity, _jumpSpeedSmoothing);
            //Vector3 upwardForce = Vector3.up * _jumpSpeed;

                targetDir += upwardForce;
                //targetDir += Physics.gravity;
                targetDir.y -= _gravityForce * _delta;

            //Sprint Check

            if (Input.GetKeyDown(KeyCode.LeftShift) && !isRunning && canRun)
            {
                isRunning = true;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift) && isRunning && canRun)
            {
                isRunning = false;
            }

            if (isRunning && !weaponLogic.isReloadingWeapon)
            {
                characterController.Move(targetDir * (_delta / _runningSpeed));
            }

            else
            {
                characterController.Move(targetDir * (_delta / _defaultSpeed));
                //isRunning = false;
            }

            //Base Movement Check
            if(characterController.velocity.magnitude != 0 && isGrounded)
            {
                isMoving = true;
            }

            else isMoving = false;
        }

        void HandleJump(float _delta)
        {
            if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded)
            {
                float jumpForce = 10f;
                characterController.Move(Vector3.up * _delta * jumpForce);
            }
        }

        void HandleMouseLook(float _delta)
        {
            Vector2 targetMouseDelta = new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            currentDelta = Vector2.SmoothDamp(currentDelta, targetMouseDelta, ref currentDeltaVelocity, _lookSmoothing);

            _cameraPitch -= _delta * currentDelta.y * _lookSensitivity;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -45, 45);

            pivotTrans.localEulerAngles = Vector3.right * _cameraPitch;

            transform.Rotate(_delta * _lookSensitivity * currentDelta.x * Vector3.up);
        }

        #endregion

        

        private void Update()
        {
            #region - Variables/Cursor Check -
            float _delta = Time.deltaTime;
            characterMagnitude = characterController.velocity.magnitude;
            bool isRunningAni = isRunning;
            bool isGroundedAni = isGrounded;
            CharacterController characterControllerAni = characterController;

            if(Input.GetButtonDown("Cancel") && Cursor.visible == false)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            #endregion

            HandleInput();
            HandleMovement(_delta);
            HandleJump(_delta);
            HandleMouseLook(_delta);
            weaponLogic.HandleAnimations(characterControllerAni, isRunningAni, isGroundedAni);

            if (Input.GetKeyDown(KeyCode.R) && !isRunning && !weaponLogic.isReloadingWeapon)
            {
                weaponLogic.HandleReload();
            }

            if(isRunning)
            {
                weaponLogic.StopReload();
            }

            if (Input.GetButton("Fire1"))
            {
                weaponLogic.HandleShootLogic(isRunningAni);
            }

            if (Input.GetButton("Fire2"))
            {
                weaponLogic.WeaponAimIn();
            }

        }
    }
}
