using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace EnvironmentSpawnerNamespace
{
    public class EnvironmentSpawner : MonoBehaviour
    {
        [Tooltip("Размер области действия скрипта. На всё что находится под синим квадратом может быть установлен объект.\r\n\nThe size of the scope of the script. An object can be installed on everything under the blue square.")]
        [SerializeField] Vector2 dimensions = new Vector2(2, 2);

        [Tooltip("Устанавливает это имя для объекта. Если переименование не требуется- оставить это поле пустым.\r\n\nSets this name for the object. If renaming is not required, leave this field empty.")]
        [SerializeField] string nameOfInstancignObjects;

        [Tooltip("Добавить ли к имени установленного объекта его ID?\r\n\nShould I add its ID to the name of the installed object?")]
        [SerializeField] bool shouldAddIDToNameOfInstalledObject=false;

        [Tooltip("Сколько объектов нужно установить при нажатии кнопки  \"Generate\".\r\n\nHow many objects should be installed when the \"Generate\" button is clicked.")]
        [SerializeField] uint numberOfSpawnedObjects = 0;

        [Tooltip("Префабы объектов, которые будут устанавливаться скриптом. Для каждой установки будет браться случайный объект из списка.\r\n\nPrefabs of objects that will be installed by the script. A random object from the list will be taken for each installation.")]
        [SerializeField] GameObject[] prefabs;

        [Tooltip("Объект находится в \"техническом\" примитива типа \"Box\", который нужен только для правильной установки этого объекта через этот скрипт и который должен быть удалён? Примитив должен иметь название \"TechnicalBox\". \r\n\n Is the object located in a \"technical\" primitive of the \"Box\" type, which is needed only for the correct installation of this object through this script and which should be deleted? The primitive must have the name \"TechnicalBox\".")]
        [SerializeField] bool objectInBox = false;

        [Tooltip("Объект находится в \"техническом\" BoxCollider , который нужен только для правильной установки им через этот скрипт и который должен быть удалён? Для правильной работы скрипта на после удаления \"техническом\" BoxCollider всё равно должен остаться хоть один Collider .\r\n\nIs the object in the \"technical\" BoxCollider, which is only needed to install it correctly through this script and which should be deleted? For the script to work correctly, after deleting the \"technical\" BoxCollider, at least one Collider should still remain.")]
        [SerializeField] bool objectInBoxCollider = false;

        [Tooltip("При установке объекта скрипт может задавать ему рандомный размер между этими двумя значениями. Если это не нужно сделайте эти 2 числа одинаковыми.\r\n\nWhen installing an object, the script can set it a random size between these two values. If it is not necessary, make these 2 numbers the same.")]
        [SerializeField] Vector2 scaleVariation = new Vector2(0.5f, 1.5f);

        [Tooltip("Отклонение по оси \"X\" которое нужно чтоб объект устанавливался вертикально а не лежал на боку.\r\n\nThe deviation along the \"X\" axis, which is necessary so that the object is installed vertically and not lying on its side.")]
        [SerializeField] float Xrotation;

        [Tooltip("При установке объекта скрипт может задавать ему рандомный угол поворота между этими двумя значениями. Если это не нужно сделайте эти 2 числа одинаковыми.\r\n\nWhen installing an object, the script can set it a random rotation angle between these two values. If it is not necessary, make these 2 numbers the same.")]
        [SerializeField] Vector2 rotationVariation = new Vector2(0, 360);

        [Tooltip("Слои которые будут игнорировать Raycast. Raycast просто будет проходить сквозь объекты с выбранными здесь слоями.\r\n\nLayers that will ignore Raycast. The Raycast will simply pass through the objects with the layers selected here.")]
        [SerializeField] LayerMask ignoreLayer;

        [Tooltip("Объект не может быть установлен если будет касаться любых объектов с выбранными слоями.\r\n\nThe object cannot be installed if it touches any objects with selected layers.")]
        [SerializeField] LayerMask unwantedLayers;

        [Tooltip("Объект не может быть установлен если будет касаться любых объектов с выбранными Tags.\r\n\nThe object cannot be installed if it touches any objects with the selected Tags.")]
        [SerializeField] string[] unwantedTags;

        [Tooltip("При установке объекта назначает ему родителем указанный тут объект. Если это поле останется пустым- родителем устанавливаемому объекту будет назначен объект с этим скриптом.\r\n\nWhen installing an object, assigns it the parent of the object specified here. If this field remains empty, the object with this script will be assigned as the parent of the object to be installed.")]
        [SerializeField] GameObject parentObject;

        [Tooltip("Список объектов которые были установлены этим скриптом.\r\n\nThe list of objects that were installed by this script.")]
        [SerializeField] List<GameObject> spawnedObjects;

        //рисет квадрат вокруг области в которой скрипт будет размещать обьекты
        //draws a square around the area where the script will place objects
        void OnDrawGizmos()
		{
			Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
			Gizmos.matrix = rotationMatrix;

			Gizmos.color = new Color(0, 0, 1);
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(dimensions.x * transform.localScale.x, 0.1f, dimensions.y * transform.localScale.z));
			Gizmos.color = new Color(0, 0, 1, 0.1f);
			Gizmos.DrawCube(Vector3.zero, new Vector3(dimensions.x * transform.localScale.x, 0.1f, dimensions.y * transform.localScale.z));
		}

        //метод, который позволяет размещать объекты в окне инспектора без запуска Игрового Мода.
        //Если объекты размещаются в неправильном месте 254 раза- выполнение метода заканчивается
        //a method that allows you to place objects in the inspector window without launching a Game Mod.
        //If objects are placed in the wrong place 254 times, the execution of the method ends
        public void GenerateEnvironment_Main()
        {
            if (SettingsChek())
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
                            instObj.name = nameOfInstancignObjects;
                        //else
                        //{
                        //    instObj.name = instObj.name + " " + instObj.GetInstanceID();
                        //}
                        if (shouldAddIDToNameOfInstalledObject == true)
                            instObj.name = instObj.name + " " + instObj.GetInstanceID();
                    }
                }
            }
        }

        //сама функция, которая делает все действия по установке, масштабированию и всему необходимому с устанавливаемым обьектом.
        //the function itself, which does all the installation, scaling and everything necessary with the installed object.
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
            //GameObject instantiateObject = PrefabUtility.InstantiatePrefab(prefabs[indexOfSpawnedObject] as GameObject);
            instantiateObject.name = prefabs[indexOfSpawnedObject].name;

            ScaleObject(ref instantiateObject);

            RotateObject(ref instantiateObject);

            SetParent(ref instantiateObject);

            //if (instantiateObject.GetComponent<BoxCollider>())
                if (OverlapBox(instantiateObject))
                {
                    DestroyImmediate(instantiateObject);
                    return null;
                }

            if (objectInBoxCollider)
                DestroyTechnicalBoxCollider( ref instantiateObject);

            if (objectInBox)
                DestroyTechnicalBox(instantiateObject);

            return instantiateObject;
        }

        /*Старая версия этого метода
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

        //функция вызываемая уничтожаемым объектом, чтобы скрипт мог удалить его из  списков установленных им объектов.
        //a function called by the object being destroyed so that the script can remove it from the lists of objects it has installed.
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

        //функция которая удалит висящий на объекте "технический" Box Collider.
        //a function that will remove the "technical" Box Collider hanging on the object.
        private void DestroyTechnicalBoxCollider(ref GameObject meshReplacementObject)
        {
            DestroyImmediate(meshReplacementObject.GetComponent<BoxCollider>());
        }
        private void DestroyTechnicalBox(GameObject objWithTechnicalBox)
        {
            DestroyImmediate(objWithTechnicalBox.transform.Find("TechnicalBox").gameObject);
        }

        //функция удаляет все Child объекты у выбранного объекта
        //the function deletes all Child objects from the selected object
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
            //Bounds bound = instantiateObject.GetComponent<BoxCollider>().bounds;
            Bounds bound;
            if (objectInBox==false)
            {
                if (instantiateObject.GetComponent<MeshRenderer>() != null)
                    bound = instantiateObject.GetComponent<MeshRenderer>().bounds;

                else if (instantiateObject.GetComponent<MeshCollider>() != null)
                    bound = instantiateObject.GetComponent<MeshCollider>().bounds;

                else if (instantiateObject.GetComponent<BoxCollider>() != null)
                    bound = instantiateObject.GetComponent<BoxCollider>().bounds;

                else
                {
                    Debug.LogError("Для правильной работы скрипта на родительском объекте которые нужно установить должен быть либо BoxCollider, либо MeshCollider, либо MeshRenderer.\n" +
                        "For the script to work correctly, either BoxCollider, MeshCollider, or MeshRenderer must be installed on the parent object.");
                    return true;
                }
            }
            else
            {
                Transform technicalBox = instantiateObject.transform.Find("TechnicalBox");
                //if (instantiateObject.transform.Find("TechnicalBox") == null)
                if (technicalBox == null)
                    {
                    Debug.LogError("В устанавливаемом объекте не получилось найти объект с именем \"TechnicalBox\". Проверьте точно ли он существует. \r\n An object named \"TechnicalBox\" could not be found in the object being installed. Check if it exists for sure.");
                    return true;
                }
                else
                {
                    MeshRenderer technicalBoxMeshRenderer = technicalBox.GetComponent<MeshRenderer>();
                    if (technicalBoxMeshRenderer == null)
                    {
                        Debug.LogError("В объекте \"TechnicalBox\" не получилось найти MeshRenderer. Проверьте точно ли он существует. \r\n The MeshRenderer could not be found in the \"TechnicalBox\" object. Check if it exists for sure. ");
                        return true;
                    }
                    else
                        bound = technicalBoxMeshRenderer.bounds;
                }
            }

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

        //функция масштабирования устанавливаемого объекта
        //the scaling function of the object being installed
        private void ScaleObject(ref GameObject instantiateObject)
		{
			float _scale = Random.Range(scaleVariation.x, scaleVariation.y);
			instantiateObject.transform.localScale = new Vector3(_scale, _scale, _scale);
		}

        //функция поворачивания устанавливаемого объекта
        //the function of rotating the object to be installed
        private void RotateObject(ref GameObject instantiateObject)
		{
			float yRotations = Random.Range(rotationVariation.x, rotationVariation.y);
			instantiateObject.transform.Rotate(0, yRotations, 0, Space.Self);
            if (Xrotation != 0)
            {
                instantiateObject.transform.Rotate(Xrotation, yRotations, 0, Space.Self);
            }
            else 
            {
                instantiateObject.transform.Rotate(0, yRotations, 0, Space.Self);
            }
		}

        //функция установки родителя устанавливаемого объекта
        //the function of setting the parent of the object to be installed
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

        //функция проверки слоя, в который ударился Raycast() чтобы понять можно ли тут устанавливать объекты
        //the function of checking the layer that Raycast() hit to see if it is possible to install objects here
        private bool LayerComparison(LayerMask layer)
		{
			if (((1 << layer) & unwantedLayers) != 0)
				return true;
			else
				return false;
		}

        //функция проверки Tag объекта, в который ударился Raycast() чтобы понять можно ли тут устанавливать объекты
        //the function of checking the Tag of the object that Raycast() hit to see if it is possible to install objects here
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

        //функция выстрела RaycastHit, которая выбирает рандомное место в области действия скрипта EnvironmentSpawner()
        //и возвращает полкчившуюся точку.
        //The RaycastHit shot function, which selects a random location in the scope of the EnvironmentSpawner()
        //script and returns the broken point.
        private RaycastHit Raycast()
		{
			RaycastHit hit;
			Vector3 randomRaycastPoint = GenerateRandomRaycastPoint();
			Physics.Raycast(transform.position + transform.right * randomRaycastPoint.x + transform.up * randomRaycastPoint.y + transform.forward * randomRaycastPoint.z, Vector3.down, out hit, Mathf.Infinity, ~ignoreLayer);
			return hit;			
		}

        //функция создания  места откуда будет стреляться райкаст.
        //the function of creating a place from where the raycast will be shot.
        private Vector3 GenerateRandomRaycastPoint()     
		{
			Vector3 rayPos = new Vector3(Random.Range(-dimensions.x / 2 * transform.localScale.x, dimensions.x / 2 * transform.localScale.z)
			, 0
			, Random.Range(-dimensions.y / 2 * transform.localScale.x, dimensions.y / 2 * transform.localScale.z));
			return rayPos;
		}
        private bool SettingsChek()
        {
            if (objectInBox && objectInBoxCollider)
            {
                Debug.LogError("Твои настройки не правильные. Проверьте настройки. Your settings are not correct. Check the settings.\r\n objectInBox && objectInBoxCollider==true");
                return false;
            }
            else
                return true;
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
