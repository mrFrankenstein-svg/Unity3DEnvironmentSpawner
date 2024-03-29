using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

namespace EnvironmentSpawnerNamespace
{
    public class EnvironmentSpawner : MonoBehaviour
    {

        [SerializeField] uint numberOfSpawnedObjects = 0;
        [SerializeField] List<GameObject> spawnedObjects;
        [SerializeField] Vector2 scaleVariation = new Vector2(0.5f, 1.5f);
        [SerializeField] Vector2 rotationVariation = new Vector2(0, 360);
        [SerializeField] LayerMask ignoreLayer;
        [SerializeField] LayerMask unwantedLayers;
        [SerializeField] string[] unwantedTags;
        [SerializeField] GameObject parentObject;
        [SerializeField] bool changeMeshAfterInstallingObject = false;
        [SerializeField] string nameOfInstancignObjects;
        [SerializeField] GameObject[] prefabs;
        [SerializeField] Vector2 dimensions = new Vector2(2, 2);

        void OnDrawGizmos()
		{
			Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
			Gizmos.matrix = rotationMatrix;

			Gizmos.color = new Color(0, 0, 1);
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(dimensions.x * transform.localScale.x, 0.1f, dimensions.y * transform.localScale.z));
			Gizmos.color = new Color(0, 0, 1, 0.1f);
			Gizmos.DrawCube(Vector3.zero, new Vector3(dimensions.x * transform.localScale.x, 0.1f, dimensions.y * transform.localScale.z));

		}
        public void GenerateEnvironment_Main()
        {
            byte mistakes = 0;
            uint maxSpawnObjects = numberOfSpawnedObjects - (uint)spawnedObjects.Count;

            for (; maxSpawnObjects > 0;)
            {
                if (mistakes >= 254)
                    break;
                GameObject instObj = GenerateEnvironment_Slave();
                if (instObj == null)
                {
                    mistakes++;
                }
                else
                {
                    spawnedObjects.Add(instObj);
                    mistakes = 0;
                    maxSpawnObjects--;

                    instObj.AddComponent<OnDestroyScript>();
                    instObj.GetComponent<OnDestroyScript>().SetSpawner(this);

                    if (nameOfInstancignObjects != "")
                        instObj.name = nameOfInstancignObjects + " " + instObj.GetInstanceID();
                    else
                    {
                        instObj.name = instObj.name + " " + instObj.GetInstanceID();
                    }
                }
            }
        }

        private GameObject GenerateEnvironment_Slave()
        {
            RaycastHit hit = Raycast();

            if (LayerComparison(hit.transform.gameObject.layer))
            {
                return null;
            }

            if (TagComparison(hit.transform))
            {
                return null;
            }

            int indexOfSpawnedObject = Random.Range(0, prefabs.Length);
            GameObject instantiateObject = Instantiate(prefabs[indexOfSpawnedObject], hit.point, Quaternion.identity);
            instantiateObject.name = prefabs[indexOfSpawnedObject].name;

            ScaleObject(ref instantiateObject);

            RotateObject(ref instantiateObject);

            SetParent(ref instantiateObject);

            if (instantiateObject.GetComponent<BoxCollider>())
                if (OverlapBox(instantiateObject))
                {
                    DestroyImmediate(instantiateObject);
                    return null;
                }

            if (changeMeshAfterInstallingObject)
                ChacgeMesh(indexOfSpawnedObject, ref instantiateObject);

            return instantiateObject;
        }
        /*������ ������ ����� ������
        Old version of this method
        public void GenerateEnvironment()
        {

            byte mistakes = 0;
            uint maxSpawnObjects = numberOfSpawnedObjects;

            for (; numberOfSpawnedObjects > 0;)
            {
                if (mistakes >= 254)
                    break;

                RaycastHit hit = Raycast();


                if (LayerComparison(hit.transform.gameObject.layer))
                {
                    mistakes++;
                    continue;
                }

                if (TagComparison(hit.transform))
                {
                    mistakes++;
                    continue;
                }

                int indexOfSpawnedObject = Random.Range(0, prefabs.Length);
                GameObject instantiateObject = Instantiate(prefabs[indexOfSpawnedObject], hit.point, Quaternion.identity);

                ScaleObject(ref instantiateObject);

                RotateObject(ref instantiateObject);

                SetParent(ref instantiateObject);

                if (instantiateObject.GetComponent<BoxCollider>())
                    if (OverlapBox(instantiateObject))
                    {
                        DestroyImmediate(instantiateObject);
                        mistakes++;
                        continue;
                    }

                if (changeMeshAfterInstallingObject)
                    ChacgeMesh(indexOfSpawnedObject, ref instantiateObject);


                if (nameOfInstancignObjects != "")
                    instantiateObject.name = (maxSpawnObjects - numberOfSpawnedObjects) + " " + nameOfInstancignObjects;
                else
                    instantiateObject.name = (maxSpawnObjects - numberOfSpawnedObjects) + " " + prefabs[indexOfSpawnedObject].name;

                mistakes = 0;
                numberOfSpawnedObjects--;
            }
        }
        */

