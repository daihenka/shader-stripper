using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    internal static class AssetDatabaseUtility
    {
        public static void AddObjectToUnityAsset(this Object objectToAdd, Object assetObject)
        {
            AssetDatabase.AddObjectToAsset(objectToAdd, assetObject);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(objectToAdd));
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        public static void RemoveObjectFromUnityAsset(this Object obj, string parentAssetPath)
        {
            if (!obj)
            {
                return;
            }

            if (AssetDatabase.GetAssetPath(obj) == parentAssetPath)
            {
                RemoveObjectFromUnityAsset(obj);
            }
        }

        public static void RemoveObjectFromUnityAsset(this Object obj)
        {
            if (!obj)
            {
                return;
            }

            AssetDatabase.RemoveObjectFromAsset(obj);
            Object.DestroyImmediate(obj, true);
        }

        public static void RemoveNestedObjectsFromUnityAsset(this object obj, string parentAssetPath, bool includeSelf = true)
        {
            if (obj == null)
            {
                return;
            }

            var fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var fieldInfo in fieldInfos)
            {
                if (!fieldInfo.IsPublic && fieldInfo.GetCustomAttribute<SerializeField>() == null)
                {
                    continue;
                }

                if (fieldInfo.FieldType.IsSubclassOf(typeof(Object)))
                {
                    var value = (Object) fieldInfo.GetValue(obj);
                    if (value)
                    {
                        value.RemoveNestedObjectsFromUnityAsset(parentAssetPath);
                    }
                }
                else if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>))))
                {
                    var type = fieldInfo.FieldType.GetGenericArguments()[0];
                    if (type.IsSubclassOf(typeof(Object)))
                    {
                        var collection = (IEnumerable) fieldInfo.GetValue(obj);
                        foreach (var value in collection)
                        {
                            value.RemoveNestedObjectsFromUnityAsset(parentAssetPath);
                        }
                    }
                }
            }

            if (includeSelf && obj is Object o)
            {
                o.RemoveObjectFromUnityAsset(parentAssetPath);
            }
        }
    }
}