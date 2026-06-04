using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.AI.Navigation.Samples
{
    /// <summary>
    /// Prefab spawner with a key input
    /// </summary>
    public class SpawnPrefabOnKeyDown : MonoBehaviour
    {
        [SerializeField]
        GameObject prefab;

        [SerializeField]
        Key key;

        [SerializeField]
        Transform spawnedPrefabsHolder;

        Transform m_Transform;

        void Start()
        {
            m_Transform = transform;

            if (spawnedPrefabsHolder == null)
            {
                spawnedPrefabsHolder = m_Transform;
            }
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || key == Key.None)
                return;

            if (keyboard[key]?.wasPressedThisFrame == true && prefab != null)
                Instantiate(prefab, m_Transform.position, m_Transform.rotation, spawnedPrefabsHolder);
        }
    }
}
