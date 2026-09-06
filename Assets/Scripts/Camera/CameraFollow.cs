using System;
using UnityEngine;

namespace Game
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private float smoothSpeed = 10f;
        public static CameraFollow Singleton { get; private set; }

        private Transform target;

        private void Awake()
        {
            Singleton = this;
        }

        public void Set(Transform target)
        {
            this.target = target;
        }
        
        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 targetPosition = target.position;
            targetPosition.z = transform.position.z;

            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}