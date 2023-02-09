using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArmisDev
{
    [RequireComponent(typeof(CharacterController))]
    public class Controller : MonoBehaviour
    {
        [Header("Controller Attributes")]
        [SerializeField] CharacterController characterController;
        [SerializeField] private float _walkSpeed = 0.5f;
        [SerializeField] Transform pivotTrans;
        [SerializeField] Transform camTrans;

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

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
        }

        void HandleInput()
        {
            //Grabs our floats and apply our inputs to them
            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");
            _MouseX = Input.GetAxis("Mouse X");
            _MouseY = Input.GetAxis("Mouse Y");
        }

        void HandleMovement(float delta)
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
            characterController.Move(desiredDir * (delta / _walkSpeed));
        }

        private void Update()
        {
            if (!isLocal) return;

            HandleInput();
            HandleMovement(Time.deltaTime);
        }
    }
}
