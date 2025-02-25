﻿/*
Copyright 2019 - 2021 Inetum

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

using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Request to teleport a user. Adds rotation compared to a <see cref="NavigationRequest"/>.
    /// </summary>
    public class ViewpointTeleportRequest : TeleportRequest
    {
        public ViewpointTeleportRequest(Vector3 position, Quaternion rotation) : base(position, rotation)
        {

        }

        /// <inheritdoc/>
        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.ViewpointTeleportationRequest;
        }

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            return base.ToBytable(user);
        }

        /// <inheritdoc/>
        protected override NavigateDto CreateDto() { return new ViewpointTeleportDto(); }

        /// <inheritdoc/>
        protected override void WriteProperties(NavigateDto dto)
        {
            base.WriteProperties(dto);
            //if (dto is ViewpointTeleportDto tpDto)
            //    tpDto.rotation = rotation;
        }

    }
}