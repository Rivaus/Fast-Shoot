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

using BeardedManStudios.Forge.Networking.Frame;
using inetum.unityUtils;
using System.Collections.Generic;
using System.Threading;

namespace umi3d.common
{
    /// <summary>
    /// Bytes section in an array of bytes.
    /// </summary>
    public class ByteContainer
    {
        public ulong environmentId { get; private set; }

        public ulong timeStep { get; private set; }

        /// <summary>
        /// The byte array that the container section belongs to.
        /// </summary>
        public byte[] bytes { get; private set; }

        /// <summary>
        /// Starting index of the section in the <see cref="bytes"/> array.
        /// </summary>
        public int position;

        /// <summary>
        /// Number of byte in the section.
        /// </summary>
        public int length;

        public List<CancellationToken> tokens;

        public UMI3DVersion.Version version;


        private ByteContainer(ulong environmentId, UMI3DVersion.Version version)
        {
            tokens = new();
            this.environmentId = environmentId;
            this.version = version;
        }

        public ByteContainer(ulong environmentId, Binary frame, UMI3DVersion.Version version) : this(environmentId, frame.TimeStep, frame.StreamData.byteArr, version)
        {
        }

        public ByteContainer(ulong environmentId, ulong timeStep, byte[] bytes, UMI3DVersion.Version version) : this(environmentId, version)
        {
            this.timeStep = timeStep;
            this.bytes = bytes;
            position = 0;
            length = bytes.Length;
        }

        public ByteContainer(ByteContainer container) : this(container.environmentId, container.version)
        {
            this.bytes = container.bytes;
            position = container.position;
            length = container.length;
            timeStep = container.timeStep;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{bytes.ToString<byte>()} [{position} : {length}]";
        }
    }

    public class DtoContainer
    {
        public ulong environmentId { get; private set; }
        public AbstractOperationDto operation;
        public List<CancellationToken> tokens;

        private DtoContainer(ulong environmentId)
        {
            tokens = new();
            this.environmentId = environmentId;
        }

        public DtoContainer(ulong environmentId, AbstractOperationDto operation) : this(environmentId)
        {
            this.operation = operation;
        }
    }

}