/*
Copyright 2019 - 2024 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using UnityEngine;

namespace inetum.unityUtils
{
    public sealed class GlobalComponent : MonoBehaviour
    {
        [SerializeField] bool addRemoveWhenActivationChange;
        [SerializeField] MonoBehaviour[] variables;

        private void Awake()
        {
            if (!addRemoveWhenActivationChange)
            {
                foreach (var variable in variables)
                {
                    Global.Add(variable.GetType().FullName, variable);
                }
            }
        }

        private void OnEnable()
        {
            if (!addRemoveWhenActivationChange)
            {
                return;
            }

            foreach (var variable in variables)
            {
                Global.Add(variable.GetType().FullName, variable);
            }
        }

        private void OnDisable()
        {
            if (!addRemoveWhenActivationChange)
            {
                return;
            }

            foreach (var variable in variables)
            {
                Global.Remove(variable.GetType().FullName);
            }
        }
    }
}