﻿/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License; Version 2.0 (the );
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing; software
distributed under the License is distributed on an  BASIS;
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND; either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace umi3d.common
{
    /// <summary>
    /// Contains the keys for all available operations within the UMI3D protocol.
    /// </summary>
    /// Those keys are used when exchanging operations or request between the server and clients 
    /// to identify which process should be started on the receiving side.
    public static class UMI3DOperationKeys
    {
        public const uint Transaction = 1;
        public const uint LoadEntity = 2;
        public const uint DeleteEntity = 3;
        public const uint NavigationRequest = 4;
        public const uint TeleportationRequest = 5;
        public const uint UploadFileRequest = 6;
        public const uint GetLocalInfoRequest = 7;
        public const uint RedirectionRequest = 8;
        public const uint ForceLogoutRequest = 9;
        public const uint PlayPoseRequest = 10;
        public const uint ViewpointTeleportationRequest = 11;

        public const uint UserMicrophoneStatus = 21;
        public const uint UserAvatarStatus = 22;
        public const uint UserAttentionStatus = 23;
        public const uint MuteAllMicrophoneStatus = 24;
        public const uint MuteAllAvatarStatus = 25;
        public const uint MuteAllAttentionStatus = 26;

        public const uint SetEntityProperty = 101;
        public const uint SetEntityDictionnaryProperty = 103;
        public const uint SetEntityDictionnaryAddProperty = 104;
        public const uint SetEntityDictionnaryRemoveProperty = 105;
        public const uint SetEntityListProperty = 106;
        public const uint SetEntityListAddProperty = 107;
        public const uint SetEntityListRemoveProperty = 108;
        public const uint SetEntityMatrixProperty = 109;

        public const uint MultiSetEntityProperty = 110;

        public const uint UpdateBindingsActivation = 120;
        public const uint AddBinding = 121;
        public const uint RemoveBinding = 122;


        public const uint ProjectTool = 200;
        public const uint SwitchTool = 201;
        public const uint ReleaseTool = 202;

        public const uint SetUTSTargetFPS = 300;
        public const uint SetStreamedBones = 301;
        public const uint StartInterpolationProperty = 302;
        public const uint StopInterpolationProperty = 303;
        public const uint SetSendingCameraProperty = 304;
        public const uint SetSendingTracking = 305;
        public const uint FrameRequest = 306;
        public const uint FrameConfirmation = 307;
        public const uint SetUTSBoneTargetFPS = 308;

        public const uint FpsNavigationMode = 400;
        public const uint FlyingNavigationMode = 401;
        public const uint LayeredFlyingNavigationMode = 402;
        public const uint LockedNavigationMode = 403;

        public const uint InteractionRequest = 10001;
        public const uint EventStateChanged = 10002;
        public const uint EventTriggered = 10003;
        public const uint FormAnswer = 10004;
        public const uint Hoverred = 10005;
        public const uint HoverStateChanged = 10006;
        public const uint LinkOpened = 10007;
        public const uint ManipulationRequest = 10008;
        public const uint ParameterSettingRequest = 10009;
        public const uint ToolProjected = 10010;
        public const uint ToolReleased = 10011;

        public const uint UserCameraProperties = 10012;
        public const uint UserTrackingFrame = 10013;
        public const uint NotificationCallback = 10014;
        public const uint BoardedVehicleRequest = 10015;
        public const uint UserTrackingBone = 10016;

        public const uint EmoteRequest = 10020;

        public const uint ValidatePoseConditionRequest = 10030;
        public const uint CheckPoseAnimatorConditionsRequest = 10031;

        public const uint VolumeUserTransit = 10100;

        public const uint WebViewUrlRequest = 10200;
        public const uint WebViewSynchronizationRequest = 10201;

    }

    /// <summary>
    /// Contains the keys for all available parameter types within the UMI3D protocol.
    /// </summary>
    /// Those keys are used when exchanging operations or request between the server and clients 
    /// to identify what parameters to apply on processes started on the receiving side.
    public static class UMI3DParameterKeys
    {
        public const uint FloatRange = 1;
        public const uint IntRange = 2;
        public const uint Bool = 3;
        public const uint Float = 4;
        public const uint Int = 5;
        public const uint String = 6;
        public const uint StringUploadFile = 7;
        public const uint Enum = 8;
    }
}