        public void ThisObjectHasBeenDestroyed(GameObject destroyedObj)
        {
            for (int i = 0; i < spawnedObjects.Count ; i++)
            {
                if (spawnedObjects[i] == null || spawnedObjects[i] == destroyedObj)
                {
                    spawnedObjects.RemoveAt(i); 
                    break;
                }
            }
        }
        private void ChacgeMesh(int indexOfMesh, ref GameObject meshReplacementObject)
		{
			DestroyImmediate(meshReplacementObject.GetComponent<BoxCollider>());
			MeshCollider meshCollider= meshReplacementObject.AddComponent<MeshCollider>();
		}
		public void DestrouAllInParentObject()
        {
            List<Transform> ret = new List<Transform>();
            foreach (Transform child in parentObject.transform) 
				ret.Add(child);

            foreach (var item in ret)
				DestroyImmediate(item.gameObject);

            spawnedObjects.Clear();

        }
        private bool OverlapBox(GameObject instantiateObject)
        {
			Bounds bound = instantiateObject.GetComponent<MeshRenderer>().bounds;
			instantiateObject.SetActive(false);

			//Collider[] leaningColliders = Physics.OverlapBox(centerPointOfObject, col.size, instantiateObject.transform.rotation);
			Collider[] leaningColliders = Physics.OverlapBox(bound.center, bound.size/2, instantiateObject.transform.rotation);


			instantiateObject.SetActive(true);

			foreach (var collider in leaningColliders)
            {
				if (LayerComparison(collider.transform.gameObject.layer))
					return true;

				if (TagComparison(collider.transform))
					return true;
			}
			return false;
        }
        private void ScaleObject(ref GameObject instantiateObject)
		{
			float _scale = Random.Range(scaleVariation.x, scaleVariation.y);
			instantiateObject.transform.localScale = new Vector3(_scale, _scale, _scale);
		}			
		private void RotateObject(ref GameObject instantiateObject)
		{
			float yRotations = Random.Range(rotationVariation.x, rotationVariation.y);
			instantiateObject.transform.Rotate(0, yRotations, 0, Space.Self);
		}
        private void SetParent(ref GameObject instantiateObject)
		{
            if (parentObject)
            {
                instantiateObject.transform.SetParent(parentObject.transform);
            }
            else
            {
                instantiateObject.transform.SetParent(gameObject.transform);
            }
        }
		private bool LayerComparison(LayerMask layer)
		{
			if (((1 << layer) & unwantedLayers) != 0)
				return true;
			else
				return false;
		}

		private bool TagComparison(Transform taggedObject)
		{
			if (unwantedTags.Length != 0)
			{
				foreach (var item in unwantedTags)
				{
					if (taggedObject.tag == item)
					{
						return true;
					}
				}
			}
			else
			{
				return false;
			}
			return false;
		}
		private RaycastHit Raycast()
		{
			RaycastHit hit;
			Vector3 randomRaycastPoint = GenerateRandomRaycastPoint();
			Physics.Raycast(transform.position + transform.right * randomRaycastPoint.x + transform.up * randomRaycastPoint.y + transform.forward * randomRaycastPoint.z, Vector3.down, out hit, Mathf.Infinity, ~ignoreLayer);
			return hit;			
		}
		private Vector3 GenerateRandomRaycastPoint()     //������ ����� ������ ����� ��������� �������
		{
			//get flat plane positions for each population id
			Vector3 rayPos = new Vector3(Random.Range(-dimensions.x / 2 * transform.localScale.x, dimensions.x / 2 * transform.localScale.z)
			, 0
			, Random.Range(-dimensions.y / 2 * transform.localScale.x, dimensions.y / 2 * transform.localScale.z));
			return rayPos;
		}

		private void COLLIDERSDEBUG(Collider[] col, string objName)
		{
            foreach (var item in col)
            {
				objName += objName +" "+ item.name;
			}
			Debug.Log(objName);
		}
	}
}
