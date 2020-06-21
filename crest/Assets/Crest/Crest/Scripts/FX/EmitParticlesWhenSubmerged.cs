﻿// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Triggers the attached particle system when the water at this position exceeds a given velocity upwards. Useful for splashes.
    /// </summary>
    public class EmitParticlesWhenSubmerged : MonoBehaviour
    {
        [Header("Emission Settings")]

        [Tooltip("Minimum vertical water velocity, emits if water exceeds this velocity upwards."), SerializeField]
        float _minimumVelocity = 0.4f;

        [Tooltip("Maximum difference in height between the water surface and this position. No emission if water is too far above/below this position."), SerializeField]
        float _maximumHeightDifference = 0.5f;

        [Tooltip("Decrease to respond to smaller waves. Increase to filter out small waves and only emit from bigger waves."), SerializeField]
        float _minWavelength = 1f;

        [Tooltip("Scale value for particle speed multiplier. Time axis is proportion of the Minimum Velocity setting. Value of curve at Time=1 is used if water vel exactly matches minimum emission velocity. Value of curve at Time=3 is used if water vel is 3x greater than the minimum velocity."), SerializeField]
        AnimationCurve _initalVelVsWaterVel2 = new AnimationCurve(new Keyframe[] { new Keyframe(1f, 1f), new Keyframe(4f, 5f) });

        [Header("Debug Settings")]
        [Tooltip(""), SerializeField]
        bool _logEvents = false;

        ParticleSystem _particleSystem = null;
        SampleHeightHelper _sampleHeightHelper = new SampleHeightHelper();

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            if (_particleSystem == null)
            {
                Debug.LogError("No particle system attached, disabling EmitParticlesWhenSubmerged.", this);
                enabled = false;
                return;
            }
        }

        void Update()
        {
            if (_particleSystem.isEmitting)
                return;

            _sampleHeightHelper.Init(transform.position, _minWavelength * 2f);

            float height = 0f;
            var dummy = Vector3.zero;
            var vel = Vector3.zero;
            if (_sampleHeightHelper.Sample(ref height, ref dummy, ref vel))
            {
                if (Mathf.Abs(height - transform.position.y) < _maximumHeightDifference && vel.y > _minimumVelocity && !_particleSystem.isPlaying)
                {
                    _particleSystem.Play();

                    var module = _particleSystem.main;
                    module.startSpeedMultiplier = _initalVelVsWaterVel2.Evaluate(vel.y / _minimumVelocity);

                    if (_logEvents)
                    {
                        Debug.Log($"Particle emission, emit speed multiplier: {module.startSpeedMultiplier}", this);
                    }
                }
            }
        }
    }
}
