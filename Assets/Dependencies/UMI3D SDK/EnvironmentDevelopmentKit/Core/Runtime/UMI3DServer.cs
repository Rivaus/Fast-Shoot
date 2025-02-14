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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Manager for the UMI3D server.
    /// </summary>
    /// Management of users is performed by the server.
    public class UMI3DServer : SingleBehaviour<UMI3DServer>, IUMI3DServer
    {
        /// <summary>
        /// IP of the UMI3D server.
        /// </summary>
        /// Default to "Localhost.
        [SerializeField, Tooltip("IP of the UMI3D server.")]
        protected string ip = "localhost";

        /// <summary>
        /// Initialize the server.
        /// </summary>
        public virtual void Init()
        {
            dataFullPath = Path.Combine(Application.dataPath, dataPath);
            dataFullPath = System.IO.Path.GetFullPath(dataFullPath);

            publicDataFullPath = Path.Combine(Application.dataPath, dataPath, publicDataPath);
            publicDataFullPath = System.IO.Path.GetFullPath(publicDataFullPath);

            privateDataFullPath = Path.Combine(Application.dataPath, dataPath, privateDataPath);
            privateDataFullPath = System.IO.Path.GetFullPath(privateDataFullPath);
        }

        #region ressources
        public static string dataPath = "../data/";
        public static string publicDataPath = "/public";
        public static string privateDataPath = "/private";
        private string publicDataFullPath;
        private string privateDataFullPath;
        private string dataFullPath;

        public static string publicRepository => Instance?.publicDataFullPath;
        public static string privateRepository => Instance?.privateDataFullPath;
        public static string dataRepository => Instance?.dataFullPath;

        public static bool IsInDataRepository(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            return fullPath.StartsWith(dataRepository);
        }

        public static bool IsInPrivateRepository(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            return fullPath.StartsWith(privateRepository);
        }

        public static bool IsInPublicRepository(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            return fullPath.StartsWith(publicRepository);
        }

        #endregion

        /// <summary>
        /// Return the URL of the HTTP Server.
        /// </summary>
        /// <returns></returns>
        public static string GetHttpUrl()
        {
            return Instance._GetHttpUrl();
        }

        protected virtual string _GetHttpUrl()
        {
            return ip;
        }

        public static string GetResourcesUrl()
        {
            return Instance._GetResourcesUrl();
        }

        protected virtual string _GetResourcesUrl()
        {
            return ip;
        }

        /*
        /// <summary>
        /// Return the Url of the websocket server.
        /// </summary>
        /// <returns></returns>
        static public string GetWebsocketUrl()
        {
            return Instance._GetWebsocketUrl();
        }

        protected virtual string _GetWebsocketUrl()
        {
            return ip;
        }*/

        /// <summary>
        /// Return a <see cref="EnvironmentConnectionDto"/> with essential info for connection. 
        /// Warning : returns null.
        /// </summary>
        /// <returns></returns>
        public virtual EnvironmentConnectionDto ToDto()
        {
            return null;
        }

        /// <summary>
        /// Notify that the user has changed.
        /// </summary>
        /// <param name="user"></param>
        public virtual void NotifyUserChanged(UMI3DUser user)
        {
        }

        /// <summary>
        /// Notify that the user has changed.
        /// </summary>
        /// <param name="user"></param>
        public virtual void NotifyUserRefreshed(UMI3DUser user)
        {
            OnUserRefreshed.Invoke(user);
        }

        /// <summary>
        /// Get the set of all <see cref="UMI3DUser"/> instances in the environment.
        /// </summary>
        /// <returns></returns>
        public virtual HashSet<UMI3DUser> UserSet()
        {
            return new HashSet<UMI3DUser>(UMI3DEnvironment.GetEntities<UMI3DUser>());
        }

        /// <summary>
        /// Get the set of all <see cref="UMI3DUser"/> instances in the environment that have already joined.
        /// </summary>
        /// <returns></returns>
        public virtual HashSet<UMI3DUser> UserSetWhenHasJoined()
        {
            return new HashSet<UMI3DUser>(UMI3DEnvironment.GetEntities<UMI3DUser>().Where((u) => u.hasJoined));
        }

        /// <summary>
        /// Get the collection of all <see cref="UMI3DUser"/> instances in the environment.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<UMI3DUser> Users()
        {
            return UMI3DEnvironment.GetEntities<UMI3DUser>();
        }

        /// <summary>
        /// Call to notify a user status change.
        /// </summary>
        /// <param name="user">User that get its status updated</param>
        /// <param name="status">New status</param>
        public virtual void NotifyUserStatusChanged(UMI3DUser user, StatusType status)
        {
            switch (status)
            {
                case StatusType.CREATED:
                    OnUserCreated.Invoke(user);
                    break;
                case StatusType.READY:
                    OnUserReady.Invoke(user);
                    break;
                case StatusType.AWAY:
                    OnUserAway.Invoke(user);
                    break;
                case StatusType.MISSING:
                    LookForMissing(user);
                    OnUserMissing.Invoke(user);
                    break;
                case StatusType.ACTIVE:
                    OnUserActive.Invoke(user);
                    break;
            }
        }

        /// <summary>
        /// Looks for a missing user.
        /// </summary>
        /// <param name="user">User to check the presence.</param>
        protected virtual void LookForMissing(UMI3DUser user) { }

        /// <summary>
        /// Send a <see cref="Transaction"/> to all clients.
        /// </summary>
        /// <param name="transaction"></param>
        public static void Dispatch(Transaction transaction)
        {
            if (Exists) Instance._Dispatch(transaction);
        }

        /// <summary>
        /// Send a <see cref="Transaction"/> to all clients.
        /// </summary>
        /// <param name="transaction"></param>
        public void DispatchTransaction(Transaction transaction)
        {
            _Dispatch(transaction);
        }

        //? empty ?
        protected virtual void _Dispatch(Transaction transaction)
        {
        }

        /// <summary>
        /// Get the current server Unity time in seconds since it has started.
        /// </summary>
        /// <returns></returns>
        public virtual float ReturnServerTime()
        {
            return Time.time;
        }

        #region session
        public UMI3DUserEvent OnUserJoin { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserRefreshed { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserRegistered { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserCreated { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserRecreated { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserReady { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserAway { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserMissing { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserActive { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserLeave { get; protected set; } = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserUnregistered { get; protected set; } = new UMI3DUserEvent();
        #endregion
    }
}