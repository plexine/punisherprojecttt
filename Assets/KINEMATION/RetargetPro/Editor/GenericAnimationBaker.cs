﻿// Designed by KINEMATION, 2024.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.RetargetPro.Editor
{
    public class AnimationFrame
    {
        public string path = "";
        public Transform boneReference;
        
        public AnimationCurve localPositionX = new AnimationCurve();
        public AnimationCurve localPositionY = new AnimationCurve();
        public AnimationCurve localPositionZ = new AnimationCurve();
        
        public AnimationCurve localRotationX = new AnimationCurve();
        public AnimationCurve localRotationY = new AnimationCurve();
        public AnimationCurve localRotationZ = new AnimationCurve();
        public AnimationCurve localRotationW = new AnimationCurve();
        
        public AnimationCurve localScaleX = new AnimationCurve();
        public AnimationCurve localScaleY = new AnimationCurve();
        public AnimationCurve localScaleZ = new AnimationCurve();
    }
    
    public class GenericAnimationBaker
    {
        private List<AnimationFrame> _animationFrames = new List<AnimationFrame>();
        private bool _keyframeAll;

        private void AddLinearKey(AnimationCurve curve, float time, float value)
        {
            int index = curve.AddKey(time, value);

            AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, index, AnimationUtility.TangentMode.Linear);

            if (_keyframeAll || index <= 1) return;
            
            float a = curve.keys[index - 2].value;
            float b = curve.keys[index - 1].value;

            if (Mathf.Approximately(a, b) && Mathf.Approximately(b, value))
            {
                AnimationUtility.SetKeyRightTangentMode(curve, index - 2, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.Constant);
                curve.RemoveKey(index - 1);
            }
        }

        public void Initialize(GameObject target, Transform[] hierarchy, bool keyframeAll = true)
        {
            Transform root = target.transform;

            for (int i = 0; i < hierarchy.Length; i++)
            {
                var element = hierarchy[i];
                var parent = element.parent;
                
                string path = element.name;
                
                while (parent != null && parent != root)
                {
                    path = $"{parent.name}/{path}";
                    parent = parent.parent;
                }

                _animationFrames.Add(new AnimationFrame()
                {
                    boneReference = element,
                    path = path,
                    localPositionX = new AnimationCurve(),
                    localPositionY = new AnimationCurve(),
                    localPositionZ = new AnimationCurve(),
                    localRotationW = new AnimationCurve(),
                    localRotationX = new AnimationCurve(),
                    localRotationY = new AnimationCurve(),
                    localRotationZ = new AnimationCurve(),
                    localScaleX = new AnimationCurve(),
                    localScaleY = new AnimationCurve(),
                    localScaleZ = new AnimationCurve(),
                });
            }

            _keyframeAll = keyframeAll;
        }

        public void BakeAnimationFrame(float time)
        {
            // ReSharper Disable All
            
            foreach (var frame in _animationFrames)
            {
                Transform element = frame.boneReference;

                Quaternion normalizedRotation = element.localRotation.normalized;

                AddLinearKey(frame.localPositionX, time, element.localPosition.x);
                AddLinearKey(frame.localPositionY, time, element.localPosition.y);
                AddLinearKey(frame.localPositionZ, time, element.localPosition.z);
                
                AddLinearKey(frame.localRotationW, time, normalizedRotation.w);
                AddLinearKey(frame.localRotationX, time, normalizedRotation.x);
                AddLinearKey(frame.localRotationY, time, normalizedRotation.y);
                AddLinearKey(frame.localRotationZ, time, normalizedRotation.z);
                
                AddLinearKey(frame.localScaleX, time, element.localScale.x);
                AddLinearKey(frame.localScaleY, time, element.localScale.y);
                AddLinearKey(frame.localScaleZ, time, element.localScale.z);
            }
        }

        public void WriteToClip(AnimationClip clip)
        {
            foreach (var frame in _animationFrames)
            {
                clip.SetCurve(frame.path, typeof(Transform), "localPosition.x", frame.localPositionX);
                clip.SetCurve(frame.path, typeof(Transform), "localPosition.y", frame.localPositionY);
                clip.SetCurve(frame.path, typeof(Transform), "localPosition.z", frame.localPositionZ);
                
                clip.SetCurve(frame.path, typeof(Transform), "localRotation.x", frame.localRotationX);
                clip.SetCurve(frame.path, typeof(Transform), "localRotation.y", frame.localRotationY);
                clip.SetCurve(frame.path, typeof(Transform), "localRotation.z", frame.localRotationZ);
                clip.SetCurve(frame.path, typeof(Transform), "localRotation.w", frame.localRotationW);
                
                clip.SetCurve(frame.path, typeof(Transform), "localScale.x", frame.localScaleX);
                clip.SetCurve(frame.path, typeof(Transform), "localScale.y", frame.localScaleY);
                clip.SetCurve(frame.path, typeof(Transform), "localScale.z", frame.localScaleZ);
            }
        }

        public void WriteRootMotion(AnimationClip source, AnimationClip target)
        {
            var bindings = AnimationUtility.GetCurveBindings(source);

            foreach (var binding in bindings)
            {
                string propertyName = binding.propertyName.ToLower();
                
                if(!propertyName.Contains("roott") && !propertyName.Contains("rootq")) continue;
                
                AnimationCurve curve = AnimationUtility.GetEditorCurve(source, binding);
                target.SetCurve("", typeof(Animator), binding.propertyName, curve);
            }
        }
    }
}