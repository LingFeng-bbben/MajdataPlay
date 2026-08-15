using Unity.VisualScripting;
using UnityEngine;

class VariableExamples
{
    #region PlayerController
    public class PlayerController : MonoBehaviour
    {
        VariableDeclaration m_Velocity;
        void Start()
        {
            var variables = GetComponent<Variables>();
            m_Velocity = variables.declarations.GetDeclaration("velocity");
        }
        void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                var currentVelocity = (float)m_Velocity.value;
                m_Velocity.value = currentVelocity * 2f;
            }
        }
    }
    #endregion
}
