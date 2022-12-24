using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TG
{
    public class UpdatableData : ScriptableObject
    {
        [SerializeField]
        public bool m_autoUpdate;

        public event Action OnUpdateData;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_autoUpdate)
            {
                EditorApplication.update += AppriseOnValuesUpdated;
                EditorSceneManager.sceneSaved += (scene) => AppriseOnValuesUpdated();
            }
        }

        public void AppriseOnValuesUpdated()
        {
            EditorApplication.update -= AppriseOnValuesUpdated;
            EditorSceneManager.sceneSaved -= (scene) => AppriseOnValuesUpdated();
            OnUpdateData?.Invoke();
        }
#endif
    }
}