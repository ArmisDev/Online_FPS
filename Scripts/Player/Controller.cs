using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armis.Items;

namespace Armis
{
    [RequireComponent(typeof(CharacterController))]
    public class Controller : MonoBehaviour
    {
        //Character Section
        [Header("Controller Attributes")]
        [SerializeField] CharacterController characterController;
        [SerializeField] private float _walkSpeed = 0.5f;

        //Online Section
        [Header("Online Attributes")]
        public bool isLocal;

        #region - Float Inputs
        [HideInInspector]
        public float _horizontal;
        [HideInInspector]
        public float _vertical;
        [HideInInspector]
        public float _MouseX;
        [HideInInspector]
        public float _MouseY;
        #endregion

        //Camera Section
        [Header("Camera Parameters")]
        [SerializeField] private float _rotationSpeed;
        [SerializeField] Transform pivotTrans;
        [SerializeField] Transform camTrans;

        //Shooting section
        [Header("Shooting Parameters")]
        public bool isShooting = true;
        public RuntimeItem currentWeapon;

        [HideInInspector]
        public float _lookAngle;
        [HideInInspector]
        public float _tiltAngle;

        private void Start()
        {
            Init("TestWeapon");
        }

        public void Init(string weaponID)
        {
            characterController = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if(isLocal)
            {
                Armis.Utilities.Crosshair.singleton.Init(this);
            }

            currentWeapon = ItemManager.singleton.CreateItemInstance(weaponID);
            currentWeapon.Reload();
        }

        void HandleInput()
        {
            //Grabs our floats and apply our inputs to them
            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");
            _MouseX = Input.GetAxis("Mouse X");
            _MouseY = Input.GetAxis("Mouse Y");

            isShooting = Input.GetMouseButton(0);
        }

        void HandleMovement(float _delta)
        {
            /* Here we create a Vector 3 called direction and apply our transforms forward direction to it
             * we then take multiply our _vertical input float and multiply our forward direction by this value.
             * Next, we take the direction vector and subscribe it to transform.right and multiply this by 
             * our horizontal input float. This allows us to obtain both our local forward and local right positions
             * and store them into one Vector, which is then normalized
             */
            Vector3 direction = transform.forward * _vertical;
            direction += transform.right * _horizontal;
            direction.Normalize();

            //Here we are creating a raycast which will feed data about terrain difference (ie. Slopes, limits, etc.)
            RaycastHit hit;
            Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hit, characterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            
            //Here we are using the Vector3.ProjectOnPlane class to project a vector onto a plane (aka our hit.normal).
            Vector3 desiredDir = Vector3.ProjectOnPlane(direction, hit.normal).normalized;

            //Here we are checking to see if our player is grounded. If it is not, then we apply gravity to the player
            if(!characterController.isGrounded)
            {
                desiredDir += Physics.gravity;
            }

            //Finally we move our character using the desiredDir vector and multiply it by our delta float (aka Time.deltaTime - See Update) which is divied by our _walkSpeed
            characterController.Move(desiredDir * (_delta / _walkSpeed));
        }

        void HandleRotation(float _delta)
        {
            //Here we grab the _lookAngle float and subscribe it to our _MouseX input which we then multiply by our _delta (aka Time.deltaTime) and our _rotationSpeed
            _lookAngle += _MouseX * (_delta * _rotationSpeed);

            //Creating new euler for our camera rotation
            Vector3 camEulers = Vector3.zero;
            //Take our _lookAngle (now holds our inputs) and applies it to our camEulers Y axis
            camEulers.y = _lookAngle;
            //Rotates our character along Y axis (technically x axis).
            transform.eulerAngles = camEulers;

            /* Here we take our _tiltAngle float and subscribe it to our _MouseY float which is then multiplied by by our _delta (aka Time.deltaTime) and our _rotationSpeed
             * We want to use -= because we want the _MouseY to be inverted. This means Up on mouse is down, and vice versa. 
             * Basically it makes the X axis rotation for camera feel natural
             */
            _tiltAngle -= _MouseY * (_delta * _rotationSpeed);

            _tiltAngle = Mathf.Clamp(_tiltAngle, -45, 45);

            //Here we are creating a new euler for our tiltangle. 
            Vector3 tiltEuler = Vector3.zero;
            //Apply our _tiltAngle value to our tiltEuler Vector
            tiltEuler.x = _tiltAngle;
            //Grabs the local euler angle of our pivotTrans transform (aka the transforms degrees relative to the parent object) and applies it to our tiltEuler
            pivotTrans.localEulerAngles = tiltEuler;
        }

        void HandleShooting()
        {
            if(isShooting)
            {
                if(currentWeapon.canFire())
                {
                    currentWeapon.Shoot();
                 /*Here we are saying that if we are shooting then call the RaycastBullet function in our ballistics class
                 * This then takes three parameters the orgin of the shot, the direction of the shot
                 * and we then make sure to feed our camera controller back to the function using "this".
                 * Feeding the controller back to the RaycastBullet function is what actually enables this to be acted out on.
                 */
                    Ballistics.RaycastBullet(camTrans.position, camTrans.forward, this);
                }
            }
        }

        private void FixedUpdate()
        {
            float _delta = Time.fixedDeltaTime;
            HandleMovement(_delta);
            HandleRotation(_delta);
        }

        private void Update()
        {
            //Checks & Sets
            if (!isLocal) return;
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                    Cursor.lockState = CursorLockMode.None;
                else
                    Cursor.lockState = CursorLockMode.Locked;

                Cursor.visible = !Cursor.visible;
            }

            //Function Calls
            HandleInput();
            HandleShooting();
        }
    }
}
