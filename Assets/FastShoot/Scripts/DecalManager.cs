using inetum.unityUtils;
using System.Collections;
using System.Collections.Generic;
using umi3d.edk;
using UnityEngine;

namespace com.quentintran.server.shoot
{
    internal class DecalManager : SingleBehaviour<DecalManager>
    {
        [SerializeField]
        private GameObject bulletTemplate = null;

        [SerializeField]
        private GameObject bloodTemplate = null;

        [SerializeField]
        private UMI3DScene decalScene = null;

        [SerializeField]
        private int poolNumber = 50;

        [SerializeField]
        private float decalDisplayTime = 5f;

        private PoolObject bullet, blood;

        protected override void Awake()
        {
            base.Awake();

            if (bulletTemplate is not null)
                bullet = new PoolObject(bulletTemplate, decalDisplayTime, decalScene.transform, poolNumber, this);

            if (bloodTemplate is not null)
                blood = new PoolObject(bloodTemplate, decalDisplayTime, decalScene.transform, poolNumber, this);
        }

        internal void DisplayBulletDecal(Vector3 position, Vector3 normal)
        {
            bullet.DisplayObject(position + normal * .005f, position);
        }

        internal void DisplayBloodDecal(Vector3 position, Vector3 normal)
        {
            blood.DisplayObject(position + normal * .1f, position);
        }
    }

    internal class PoolObject
    {
        private Queue<UMI3DModel> objectQueue = new();
        private List<UMI3DModel> useObjectQueue = new();

        private float displayTime;

        private Dictionary<UMI3DModel, Coroutine> coroutineByObjects = new();

        private MonoBehaviour mono;

        internal PoolObject(GameObject template, float displayTime, Transform root, int size, MonoBehaviour mono)
        {
            this.displayTime = displayTime;
            this.mono = mono;

            for (int i = 0; i < size; i++)
            {
                GameObject obj = GameObject.Instantiate(template, root);
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;

                var model = obj.GetComponent<UMI3DModel>();
                objectQueue.Enqueue(model);

                coroutineByObjects[model] = null;
            }
        }

        internal void DisplayObject(Vector3 position, Vector3 lookAt)
        {
            UMI3DModel obj = null;

            if (objectQueue.Count > 0)
            {
                obj = objectQueue.Dequeue();
            }
            else
            {
                obj = useObjectQueue[0];
                useObjectQueue.RemoveAt(0);

                if (coroutineByObjects[obj] != null)
                {
                    mono.StopCoroutine(coroutineByObjects[obj]);
                    coroutineByObjects[obj] = null;
                }
            }

            useObjectQueue.Add(obj);

            obj.transform.position = position;
            obj.transform.LookAt(lookAt);

            Transaction t = new Transaction { reliable = true };
            t.AddIfNotNull(obj.objectActive.SetValue(true));
            t.AddIfNotNull(obj.objectPosition.SetValue(obj.transform.localPosition));
            t.AddIfNotNull(obj.objectRotation.SetValue(obj.transform.localRotation));
            t.Dispatch();

            coroutineByObjects[obj] = mono.StartCoroutine(HideObject(obj));
        }

        private IEnumerator HideObject(UMI3DModel model)
        {
            yield return new WaitForSeconds(displayTime);

            coroutineByObjects[model] = null;

            Transaction t = new Transaction { reliable = true };
            t.AddIfNotNull(model.objectActive.SetValue(false));
            t.AddIfNotNull(model.objectPosition.SetValue(Vector3.zero));
            t.Dispatch();

            objectQueue.Enqueue(model);
        }
    }
}